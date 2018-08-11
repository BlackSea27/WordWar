using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AI;
using System;

namespace Com.Aionline.WordsCities
{
    public class CountryObj : Photon.MonoBehaviour, IPunObservable
    {

        #region Public var

        public WorldObj.Nationality nationality = WorldObj.Nationality.none; // { set; get; }    //国籍

        public float taxrate { set; get; }  //税率

        public int money; // { set; get; }      //金钱

        //public CityObj[] cityarray; // { set; get; } //城市对象数组
        public HeroObj[] heroarray; // { set; get; } //武将对象数组
        //工程队在选择建造地点时如果发现该地点无法建造，则设置该标志为真，使得再次点击可以直接重选建造位置
        public bool reGoBuild = false;

        #endregion

        #region Private var
        //国家对象上的组件
        CityController cityController;
        TeamController teamController;
        UIController uiController;

        //有TeamObj被选择
        bool teamSelected = false;

        TeamObj selectedTeam = null;
        TeamObj targetTeam = null;

        List<string> cities = new List<string>(); //城市列表

        CityObj cityBegin;//起始城市

        //场景主摄像头
        Transform playercamera;
        //摄像头可以移动到的新位置
        Vector3 cameraNewPosition;
        //屏幕像素值和Unity的position单位对应的系数
        float screenSizeCoefficient;

        Transform Ground;
        Transform WorldUI;
        Transform teamScout;
        Text CountryMoney;
        float cooltime = 0.2f; //每10帧判断队伍是否能看见

        #endregion

        #region Private Methods
        /// <summary>
        /// 同步增加国家到静态所有国家对象列表
        /// </summary>
        public void AddToCountries()
        {
            photonView.RPC("AddToCountriesRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        void AddToCountriesRPC()
        {
            WorldObj.allCountries.Add(this);
        }



        /// <summary>
        /// 初始化ScoutUI
        /// </summary>
        void ScoutUINationality()
        {
            switch (nationality)
            {
                case WorldObj.Nationality.red:
                    teamScout.Find("Horseman_Red").gameObject.SetActive(true);
                    break;
                case WorldObj.Nationality.blue:
                    teamScout.Find("Horseman_Blue").gameObject.SetActive(true);
                    break;
                case WorldObj.Nationality.monster:
                    break;
                default:
                    break;
            }
            Animator scoutAnimator = teamScout.GetComponentInChildren<Animator>();
            scoutAnimator.SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], true);

            Text scoutInfoText = teamScout.Find("ScoutInfo").Find("ScoutInfoText").GetComponent<Text>();
            scoutInfoText.text = WorldObj.ScoutInfoDictionary[WorldObj.ScoutInfoType.none];
        }

            /// <summary>
            /// 国家初始化
            /// </summary>
        void CountryStart()
        {
            //初始化金钱
            money = WorldObj.BeginMoney;
            //初始化税率
            taxrate = WorldObj.BeginTax;
        }

        /// <summary>
        /// 给出起始城市
        /// </summary>
        void BeginCity()
        {
            cityBegin = WorldObj.allCities[0];
            float xTemp = 0;
            foreach (var item in WorldObj.allCities)
            {
                switch (nationality)
                {
                    case WorldObj.Nationality.none:
                        break;
                    case WorldObj.Nationality.red:
                        if (item.transform.position.x < xTemp)
                        {
                            cityBegin = item;
                            xTemp = item.transform.position.x;
                        }
                        break;
                    case WorldObj.Nationality.blue:
                        if (item.transform.position.x > xTemp)
                        {
                            cityBegin = item;
                            xTemp = item.transform.position.x;
                        }
                        break;
                    case WorldObj.Nationality.monster:
                        break;
                    default:
                        break;
                }

            }
            cities.Clear();
            cities.Add(cityBegin.word);
            cityBegin.GiveNationality(nationality);
            //给起始城市国籍标志
            cityBegin.GiveFlag();
        }


        /// <summary>
        /// 给出起始武将(并随机分配武将技和政治)
        /// </summary>
        void BeginHero()
        {
            //heroarray = FindObjectsOfType<HeroObj>();
            int randomindex = UnityEngine.Random.Range(0, WorldObj.allHeros.Count);
            if (WorldObj.allHeros[randomindex].nationality == WorldObj.Nationality.none)
            {
                WorldObj.allHeros[randomindex].GiveNationality(nationality);

                //将起始武将加入到起始城市的武将列表（同步）
                photonView.RPC("BeginHeroAssign", PhotonTargets.All, nationality, WorldObj.allHeros[randomindex].word);

            }
            else //否则重新随机武将
            {
                Debug.Log("执行了一次重新随机分配武将的代码");
                BeginHero();
            }
        }
        [PunRPC]
        void BeginHeroAssign( WorldObj.Nationality _nationality,string _randomHero)
        {
            foreach (var item in WorldObj.allCities)
            {
                if (item.nationality == _nationality)
                {
                    item.heros.Add(_randomHero);
                }
            }
        }


