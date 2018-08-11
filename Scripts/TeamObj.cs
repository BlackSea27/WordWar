using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Aionline.WordsCities
{
    public class TeamObj : Photon.MonoBehaviour //, IPunObservable
    {
        #region Public Vars        
        public TeamObj targetTeam { set; get; }
        public CityObj targetCity { set; get; }

        //工程队伍变量
        //工人队要建造的配件表
        public string[] partsnames { get; set; }
        //提示模型
        public Transform hintModel { set; get; }
        //提示模型闪烁协程
        public Coroutine coroutineHintModelFlash;
        //去往指定位置建造协程
        public Coroutine coroutineGoAndBuild;
        //逐步建造协程
        public Coroutine coroutineStepBuild;
        //空军队伍跟踪协程
        public Coroutine coroutineFollow;
        //转向面对目标协程
        public Coroutine coroutineTurnToTarget;
        //队伍特殊属性列表
        public List<WorldObj.TeamFeature> features;

        //队伍属性
        public WorldObj.Nationality nationality = WorldObj.Nationality.none;// { set; get; }
        public int soldiernumber;// { set; get; }
        public WorldObj.SoldierType soldiertype;// { set; get; }
        //装载型队伍（目前如TANK）的所装载的队伍的士兵类型
        public WorldObj.SoldierType driversoldiertype;
        public string hero;// { set; get; }
        public float teamspeed;//{ set; get; }
        public int teammorale;// { set; get; }

        //是否锁定状态
        public bool locking = false;//{ set; get; }
        //是否战争状态
        public bool fighting = false;//{ set; get; }
        //是否可视
        public bool visible = false;//{ set; get; }
        //正在交战列表
        public List<string> enemies;

        public Transform teamHealthCanvas;//队伍血条
        #endregion


        #region Private Vars
        float cooltimeTeamFight = 0;
        float cooltimeCityFight = 0;
        float cooltime = 1.5f;
        bool death = false;
        AnimatorStateInfo teamAnimatorState; //动画状态
        float moving; //移动速度
        float attackDistance; //攻击距离

        int cityHighestForce = 0; //城市 最高（总）的英雄武力值
        CityObj[] cityarray;//所有城市数组
        HeroObj heroSelf;//带队英雄
        TeamObj findTeam;//野怪或塔发现的队伍
        TeamObj nowTargetTeam;//改变目标前的在用的目标
        CityObj nowTargetCity;//改变目标前的在用的目标

        UIController uiController;
        TeamController teamController;//队伍控制组件
        Animator teamAnimator;//队伍动画组件
        NavMeshAgent teamNavMeshAgent;//队伍导航组件
        Slider teamSlider;//队伍血条组件
        
        string[] actions = Enum.GetNames(typeof(WorldObj.AnimatorAction));//队伍动画动作的枚举键名称数组

        #endregion

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(this);

            if (photonView.isMine)
            {
                //添加该队伍到所有队伍列表
                AddToTeams();
                visible = true;
                if (nationality == WorldObj.Nationality.monster)
                {
                    visible = false;
                }
            }            

            //获取本国的teamController组件和uiController组件
            CountryObj selfCountry = WorldObj.MatchCountry(nationality);
            if (selfCountry)
            {
                teamController = selfCountry.GetComponent<TeamController>();
                uiController = selfCountry.GetComponent<UIController>();
            }
            teamAnimator = GetComponentInChildren<Animator>();
            teamNavMeshAgent = GetComponent<NavMeshAgent>();

            //初始化队伍特征
            GetTeamFeatures();

            //初始化血条
            teamSlider = teamHealthCanvas.Find("Slider").GetComponent<Slider>();
            teamSlider.maxValue = soldiernumber;
            teamSlider.value = teamSlider.maxValue;
            Image teamSliderFillImage = teamSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
            switch (nationality)
            {
                case WorldObj.Nationality.none:
                    break;
                case WorldObj.Nationality.red:
                    teamSliderFillImage.color = new Color(0.6f, 0f, 0f);
                    break;
                case WorldObj.Nationality.blue:
                    teamSliderFillImage.color = new Color(0f, 0.5f, 1f);
                    break;
                case WorldObj.Nationality.monster:
                    teamSliderFillImage.color = new Color(0f, 0.6f, 0f);
                    break;
                default:
                    break;
            }
            
            if (features.Contains(WorldObj.TeamFeature.AirForce))
            {
                teamSlider.transform.localPosition = new Vector3(0f, 1f, -3f);
            }
            else
            {
                teamSlider.transform.localPosition = new Vector3(0f, 1f, -5f);
            }

            //拆分血条           
            teamHealthCanvas.SetParent(null);

            //攻击距离计算
            attackDistance = WorldObj.AttackDistance;
            switch (soldiertype)
            {
                case WorldObj.SoldierType.SwordMan:
                    break;
                case WorldObj.SoldierType.Archer:
                    attackDistance *= 1.7f; //弓箭手射程是普通的1.7倍
                    break;
                case WorldObj.SoldierType.Cavalry:
                    break;
                case WorldObj.SoldierType.Tank:
                    attackDistance *= 1.7f; 
                    break;
                case WorldObj.SoldierType.Griffin:
                    attackDistance *= 2f; //Griffin射程是普通的2f
                    break;
                default:
                    break;
            }

            //己方英雄赋值
            heroSelf = WorldObj.MatchHero(hero);
            //初始化塔或野怪发现的队伍
            findTeam = null;

            //originalSoldierNumber = soldiernumber;

            //该队是塔则直接返回
            if (features.Contains(WorldObj.TeamFeature.Tower))
            {
                return;
            }

            //初始化队伍速度
            switch (soldiertype)
            {
                case WorldObj.SoldierType.none:
                    UpdateTeamSpeed(WorldObj.SwordManSpeed);                    
                    break;
                case WorldObj.SoldierType.SwordMan:
                    UpdateTeamSpeed(WorldObj.SwordManSpeed);
                    break;
                case WorldObj.SoldierType.Archer:
                    UpdateTeamSpeed(WorldObj.ArcherSpeed);
                    break;
                case WorldObj.SoldierType.Cavalry:
                    UpdateTeamSpeed(WorldObj.CavalrySpeed);
                    break;
                case WorldObj.SoldierType.Worker:
                    UpdateTeamSpeed(WorldObj.WorkerSpeed);
                    break;
                default:
                    break;
            }

            //该队是空军则直接返回
            if (features.Contains(WorldObj.TeamFeature.AirForce))
            {
                return;
            }

            UpdateTeamNAVSpeed(teamspeed);
            //teamNavMeshAgent.speed = teamspeed;
            UpdateTeamNAVStopingDistance(WorldObj.AttackDistance - 1f);
        }
        /// <summary>
        /// 初始化队伍特征
        /// </summary>
        void GetTeamFeatures()
        {
            features = new List<WorldObj.TeamFeature>();
            switch (soldiertype)
            {
                case WorldObj.SoldierType.none:
                    break;
                case WorldObj.SoldierType.SwordMan:
                    features.Add(WorldObj.TeamFeature.Army);
                    features.Add(WorldObj.TeamFeature.AttackGround);
                    break;
                case WorldObj.SoldierType.Archer:
                    features.Add(WorldObj.TeamFeature.Army);
                    features.Add(WorldObj.TeamFeature.AttackGround);
                    features.Add(WorldObj.TeamFeature.AttackAir);
                    break;
                case WorldObj.SoldierType.Cavalry:
                    features.Add(WorldObj.TeamFeature.Army);
                    features.Add(WorldObj.TeamFeature.AttackGround);
                    break;
                case WorldObj.SoldierType.Tank:
                    List<string> partsNames = new List<string>(partsnames);
                    if (partsNames.Contains("copter"))
                    {
                        features.Add(WorldObj.TeamFeature.AirForce);                     
                    }
                    else if (partsNames.Contains("Object036"))
                    {
                        features.Add(WorldObj.TeamFeature.Army);
                    }
                    else
                    {
                        features.Add(WorldObj.TeamFeature.Tower);
                    }

                    if (partsNames.Contains("colour_gun"))
                    {
                        features.Add(WorldObj.TeamFeature.AttackAir);
                    }
                    else if(partsNames.Contains("gun"))
                    {
                        features.Add(WorldObj.TeamFeature.AttackGround);
                    }
                    break;
                case WorldObj.SoldierType.Worker:
                    features.Add(WorldObj.TeamFeature.Army);
                    features.Add(WorldObj.TeamFeature.AttackGround);
                    break;
                case WorldObj.SoldierType.Griffin:
                    features.Add(WorldObj.TeamFeature.Army);
                    features.Add(WorldObj.TeamFeature.AttackGround);
                    features.Add(WorldObj.TeamFeature.AttackAir);
                    break;
                default:
                    break;                    
            }
           
        }

        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (soldiernumber <= 0 || locking)
            {
                return;
            }

            //同步血条位置跟随队伍
            if (teamHealthCanvas)
            {
                teamHealthCanvas.position = transform.position;
            }

            if (!photonView.isMine)
            {
                return;
            }

            if (teamAnimator)
            {
                //根据速度播放或关闭行走动画
                teamAnimatorState = teamAnimator.GetCurrentAnimatorStateInfo(0);
            }


            if (!teamAnimatorState.IsName("Death") && coroutineStepBuild == null)// &&  !fighting && enemies.Count == 0)
            {
                //如果队伍是空军或塔则不计算移动速度？
                if (features.Contains(WorldObj.TeamFeature.AirForce) || features.Contains(WorldObj.TeamFeature.Tower))
                {
                    /*
                    if (enemies.Count == 0)
                    {
                        PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                    } 
                    */
                 }
                else
                {
                    moving = Mathf.Abs(teamNavMeshAgent.velocity.x) + Mathf.Abs(teamNavMeshAgent.velocity.z);
                    if (moving >= 0.05f && !fighting)
                    {
                        PlayAnimator(WorldObj.AnimatorAction.Idle, false);
                        PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                        PlayAnimator(WorldObj.AnimatorAction.Victory, false);
                        PlayAnimator(WorldObj.AnimatorAction.Run, true);
                    }
                    else if (teamAnimatorState.IsName("Run") && !teamAnimatorState.IsName("Attack02") && !fighting && enemies.Count == 0)
                    {
                        PlayAnimator(WorldObj.AnimatorAction.Run, false);
                        PlayAnimator(WorldObj.AnimatorAction.Victory, true);
                    }
                }                

            }
            //有建造进程时仅关闭行走动画
            else if (coroutineStepBuild != null)
            {
                PlayAnimator(WorldObj.AnimatorAction.Run, false);
            }
            else
            {                
                Debug.Log("播放死亡动画时不允许移动。");
                teamNavMeshAgent.isStopped = true;
                return;
            }
            
            //如果本队伍不是野怪并且不是塔，则追踪接近目标后开始进行攻击
            if (nationality != WorldObj.Nationality.monster && !features.Contains(WorldObj.TeamFeature.Tower))
            {
                //如果转换目标则清空敌人列表，关闭战斗状态
                if (targetTeam != nowTargetTeam)
                {
                    Debug.Log("转换队伍目标了");
                    ClearEnemies();
                    UpdateFighting(false);
                    PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                    nowTargetTeam = targetTeam;
                }

                if (targetCity != nowTargetCity)
                {
                    Debug.Log("转换城市目标了");
                    ClearEnemies();
                    UpdateFighting(false);
                    PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                    nowTargetCity = targetCity;
                }

                if (nowTargetTeam)
                {                    
                    //每隔1.5秒进行一次侦测和攻击
                    cooltime += Time.deltaTime;
                    if (cooltime>=1.5f)
                    {
                        //teamController.GoToPosition(nowTargetTeam.transform.position);
                        Follow(nowTargetTeam.transform.position);
                        //teamNavMeshAgent.SetDestination(nowTargetTeam.transform.position);
                        //追踪时发现目标距离己方队伍已小于攻击距离，则执行攻击
                        if (Vector3.Distance(transform.position, nowTargetTeam.transform.position) <= attackDistance)
                        {
                            TeamFight(nowTargetTeam);
                        }
                        else if (enemies.Count > 0 || fighting)
                        {
                            //目标超出攻击范围继续追踪
                            Debug.Log("超出攻击范围了。");
                            UpdateEnemies(nowTargetTeam.hero, false);
                            UpdateFighting(false);
                        }
                        cooltime = 0;
                    }
                }
                else if (nowTargetCity)
                {
                    //每隔1.5秒进行一次侦测和攻击
                    cooltimeCityFight += Time.deltaTime;
                    if (cooltimeCityFight >= 1.5f)
                    {
                        //teamNavMeshAgent.SetDestination(nowTargetCity.transform.position);
                        Follow(nowTargetCity.transform.position);
                        //追踪时发现目标距离己方队伍已小于攻击距离，则执行攻击
                        if (Vector3.Distance(transform.position, nowTargetCity.transform.position) <= attackDistance)
                        {
                            CityFight(nowTargetCity);
                        }
                        else if (enemies.Count > 0 || fighting)
                        {
                            //目标超出攻击范围继续追踪
                            Debug.Log("超出攻击范围了。");
                            UpdateEnemies(nowTargetCity.word, false);
                            UpdateFighting(false);
                        }
                        cooltimeCityFight = 0;
                    }
                }
                else //失去目标，关闭战斗动画
                {
                    if (enemies.Count > 0 || fighting)
                    {
                        PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                        PlayAnimator(WorldObj.AnimatorAction.Victory, true);
                        ClearEnemies();
                        UpdateFighting(false);
                        if (coroutineTurnToTarget != null)
                        {   //停止转向面对敌人的协程
                            StopCoroutine(coroutineTurnToTarget);
                        }
                        if (coroutineFollow != null)
                        {   //空军则停止跟踪协程
                            StopCoroutine(coroutineFollow);
                        }

                    }
                }
            }
            else //如果本队伍是野怪或者是塔，则对首次发现的队伍攻击，直到该队伍超出攻击范围或死亡，则重新侦测
            {
                if (enemies.Count == 0 && fighting)
                {
                    UpdateFighting(false);
                    PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                    PlayAnimator(WorldObj.AnimatorAction.Victory, true);
                }

                //每1.5秒进行一次侦测和攻击
                cooltimeTeamFight += Time.deltaTime;
                
                if (cooltimeTeamFight >= 1.5f)
                {
                    if (findTeam && findTeam.nationality == WorldObj.Nationality.none)
                    {
                        //队伍死亡时，先设置的其国籍为空，所以这个时候可以认为其已经死亡
                        findTeam = null;
                    }
                    if (!findTeam || Vector3.Distance(findTeam.transform.position, transform.position) > attackDistance)
                    {                        
                        findTeam = null;
                        ClearEnemies();

                        for (int i = 0; i < WorldObj.allTeams.Count; i++)
                        {
                            if (WorldObj.allTeams[i] != this)//&& WorldObj.allTeams[i].enemies.Contains(hero))
                            {
                                if (Vector3.Distance(WorldObj.allTeams[i].transform.position, transform.position) <= attackDistance
                                    && WorldObj.allTeams[i].nationality != nationality)
                                {
                                    //TeamFight(WorldObj.allTeams[i]);
                                    findTeam = WorldObj.allTeams[i];
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //缓慢转向目标
                        Vector3 targetAirPosition = new Vector3(findTeam.transform.position.x, transform.position.y, findTeam.transform.position.z);
                        if (coroutineTurnToTarget != null)
                        {
                            StopCoroutine(coroutineTurnToTarget);
                        }
                        coroutineTurnToTarget = StartCoroutine(Funcs.TurnToTarget(transform, targetAirPosition, 7f, Time.deltaTime * 2f));
                        //攻击敌人
                        TeamFight(findTeam);
                    }
                    cooltimeTeamFight = 0;
                }
            }
        }



        #region Public Methods
        /// <summary>
        /// 动画播放
        /// </summary>
        /// <param name="_action">播放的动作</param>
        /// <param name="_ok">播放或停止</param>

        public void PlayAnimator(WorldObj.AnimatorAction _action, bool _ok)
        {

            //如果兵种是TANK，则特殊处理动画
            if (soldiertype == WorldObj.SoldierType.Tank)
            {
                switch (_action)
                {
                    case WorldObj.AnimatorAction.none:
                        break;
                    case WorldObj.AnimatorAction.Idle:
                        break;
                    case WorldObj.AnimatorAction.Walk:
                        break;
                    case WorldObj.AnimatorAction.Run:
                        break;
                    case WorldObj.AnimatorAction.GetHit:
                        break;
                    case WorldObj.AnimatorAction.Attack01:
                        break;
                    case WorldObj.AnimatorAction.Attack02:
                        photonView.RPC("TankAttack02RPC", PhotonTargets.All, _ok);
                        break;
                    case WorldObj.AnimatorAction.Victory:
                        break;
                    case WorldObj.AnimatorAction.Death:
                        break;
                    case WorldObj.AnimatorAction.Work:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                GetComponentInChildren<Animator>().SetBool(actions[(int)_action], _ok);
            }            
        }
        /// <summary>
        /// 同步TANK攻击02
        /// </summary>
        /// <param name="_ok">攻击或停止</param>
        [PunRPC]
        void TankAttack02RPC(bool _ok)
        {
            foreach (var item in transform.GetComponentsInChildren<Animation>())
            {
                if (item.enabled && item.clip.name == "Gun")
                {
                    if (_ok)
                    {
                        item.Play();                        
                    }
                    else if (item.isPlaying)
                    {                                                
                        item.Stop();
                    }
                }
            }
        }
        

        /// <summary>
        /// 同步更新国籍
        /// </summary>
        /// <param name="_soldiernumber">国籍</param>
        public void GiveNationality(WorldObj.Nationality _nationality)
        {
            photonView.RPC("GiveNationalityRPC", PhotonTargets.All, _nationality);
        }
        [PunRPC]
        void GiveNationalityRPC(WorldObj.Nationality _nationality)
        {
            nationality = _nationality;
        }

        /// <summary>
        /// 同步更新队伍士兵数量
        /// </summary>
        /// <param name="_soldiernumber">新的士兵数量</param>
        public void UpdateSoldierNumber(int _soldiernumber)
        {
            photonView.RPC("UpdateSoldierNumberRPC", PhotonTargets.All, _soldiernumber);
        }
        [PunRPC]
        void UpdateSoldierNumberRPC(int _soldiernumber)
        {
            soldiernumber = _soldiernumber;
            if (soldiernumber <= 0)
            {
                //播放死亡动画
                PlayAnimator(WorldObj.AnimatorAction.Run, false);
                PlayAnimator(WorldObj.AnimatorAction.Victory, false);
                PlayAnimator(WorldObj.AnimatorAction.Idle, false);
                PlayAnimator(WorldObj.AnimatorAction.Death, true);
                PlayAnimator(WorldObj.AnimatorAction.Attack02, false);
                if (soldiertype == WorldObj.SoldierType.Worker)
                {
                    PlayAnimator(WorldObj.AnimatorAction.Work, false);
                }
                if (teamNavMeshAgent && teamNavMeshAgent.enabled)
                {
                    teamNavMeshAgent.SetDestination(transform.position);
                    teamNavMeshAgent.isStopped = true;
                }
                else
                {
                    if (coroutineFollow != null)
                    {
                        StopCoroutine(coroutineFollow);
                    }
                }
                //使死亡的角色缓慢消失
                StartCoroutine(Funcs.SlowDisappearIE(transform, 3f, 100f, false));
                Debug.Log("播放了死亡动画！");
            }
        }



        /// <summary>
        /// 同步更新队伍速度
        /// </summary>
        /// <param name="_teamspeed">新的队伍速度</param>
        public void UpdateTeamSpeed(float _teamspeed)
        {
            photonView.RPC("UpdateTeamSpeedRPC", PhotonTargets.All, _teamspeed);
        }
        [PunRPC]
        void UpdateTeamSpeedRPC(float _teamspeed)
        {
            teamspeed = _teamspeed;
        }

        /// <summary>
        /// 同步更新队伍导航控件速度
        /// </summary>
        /// <param name="_teamspeed">新的队伍速度</param>
        public void UpdateTeamNAVSpeed(float _teamnavspeed)
        {
            photonView.RPC("UpdateTeamNAVSpeedRPC", PhotonTargets.All, _teamnavspeed);
        }
        [PunRPC]
        void UpdateTeamNAVSpeedRPC(float _teamnavspeed)
        {
            //teamNavMeshAgent.speed = _teamnavspeed;
            GetComponent<NavMeshAgent>().speed = _teamnavspeed;
        }


        /// <summary>
        /// 同步更新队伍导航控件stopingdistance
        /// </summary>
        /// <param name="_teamspeed">新的stopingdistance</param>
        public void UpdateTeamNAVStopingDistance(float _distance)
        {
            photonView.RPC("UpdateTeamNAVStopingDistanceRPC", PhotonTargets.All, _distance);
        }
        [PunRPC]
        void UpdateTeamNAVStopingDistanceRPC(float _distance)
        {
            //teamNavMeshAgent.stoppingDistance = _distance;
            GetComponent<NavMeshAgent>().stoppingDistance = _distance;
        }


        /// <summary>
        /// 同步更新队伍军心
        /// </summary>
        /// <param name="_teammorale">新的队伍军心</param>
        public void UpdateTeamMorale(int _teammorale)
        {
            photonView.RPC("UpdateTeamMoraleRPC", PhotonTargets.All, _teammorale);
        }
        [PunRPC]
        void UpdateTeamMoraleRPC(int _teammorale)
        {
            if (_teammorale > 100)
            {
                _teammorale = 100; //军心最大值为100;
            }
            teammorale = _teammorale;

        }

        /// <summary>
        /// 同步更新队伍英雄
        /// </summary>
        /// <param name="_hero">新的队伍英雄</param>
        public void UpdateTeamHero(string _hero)
        {
            photonView.RPC("UpdateTeamHeroRPC", PhotonTargets.All, _hero);
        }
        [PunRPC]
        void UpdateTeamHeroRPC(string _hero)
        {
            hero = _hero;

        }

        /// <summary>
        /// 同步更新士兵类型
        /// </summary>
        /// <param name="_soldiertype">新的士兵类型</param>
        public void UpdateSoldierType(WorldObj.SoldierType _soldiertype)
        {
            photonView.RPC("UpdateSoldierTypeRPC", PhotonTargets.All, _soldiertype);
        }
            [PunRPC]
        void UpdateSoldierTypeRPC(WorldObj.SoldierType _soldiertype)
        {
            soldiertype = _soldiertype;

        }

        /// <summary>
        /// 装载型队伍同步更新被装载队伍士兵类型
        /// </summary>
        /// <param name="_soldiertype">新的士兵类型</param>
        public void UpdateDriverSoldierType(WorldObj.SoldierType _soldiertype)
        {
            photonView.RPC("UpdateDriverSoldierTypeRPC", PhotonTargets.All, _soldiertype);
        }
        [PunRPC]
        void UpdateDriverSoldierTypeRPC(WorldObj.SoldierType _soldiertype)
        {
            driversoldiertype = _soldiertype;

        }

        /// <summary>
        /// 同步增加或减少敌人列表
        /// </summary>
        public void UpdateEnemies(string _teamHero, bool _add)
        {
            photonView.RPC("UpdateEnemiesRPC", PhotonTargets.All, _teamHero, _add);
        }
        [PunRPC]
        void UpdateEnemiesRPC(string _teamHero, bool _add)
        {
            if (_add)
            {
                enemies.Add(_teamHero);
            }
            else
            {
                enemies.Remove(_teamHero);
            }                       
        }

        /// <summary>
        /// 同步清空敌人列表
        /// </summary>
        public void ClearEnemies()
        {
            photonView.RPC("ClearEnemiesRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        void ClearEnemiesRPC()
        {
            enemies.Clear();
        }

        /// <summary>
        /// 同步增加队伍到静态所有队伍对象列表
        /// </summary>
        public void AddToTeams()
        {
            photonView.RPC("AddToTeamsRPC", PhotonTargets.All,null);
        }
        [PunRPC]
        void AddToTeamsRPC()
        {
            WorldObj.allTeams.Add(this);
        }

        /// <summary>
        /// 同步更新队伍血条最大值
        /// </summary>
        /// <param name="_value">血条值最大值</param>
        public void UpdateSliderMaxValue(int _value)
        {
            photonView.RPC("UpdateSliderMaxValueRPC", PhotonTargets.All, _value);
        }
        [PunRPC]
        void UpdateSliderMaxValueRPC(int _value)
        {
            teamSlider.maxValue = _value;
        }

        /// <summary>
        /// 同步更新队伍血条值
        /// </summary>
        /// <param name="_value">血条值</param>
        public void UpdateSliderValue(int _value)
        {
            photonView.RPC("UpdateSliderValueRPC", PhotonTargets.All, _value);
        }
        [PunRPC]
        void UpdateSliderValueRPC(int _value)
        {
            teamSlider.value = _value;
        }

        /// <summary>
        /// 同步更新队伍交战状态
        /// </summary>
        /// <param name="_soldiernumber">新的状态</param>
        public void UpdateFighting(bool _fighting)
        {            
            photonView.RPC("UpdateFightingRPC", PhotonTargets.All, _fighting);
        }
        [PunRPC]
        void UpdateFightingRPC(bool _fighting)
        {
            fighting = _fighting;
        }


        /// <summary>
        /// 销毁己方队伍，删除静态队伍对象列表项
        /// </summary>
        public void DestroySelf(float _delay)
        {
            photonView.RPC("DestroySelfRPC", PhotonTargets.All,  _delay);
        }
        [PunRPC]
        void DestroySelfRPC(float _delay)
        {
            WorldObj.allTeams.Remove(this);
            if (teamHealthCanvas)
            {
                Destroy(teamHealthCanvas.gameObject);
            }
            /*
            //己方队伍身上有聚光灯则变其为全局灯
            if (teamController)
            {
                teamController.GlobalGetSpotlight(this);
            }    
            */
            Destroy(gameObject, _delay);
            
        }

        #endregion


        #region 队伍和城市战斗 Methods
        /// <summary>
        /// 跟踪目标
        /// </summary>
        /// <param name="_targetPosition">目标位置</param>
        public void Follow(Vector3 _targetPosition)
        {
            if (coroutineTurnToTarget!=null)
            {
                StopCoroutine(coroutineTurnToTarget);
            }
            if (features.Contains(WorldObj.TeamFeature.AirForce))
            {
                if (!locking)
                {
                    if (coroutineFollow != null)
                    {
                        StopCoroutine(coroutineFollow);
                    }
                    coroutineFollow = StartCoroutine(AirForceFollowIE(_targetPosition));
                }
            }
            else if (features.Contains(WorldObj.TeamFeature.Tower))
            {
            }
            else
            {
                teamNavMeshAgent.SetDestination(_targetPosition);
            }
        }
        /// <summary>
        /// 空军和塔队伍跟踪目标协程
        /// </summary>
        /// <param name="_targetPosition">目标位置</param>
        /// <returns></returns>
        public IEnumerator AirForceFollowIE(Vector3 _targetPosition)
        {
            Vector3 targetAirPosition = new Vector3(_targetPosition.x, transform.position.y, _targetPosition.z);
            //转向
            if (coroutineTurnToTarget!=null)
            {
                StopCoroutine(coroutineTurnToTarget);
            }
            coroutineTurnToTarget = StartCoroutine(Funcs.TurnToTarget(transform, targetAirPosition, 7f, Time.deltaTime * 2f));

            float stopDistance = 0.5f;
            
            if (enemies.Count > 0 && (targetTeam || targetCity ))
            {
                stopDistance = attackDistance;
            }
            //跟随
            while (Vector3.Distance(transform.position, targetAirPosition) >= stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetAirPosition, 0.02f);
                yield return new WaitForSeconds(Time.deltaTime * 1f);
            }
        }



        /// <summary>
        /// 队伍交战
        /// </summary>
        /// <param name="_teamTarget">敌方</param>
        public void TeamFight(TeamObj _teamTarget)
        {
            //非敌国籍或我方已阵亡，返回
            if (nationality == _teamTarget.nationality 
                || nationality == WorldObj.Nationality.none //阵亡时将国籍置空
                || _teamTarget.nationality == WorldObj.Nationality.none
                )
            {
                return;
            }
            //如果目标是空军我军又没有对空武器，
            //如果目标是陆军我军又没有对地武器，
            //如果目标是塔我军又没有对地武器，则返回
            if (_teamTarget.features.Contains(WorldObj.TeamFeature.AirForce) && !features.Contains(WorldObj.TeamFeature.AttackAir)
                || _teamTarget.features.Contains(WorldObj.TeamFeature.Army) && !features.Contains(WorldObj.TeamFeature.AttackGround)
                || _teamTarget.features.Contains(WorldObj.TeamFeature.Tower) && !features.Contains(WorldObj.TeamFeature.AttackGround))
            {
                return;
            }

            //本队停止移动
            if (coroutineFollow != null)
            {   //空军则停止跟踪协程
                StopCoroutine(coroutineFollow);
            }
            if (teamNavMeshAgent && teamNavMeshAgent.enabled)
            {   //陆军则使用导航组件停止移动
                teamNavMeshAgent.SetDestination(transform.position);
            }
            
            if (!fighting)
            {
                //播放攻击动画
                PlayAnimator(WorldObj.AnimatorAction.Victory, false);
                PlayAnimator(WorldObj.AnimatorAction.Run, false);
                PlayAnimator(WorldObj.AnimatorAction.Idle, false);
                PlayAnimator(WorldObj.AnimatorAction.Attack02, true);

                UpdateFighting(true);               
            }

            //播放攻击动画,增加敌人列表
            if (!enemies.Contains(_teamTarget.hero))
            {
                Debug.Log("增加我的敌人列表");
                //缓慢转向目标
                Vector3 targetAirPosition = new Vector3(_teamTarget.transform.position.x, transform.position.y, _teamTarget.transform.position.z);
                if (coroutineTurnToTarget != null)
                {
                    StopCoroutine(coroutineTurnToTarget);
                }
                coroutineTurnToTarget = StartCoroutine(Funcs.TurnToTarget(transform, targetAirPosition, 7f, Time.deltaTime * 2f));
                //transform.LookAt(_teamTarget.transform);
                UpdateEnemies(_teamTarget.hero, true);                
            }

            //队伍伤害敌方队伍（包括野怪）
            TeamDamageTeam(_teamTarget);

            //如果敌方队伍士兵死光，则英雄归野，销毁敌方队伍
            if (_teamTarget.soldiernumber <= 0)
            {
                //清除我方敌人列表
                UpdateEnemies(_teamTarget.hero, false);

                //如果杀死的敌方是野怪
                if (_teamTarget.nationality == WorldObj.Nationality.monster)
                {
                    //更新所有参与杀怪队伍的属性为满状态
                    foreach (var item in _teamTarget.enemies)
                    {
                        TeamObj fightTeam = WorldObj.MatchTeam(item);
                        HeroObj fightHero = WorldObj.MatchHero(item);
                        Debug.Log(WorldObj.NationalityDictionary[fightTeam.nationality] + " : " + WorldObj.NationalityDictionary[_teamTarget.nationality]);
                        if (fightTeam.nationality == nationality)
                        {
                            fightTeam.UpdateSoldierNumber(fightHero.dominance);
                            fightTeam.UpdateTeamMorale(100);
                            fightTeam.UpdateSliderMaxValue(fightHero.dominance);
                            fightTeam.UpdateSliderValue(fightHero.dominance);
                        }
                    }

                    //杀死野怪的国家获得金钱奖励
                    CountryObj selfCountry = WorldObj.MatchCountry(nationality);
                    switch (_teamTarget.soldiertype)
                    {
                        case WorldObj.SoldierType.Griffin:
                            selfCountry.UpdateMoney(selfCountry.money + WorldObj.GriffinBonus);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //英雄归野
                    HeroObj targetHero = WorldObj.MatchHero(_teamTarget.hero);
                    if (targetHero)
                    {
                        targetHero.GiveNationality(WorldObj.Nationality.none);
                    }
                    
                }

                //消除队伍国籍
                _teamTarget.GiveNationality(WorldObj.Nationality.none);

                _teamTarget.DestroySelf(3.5f);

            }
        }

        /// <summary>
        /// 队伍伤害队伍（包括野怪）
        /// </summary>
        /// <param name="_teamTarget">敌方队伍</param>
        void TeamDamageTeam(TeamObj _teamTarget)
        {
            //计算兵种克制值
            float restraint = 1;
            if (soldiertype == WorldObj.SoldierType.SwordMan && _teamTarget.soldiertype == WorldObj.SoldierType.Archer)
            {
                restraint = 1 + WorldObj.restraintCoefficient;
            }
            else if (_teamTarget.soldiertype == WorldObj.SoldierType.SwordMan && soldiertype == WorldObj.SoldierType.Archer)
            {
                restraint = WorldObj.restraintCoefficient;
            }
            else if (soldiertype == WorldObj.SoldierType.Archer && _teamTarget.soldiertype == WorldObj.SoldierType.Cavalry)
            {
                restraint = 1 + WorldObj.restraintCoefficient;
            }
            else if (_teamTarget.soldiertype == WorldObj.SoldierType.Archer && soldiertype == WorldObj.SoldierType.Cavalry)
            {
                restraint = WorldObj.restraintCoefficient;
            }
            else if (soldiertype == WorldObj.SoldierType.Cavalry && _teamTarget.soldiertype == WorldObj.SoldierType.SwordMan)
            {
                restraint = 1 + WorldObj.restraintCoefficient;
            }
            else if (_teamTarget.soldiertype == WorldObj.SoldierType.Cavalry && soldiertype == WorldObj.SoldierType.SwordMan)
            {
                restraint =  WorldObj.restraintCoefficient;
            }
            else if (soldiertype == WorldObj.SoldierType.Tank)
            {
                restraint = 1;
            }
            else if (_teamTarget.nationality == WorldObj.Nationality.monster)
            {
                restraint = 1;
            }


            //英雄武力值
            int heroForce = 0;
            if (nationality != WorldObj.Nationality.monster)
            {
                switch (soldiertype)
                {
                    case WorldObj.SoldierType.SwordMan:
                    case WorldObj.SoldierType.Archer:
                    case WorldObj.SoldierType.Cavalry:
                    case WorldObj.SoldierType.Worker:                
                    case WorldObj.SoldierType.Tank:
                        heroForce = WorldObj.MatchHero(hero).force;
                        break;
                    case WorldObj.SoldierType.Griffin:
                        break;
                    default:
                        break;
                }
                
            }

            //队伍破坏力计算（根据敌人列表平摊其破坏力）
            int damage = Mathf.RoundToInt(restraint * 3 * (heroForce + soldiernumber * teammorale / 10000));            

            //敌方队伍士兵承受伤害减少数量
            _teamTarget.UpdateSoldierNumber(_teamTarget.soldiernumber - damage);
            //敌方队伍血条减少
            _teamTarget.UpdateSliderValue(_teamTarget.soldiernumber);
        }



        /// <summary>
        /// 与城市交战
        /// </summary>
        /// <param name="_cityTarget">交战的敌方城市</param>
        public void CityFight(CityObj _cityTarget)
        {
            if (nationality == _cityTarget.nationality 
                || nationality == WorldObj.Nationality.none 
                || _cityTarget.nationality == WorldObj.Nationality.none)
            {
                return;
            }

            //如果我军没有对地武器，则返回
            if (!features.Contains(WorldObj.TeamFeature.AttackGround))
            {
                return;
            }

            //本队停止移动
            if (coroutineFollow != null)
            {   //空军则停止跟踪协程
                StopCoroutine(coroutineFollow);
            }
            if (teamNavMeshAgent)
            {   //陆军则使用导航组件停止移动
                teamNavMeshAgent.SetDestination(transform.position);
            }

            //增加敌方城市敌人列表
            if (!_cityTarget.enemies.Contains(hero))
            {
                _cityTarget.UpdateEnemies(hero, true);
                _cityTarget.UpdateFighting(true);
            }

            //增加我方队伍敌人列表
            if (!enemies.Contains(_cityTarget.word))
            {
                UpdateEnemies(_cityTarget.word, true);
                UpdateFighting(true);
                //播放攻击动画
                transform.LookAt(new Vector3(_cityTarget.transform.position.x,transform.position.y,_cityTarget.transform.position.z));
                PlayAnimator(WorldObj.AnimatorAction.Victory, false);
                PlayAnimator(WorldObj.AnimatorAction.Run, false);
                PlayAnimator(WorldObj.AnimatorAction.Attack02,true);
            }

            //队伍承受敌方城市伤害
            TeamTakeCityDamage(_cityTarget);

            //敌方城市承受我方队伍伤害
            CityTakeDamage(_cityTarget);

            //如果己方队伍士兵死光，则英雄归野，销毁己方队伍。
            if (soldiernumber <= 0)
            {
                //消除队伍国籍为了让其CityFight不执行，就可以对外界无影响地演示死亡动画
                GiveNationality(WorldObj.Nationality.none);
                //播放死亡动画
                PlayAnimator(WorldObj.AnimatorAction.Victory, false);
                PlayAnimator(WorldObj.AnimatorAction.Death, true);
                PlayAnimator(WorldObj.AnimatorAction.Attack02, false);                
                Debug.Log("播放了死亡动画");
                //将城市的敌人列表去掉本队伍
                _cityTarget.UpdateEnemies(hero, false);
                //如果敌人列表变空，则取消城市战争状态
                if (_cityTarget.enemies.Count == 0)
                {
                    _cityTarget.UpdateFighting(false);
                }
                if (heroSelf)
                {
                    heroSelf.GiveNationality(WorldObj.Nationality.none);
                }                
                DestroySelf(3.5f);
            }

            //如果敌方城破，则城市英雄归野，更换敌方城市国籍为我国国籍，我方队伍进入，重置城防（TANK队伍则不占城入城，仅更新城防）
            if (_cityTarget.citydefence<5 || _cityTarget.swordmannumber + _cityTarget.archernumber +_cityTarget.cavalrynumber <50)
            {
                //测试敌国是否灭亡
                bool gameOver = true;
                foreach (var item in WorldObj.allCities) //cityarray)
                {
                    if (item.nationality == _cityTarget.nationality && item != _cityTarget)
                    {
                        gameOver = false;
                        break;
                    }
                }
                if (gameOver)
                {
                    Debug.Log("敌人国家灭亡");
                }


                //将城市的敌人列表去掉本队伍
                _cityTarget.UpdateEnemies(hero, false);

                //清除所有本次城市战争相关对象添加的敌人列表项
                for (int i = 0; i < _cityTarget.enemies.Count; i++)
                {
                    TeamObj fightTeam = WorldObj.MatchTeam(_cityTarget.enemies[i]);
                    if (fightTeam)
                    {
                        fightTeam.UpdateEnemies(_cityTarget.word, false);
                    }                    
                    _cityTarget.UpdateEnemies(_cityTarget.enemies[i], false);
                }


                //城市战争状态取消
                _cityTarget.UpdateFighting(false);
                //城市敌方英雄归野
                for (int i = 0; i < _cityTarget.heros.Count; i++)
                {
                    WorldObj.MatchHero(_cityTarget.heros[i]).GiveNationality(WorldObj.Nationality.none);
                    _cityTarget.AddOrRemoveHero(_cityTarget.heros[i], false);
                }

                switch (soldiertype)
                {
                    case WorldObj.SoldierType.SwordMan:
                    case WorldObj.SoldierType.Archer:
                    case WorldObj.SoldierType.Cavalry:
                    case WorldObj.SoldierType.Worker:
                        //普通兵种则占领城市
                        CaptureCity(_cityTarget);
                        break;
                    case WorldObj.SoldierType.Tank:
                        //Tank兵种则仅重置城防
                        _cityTarget.UpdateDefence(WorldObj.BeginCityDefence);
                        break;
                    case WorldObj.SoldierType.Griffin:
                        break;
                    default:
                        break;
                }

            }
        }
        /// <summary>
        /// 占领城市
        /// </summary>
        public void CaptureCity(CityObj _cityTarget)
        {
            //重置城防
            _cityTarget.UpdateDefence(WorldObj.BeginCityDefence);

            //分配我方国籍和国籍标志
            _cityTarget.GiveNationality(nationality);
            _cityTarget.GiveFlag();

            //我方队伍入城
            //将队伍士兵归入城市
            switch (soldiertype)
            {
                case WorldObj.SoldierType.SwordMan:
                    _cityTarget.UpdateSoldiers(_cityTarget.swordmannumber + soldiernumber, _cityTarget.archernumber, _cityTarget.cavalrynumber, _cityTarget.workernumber);
                    break;
                case WorldObj.SoldierType.Archer:
                    _cityTarget.UpdateSoldiers(_cityTarget.swordmannumber, _cityTarget.archernumber + soldiernumber, _cityTarget.cavalrynumber, _cityTarget.workernumber);
                    break;
                case WorldObj.SoldierType.Cavalry:
                    _cityTarget.UpdateSoldiers(_cityTarget.swordmannumber, _cityTarget.archernumber, _cityTarget.cavalrynumber + soldiernumber, _cityTarget.workernumber);
                    break;
                case WorldObj.SoldierType.Worker:
                    _cityTarget.UpdateSoldiers(_cityTarget.swordmannumber, _cityTarget.archernumber, _cityTarget.cavalrynumber, _cityTarget.workernumber + soldiernumber);
                    break;
                default:
                    break;
            }
            //消除队伍国籍为了让其CityFight不再执行
            GiveNationality(WorldObj.Nationality.none);
            //将队伍英雄归入城市
            _cityTarget.AddOrRemoveHero(hero, true);
            //销毁本队
            DestroySelf(0f);
            //城市获得聚光灯
            //teamController.CityGetSpotlight(_cityTarget);
        }


        /// <summary>
        /// 队伍承受城市伤害
        /// </summary>
        void TeamTakeCityDamage(CityObj _cityTarget)
        {
            //城市破坏力计算
            //计算城市最高(总)英雄武力值，在非战状态计算，只计算一次后便进入战争状态
            if (!_cityTarget.fighting)
            {
                for (int i = 0; i < _cityTarget.heros.Count; i++)
                {
                    int heroForce = WorldObj.MatchHero(_cityTarget.heros[i]).force;
                    //总武力值计算方法
                    cityHighestForce += heroForce;
                    /*最高武力值计算方法
                    if (heroForce > cityHighestForce)
                    {
                        cityHighestForce = heroForce;
                    }
                    */
                }
            }

            //敌方城市破坏力计算（根据敌方的敌人列表平摊其破坏力）
            int targetDamage = Mathf.RoundToInt(3*(cityHighestForce + _cityTarget.citydefence + (_cityTarget.swordmannumber + _cityTarget.archernumber + _cityTarget.cavalrynumber) / 100) / _cityTarget.enemies.Count);
            //己方队伍士兵承受伤害减少数量
            UpdateSoldierNumber(soldiernumber - targetDamage);

            //血条减少
            UpdateSliderValue(soldiernumber);
        }
        /// <summary>
        /// 城市承受伤害
        /// </summary>
        void CityTakeDamage(CityObj _cityTarget)
        {
            //我方英雄武力值
            int heroSelfForce = 0;
            if (heroSelf)
            {
                heroSelfForce = heroSelf.force;
            }
            
            //我方队伍破坏力计算（根据我方的敌人列表平摊其破坏力）
            int damage = Mathf.RoundToInt((heroSelfForce + soldiernumber * teammorale / 10000) / enemies.Count);
            //敌方城市士兵承受伤害减少数量
            int totalSoldiers = _cityTarget.swordmannumber + _cityTarget.archernumber + _cityTarget.cavalrynumber;
            float swordmanShare = (float)_cityTarget.swordmannumber / totalSoldiers;
            float archerShare = (float)_cityTarget.archernumber / totalSoldiers;
            float cavalryShare = (float)_cityTarget.cavalrynumber / totalSoldiers;
            float workerShare = (float)_cityTarget.workernumber / totalSoldiers;
            Debug.Log(swordmanShare.ToString());
            //敌方城市士兵承受伤害减少数值
            _cityTarget.UpdateSoldiers(Mathf.FloorToInt(_cityTarget.swordmannumber - damage * swordmanShare),
                                        Mathf.FloorToInt(_cityTarget.archernumber - damage * archerShare),
                                        Mathf.FloorToInt(_cityTarget.cavalrynumber - damage * cavalryShare),
                                        Mathf.FloorToInt(_cityTarget.cavalrynumber - damage * workerShare));
            //敌方城市城防承受伤害减少数值
            _cityTarget.UpdateDefence((_cityTarget.citydefence * 100 - damage) / 100);

            //城市更改为战争状态
            _cityTarget.UpdateFighting(true);
        }

        /*
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(nationality);
                stream.SendNext(soldiernumber);
                stream.SendNext(soldiertype);
                stream.SendNext(hero);
                stream.SendNext(teamspeed);
                stream.SendNext(teammorale);
                stream.SendNext(fighting);
                stream.SendNext(going);
                //stream.SendNext(enemies);
            }
            else if (stream.isReading)
            {
                nationality = (WorldObj.Nationality)stream.ReceiveNext();
                soldiernumber = (int)stream.ReceiveNext();
                soldiertype = (WorldObj.SoldierType)stream.ReceiveNext();
                hero = (string)stream.ReceiveNext();
                teamspeed = (float)stream.ReceiveNext();
                teammorale = (int)stream.ReceiveNext();
                fighting = (bool)stream.ReceiveNext();
                going = (bool)stream.ReceiveNext();                
                //enemies = (List<>)stream.ReceiveNext();
            }
        }
        */
        #endregion
    }

}