        /// <summary>
        /// 每个城市人口增长
        /// 改变城市对象人口属性- population
        /// </summary>
        void PopulationIncrease()
        {
            //foreach (var item in FindObjectsOfType<CityObj>())
            foreach (var item in WorldObj.allCities)
            {
                //仅对本国城市人口计算更新
                if (item.nationality == nationality)
                {
                    int cityHighestPolitics = 0; //城市最高的英雄政治
                    for (int i = 0; i < item.heros.Count; i++)
                    {
                        int heroPolitics = WorldObj.MatchHero(item.heros[i]).politics;

                        if (heroPolitics > cityHighestPolitics)
                        {
                            cityHighestPolitics = heroPolitics;
                        }
                    }
                    item.UpdatePopulation(WorldObj.calculatePopulationIncrease(item.population, cityHighestPolitics, taxrate));
                }
            }
        }

        /// <summary>
        /// 金钱增长
        /// 改变属性-money
        /// </summary>
        void MoneyIncrease()
        {
            if (cities.Count == 0)
            {
                return;
            }
            //金钱增长有设置封顶常量WorldObj.TopMoney
            if (money == WorldObj.TopMoney)
            {
                return;
            }

            //根据国家所有城市人口和国家税率增长金钱
            int totalpopulation = 0;
            CityObj[] cityarray = FindObjectsOfType<CityObj>();
            foreach (var item in cityarray)
            {
                if (item.nationality == nationality)
                {
                    totalpopulation += item.population;
                }
            }

            money += Mathf.RoundToInt(totalpopulation * taxrate);

            if (money >= WorldObj.TopMoney)
            {
                money = WorldObj.TopMoney;
            }
            //显示控件显示国家财富
            CountryMoney.text = money.ToString();            
        }

        /// <summary>
        /// 税率调整
        /// 改变属性-taxrate
        /// </summary>
        /// <param name="newtaxrate">新税率</param>
        void TaxRateGive(float newtaxrate)
        {
            taxrate = newtaxrate;
        }


        #endregion


        #region Public Methods

        /// <summary>
        /// 更新金钱数值
        /// </summary>
        /// <param name="_newMoney">新的金钱数值</param>
        public void UpdateMoney(int _newMoney)
        {
            photonView.RPC("UpdateMoneyRPC", PhotonTargets.All, _newMoney);
        }
        [PunRPC]
        void UpdateMoneyRPC(int _newMoney)
        {
            money = _newMoney;
        }


        /// <summary>
        /// 金钱使用
        /// 改变属性-money
        /// </summary>
        /// <param name="takemoney">使用的金钱</param>
        /// <returns>金钱使用是否成功</returns>
        public bool MoneyReduce(int takemoney)
        {
            if (money - takemoney >= 0)
            {
                money -= takemoney;
                return true;
            }
            else
            {
                return false;
            }
            
        }


        /*
        /// <summary>
        /// 城市列表增减
        /// </summary>
        /// <param name="cityword">城市单词</param>
        /// <param name="cityget">是否增加（否为失去城市）</param>
        public void UpdateCities(string _cityword, bool _addcity)
        {
            photonView.RPC("UpdateCitiesRPC", PhotonTargets.All, _cityword, _addcity);
        }
        [PunRPC]
        void UpdateCitiesRPC(string _cityword, bool _addcity)
        {
            if (_addcity)
            {
                cities.Add(_cityword);
                Debug.Log("城市：" + cities[0] + cities[1]+cities[2]);
            }
            else
            {
                cities.Remove(_cityword);
                if (cities.Count == 0)
                {
                    CountryDestruction();
                }
            }
        }
        */

        /*
        /// <summary>
        /// 武将列表增减
        /// </summary>
        /// <param name="heroword">武将单词</param>
        /// <param name="heroget">是否增加（否为失去武将）</param>
        public void HeroListGetOrLost(string heroword, bool heroget)
        {
            if (heroget)
            {
                heros.Add(heroword);
            }
            else if (!heroget)
            {
                heros.Remove(heroword);
            }
        }
        */

        /// <summary>
        /// 国家灭亡
        /// </summary>
        public void CountryDestruction()
        {
            Debug.Log("国家" + nationality + "灭亡");
            PhotonNetwork.Destroy(this.photonView);            

            //这里需实现游戏结束的相关代码
            
        }


        #endregion

        void Start()
        {
            DontDestroyOnLoad(this);
            if (!photonView.isMine)
            {
                return;
            }
            AddToCountries();

            cityController = GetComponent<CityController>();
            teamController = GetComponent<TeamController>();
            uiController = GetComponent<UIController>();

            playercamera = GameObject.Find("PlayerCamera").transform;
            cameraNewPosition = playercamera.transform.position;

            //计算屏幕像素值和正交摄像机的比例对应系数
            screenSizeCoefficient = Screen.height / 2 / playercamera.GetComponent<Camera>().orthographicSize;

            Ground = GameObject.Find("Ground(Clone)").transform;
            WorldUI = GameObject.Find("WorldUI(Clone)").transform;
            teamScout = WorldUI.Find("Panel").Find("TeamScout(Clone)");
            CountryMoney = WorldUI.Find("Panel").Find("CountryMoney").Find("MoneyText").GetComponent<Text>();

            //初始化ScoutUI的国籍
            ScoutUINationality();

            //初始化该国家
            CountryStart();
            //给出起始城市
            BeginCity();

            //设置摄像头的对准起始城市位置            
            cameraNewPosition.x = cityBegin.transform.position.x;

            //给出起始武将
            BeginHero();
            //1秒后定期5秒执行一次全国金钱总和增长计算
            InvokeRepeating("MoneyIncrease", 1f, 5f);
            //5秒后定期5秒执行一次全国每个城市人口增长计算和更新
            InvokeRepeating("PopulationIncrease", 5f, 5f);
        }

        private void FixedUpdate()
        {
            if (!photonView.isMine)
            {
                return;
            }

            //如果更换场景摄像头被Destroy了则playercamera指向新场景的摄像头
            if (!playercamera)
            {
                playercamera = FindObjectOfType<Camera>().transform;
            }

            if (!Ground)
            {
                Ground = GameObject.Find("Ground(Clone)").transform;
            }

            cooltime += Time.deltaTime;
            if (cooltime>=0.2f)
            {
                cooltime = 0f;
                //每10帧判断敌人队伍是否应该被看见
                foreach (var item in WorldObj.allTeams)
                {
                    //判断每一支非己方的TeamObj是否该被看见
                    if (item.nationality != nationality)
                    {
                        bool seen = false; //是否该被看见变量
                        GameObject infoSource = null;
                        foreach (var team in WorldObj.allTeams)
                        {
                            if (team.nationality == nationality)
                            {
                                if (Vector3.Distance(item.transform.position, team.transform.position) < WorldObj.SkillDistance)
                                {
                                    seen = true;
                                    infoSource = team.gameObject;
                                    break;
                                }
                            }
                        }
                        foreach (var eachcity in WorldObj.allCities)
                        {
                            if (eachcity && eachcity.nationality == nationality)
                            {
                                if (Vector3.Distance(item.transform.position, eachcity.transform.position) < WorldObj.SkillDistance)
                                {
                                    seen = true;
                                    infoSource = eachcity.gameObject;
                                    break;
                                }
                            }
                        }


                        //根据判断结果使敌人消失或者显形(包括敌人血条teamHealthCanvas)
                        if (seen && !item.visible) //使显形
                        {
                            item.visible = true;
                            StartCoroutine(Funcs.SlowDisappearIE(item.teamHealthCanvas, 2f, 100f, true));
                            StartCoroutine(Funcs.SlowDisappearIE(item.transform, 2f, 100f, true));
                            try
                            {
                                //侦查信息显示
                                uiController.ScoutInfoShow(nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.EnemyBeFound, infoSource);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (!seen && item.visible)//使消失
                        {
                            item.visible = false;
                            StartCoroutine(Funcs.SlowDisappearIE(item.teamHealthCanvas, 2f, 100f, false));
                            StartCoroutine(Funcs.SlowDisappearIE(item.transform, 2f, 100f, false));
                        }
                    }
                }
            }


            //如果正在操作界面的城市受到攻击，则关闭UI
            if (cityController.cityObj && cityController.cityObj.fighting && WorldObj.UIRun)
            {
                uiController.CloseAllPanels();
            }

            //如果城市的敌人列表中的队伍不再攻城或者已经不存在，则从列表中清除该队伍
            foreach (var item in WorldObj.allCities)
            {
                if (item && item.fighting)
                {
                    for (int i = 0; i < item.enemies.Count; i++)
                    {
                        TeamObj fightTeam = null;
                        foreach (var eachTeam in WorldObj.allTeams)
                        {
                            if (eachTeam.hero == item.enemies[i])
                            {
                                fightTeam = eachTeam;
                            }
                        }

                        if ((fightTeam && !fightTeam.enemies.Contains(item.word)) || !fightTeam)
                        {
                            item.UpdateEnemies(item.enemies[i], false);
                        }
                        if (item.enemies.Count == 0)
                        {
                            item.UpdateFighting(false);
                        }
                    }
                }
            }

        }

        void Update()
        {
            if (!photonView.isMine)
            {
                return;
            }


            //测试用代码
            if (Input.GetMouseButtonDown(1))
            {

            }



            //鼠标或触摸屏的输入操作
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector3 pos;
                if (Input.GetMouseButtonDown(0))
                {
                    pos = Input.mousePosition;
                }
                else
                {
                    pos = Input.GetTouch(0).deltaPosition;
                }

                //从摄像头发送射线指定屏幕位置到场景位置
                RaycastHit hit = Funcs.RayScreenPoint(playercamera, pos);
                if (!hit.collider )
                {
                    Debug.Log("未点击到任何物体！");
                    return;
                }
                Transform obj = hit.collider.transform;

                //如果UI没有在运行
                if ( !WorldObj.UIRun)
                {
                    //摄像头移动的操作
                    if (!teamSelected && obj.parent && !obj.parent.GetComponent<TeamObj>()
                        && (pos.x < Screen.width / 4 || pos.x > Screen.width * 3 / 4))//  || pos.y < Screen.height / 3 || pos.y > Screen.height * 2 / 3))
                    {             
                        float newx = (pos.x - Screen.width / 2) / 100f;
                        //float newy = (pos.y - Screen.height / 2) / 100f;

                        if (obj)
                        {
                            newx = hit.point.x;
                        }

                        //如果没到左右边界，或者到了左右边界但点击的位置不会让摄像头移出边界，则赋值新目标位置cameraNewPosition
                        if (Mathf.Abs(playercamera.position.x) >= (Ground.localScale.x * 10 / 2 - Screen.width / 2 / screenSizeCoefficient))
                        {
                            if (Mathf.Abs(newx) < Mathf.Abs(playercamera.position.x))
                            {
                                cameraNewPosition = new Vector3(newx, playercamera.position.y, playercamera.position.z); //newy);
                            }
                        }
                        else
                        {
                            cameraNewPosition = new Vector3(newx, playercamera.position.y, playercamera.position.z); //newy);
                        }
                    }
                    else
                    {
                        cameraNewPosition = playercamera.position;
                    }



                    //如果点到城市并且没有事先选择队伍
                    if (!teamSelected && obj.parent && obj.parent.GetComponent<CityObj>())
                    {
                        CityObj aCity = obj.parent.GetComponent<CityObj>();
                        if (aCity.nationality == nationality && !aCity.fighting)
                        {
                            //teamController.CityGetSpotlight(aCity); //城市获得聚光灯
                            cityController.cityObj = aCity;
                            cityController.CityMenu();
                        }
                    }
                    //如果点到队伍并且没有事先选择队伍
                    else if (!teamSelected && obj.parent && obj.parent.GetComponent<TeamObj>())
                    {
                        selectedTeam = obj.parent.GetComponent<TeamObj>();
                        //如果是敌对队伍或带队英雄被沉默，则队伍不可操作
                        HeroObj teamHero = WorldObj.MatchHero(selectedTeam.hero);
                        if (selectedTeam.nationality != nationality 
                            || (!teamHero || teamHero.heroskill == WorldObj.HeroSkill.none) || selectedTeam.hero == "")
                        {
                            return;
                        }
                        teamController.teamObj = selectedTeam;
                        //打开信息提示，使已选择标志为真
                        teamController.TeamHint();
                        teamSelected = true;

                    }
                    //如果点到城市并且事先选择了队伍
                    else if (teamSelected && obj.parent && obj.parent.GetComponent<CityObj>())
                    {
                        if (teamController.clickSkill)
                        {
                            return;
                        }

                        if (selectedTeam)
                        {
                            selectedTeam.targetCity = obj.parent.GetComponent<CityObj>();
                            selectedTeam.targetTeam = null;
                        }
                        teamController.TeamToCity(obj.parent.GetComponent<CityObj>());
                        //关闭信息提示，使已选择标志为假
                        teamSelected = false;
                        teamController.TeamHintClose();
                    }
                    //如果点到队伍并且事先选择了队伍
                    else if (teamSelected && obj.parent && obj.parent.GetComponent<TeamObj>())
                    {
                        if ( selectedTeam && obj.parent.GetComponent<TeamObj>() != selectedTeam)
                        {
                            selectedTeam.targetTeam = obj.parent.GetComponent<TeamObj>();
                            selectedTeam.targetCity = null;
                        }
                        //teamController.GoToPosition(hit.point);
                        /*
                        switch (selectedTeam.soldiertype)
                        {
                            case WorldObj.SoldierType.SwordMan:
                            case WorldObj.SoldierType.Archer:
                            case WorldObj.SoldierType.Cavalry:
                            case WorldObj.SoldierType.Tank:
                                teamController.GoToPosition(hit.point);

                                break;
                            //是工程兵则走到指定位置开始建造
                            case WorldObj.SoldierType.Worker:
                                teamController.GoAndBuild(hit.point);
                                break;
                            case WorldObj.SoldierType.Griffin:
                                break;
                            default:
                                break;
                        }
                        */
                        switch (selectedTeam.soldiertype)
                        {
                            case WorldObj.SoldierType.SwordMan:
                            case WorldObj.SoldierType.Archer:
                            case WorldObj.SoldierType.Cavalry:
                            case WorldObj.SoldierType.Tank:                                
                            case WorldObj.SoldierType.Worker:
                                if (!teamController.clickSkill)
                                {
                                    teamController.TeamToTeam(obj.parent.GetComponent<TeamObj>());
                                }                                
                                break;
                            case WorldObj.SoldierType.Griffin:
                                break;
                            default:
                                break;
                        }

                        teamSelected = false;
                        //使用英雄技能
                        TeamObj objTeam = obj.parent.GetComponent<TeamObj>();
                        teamController.HeroUseSkill(objTeam);
                        teamController.TeamHintClose();                        
                    }
                    //如果点到地上并且事先选择了队伍
                    else if (teamSelected && obj && obj == Ground)
                    {
                        if (selectedTeam)
                        {
                            selectedTeam.targetTeam = null;
                            selectedTeam.targetCity = null;
                        }

                        switch (selectedTeam.soldiertype)
                        {
                            case WorldObj.SoldierType.SwordMan:
                            case WorldObj.SoldierType.Archer:
                            case WorldObj.SoldierType.Cavalry:
                            case WorldObj.SoldierType.Tank:
                                teamController.GoToPosition(hit.point);
                                break;
                            //是工程兵则走到指定位置开始建造
                            case WorldObj.SoldierType.Worker:
                                teamController.GoAndBuild(hit.point); 
                                break;
                            case WorldObj.SoldierType.Griffin:
                                break;
                            default:
                                break;
                        }

                        teamSelected = false;

                        teamController.TeamHintClose();

                        //如果工兵队发现位置不可建造，则重新选择建造地点
                        if (reGoBuild)
                        {
                            teamSelected = true;
                            teamController.clickSkill = true;
                            reGoBuild = false;
                        }

                    }
                    //如果点到侦查模型上
                    else if (obj && obj == teamScout)
                    {
                        teamSelected = false;
                        teamController.TeamHintClose();
                        //如果有信息来源，则聚光信息来源，摄像头移动到其位置
                        if (WorldObj.scoutInfoSourceObj)
                        {
                            TeamObj teamInfoSource = WorldObj.scoutInfoSourceObj.GetComponent<TeamObj>();
                            CityObj cityInfoSource = WorldObj.scoutInfoSourceObj.GetComponent<CityObj>();
                            if (teamInfoSource) //如果是队伍信息来源
                            {
                                //teamController.TeamGetSpotlight(teamInfoSource);
                                cameraNewPosition.x = teamInfoSource.transform.position.x;
                            }
                            else if (cityInfoSource) //如果是城市信息来源
                            {
                                //teamController.CityGetSpotlight(cityInfoSource);
                                cameraNewPosition.x = cityInfoSource.transform.position.x;
                            }
                        }
                    }

                }
            }
                        
        }

        private void LateUpdate()
        {
            if (!photonView.isMine)
            {
                return;
            }        
            //摄像头移动
            if (playercamera && playercamera.transform.position != cameraNewPosition)
            {
                playercamera.transform.position = Vector3.Lerp(playercamera.transform.position, cameraNewPosition, 1f * Time.deltaTime);
                //如果到了左右边界，则停止移动摄像头
                if (Mathf.Abs(playercamera.transform.position.x) >= (Ground.localScale.x * 10 / 2 - Screen.width / 2 / screenSizeCoefficient))
                {
                    cameraNewPosition = playercamera.transform.position;
                }
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(nationality);

            }
            else
            {
                nationality = (WorldObj.Nationality)stream.ReceiveNext();

            }
        }
    }
}
