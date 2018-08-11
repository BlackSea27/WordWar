using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Com.Aionline.WordsCities
{
    public class TeamController : Photon.MonoBehaviour
    {
        #region Public Var
        //英雄技能选择范围预制体
        public Transform selectRange;
        //英雄技能冷却遮罩预制体
        public Transform heroSkillCoolDownRange;
        //英雄技能准备完成提示预制体
        public Transform heroSkillReadyHit;

        //选择的队伍
        public TeamObj teamObj { get; set; }
        //在 去建造 协程中的队伍对象数组
        public List<TeamObj> builders { get; set; }
        //正在建造中的新队伍列表（逐配件激活中的新队伍，如Tank）
        public List<GameObject> newTeams { get; set; }
        //当前被建造队伍序号
        public int newTeamIndex;
        //当前被建造队伍的模型列表
        public List<Transform> models { get; set; }
        public int modelIndex;

        //英雄技能选择范围
        public Transform selectRangeObj { get; set; }
        //建造提示模型
        public Transform hintModel;

        #endregion

        #region Private Var
        //当前的国家对象
        CountryObj countryObj;
        //城市控制组件
        CityController cityController;

        //当前的单词对象
        //WordObj currentWord;

        //屏幕下方UI
        Transform WorldUI;
        Transform teamPanel;
        //Button heroInfoButton;
        Button heroSkillButton;
        Text teamInfoText;

        //点击了英雄技能按钮
        public bool clickSkill = false;
        #endregion


        void Start()
        {
            //找到当前的国家对象组件
            countryObj = GetComponent<CountryObj>();
            //找到城市控制组件
            cityController = GetComponent<CityController>();
            //建造者列表
            builders = new List<TeamObj>();
            //被建造中的队伍模型（如Tank）列表
            newTeams = new List<GameObject>();
            //待激活的模型列表（根据国籍和兵种从newTeams中取得）
            models = new List<Transform>();

            if (!photonView.isMine)
            {
                return;
            }

            //UI控件定义
            WorldUI = GameObject.Find("PlayerCamera").transform.Find("WorldUI(Clone)").transform;
            teamPanel = WorldUI.Find("TeamPanel");
            //heroInfoButton = teamPanel.Find("HeroInfoButton").GetComponent<Button>();
            heroSkillButton = teamPanel.Find("HeroSkillButton").GetComponent<Button>();
            teamInfoText = teamPanel.Find("TeamInfoText").GetComponent<Text>();

            //点击事件添加
            //AddHeroInfoButtonListener();
            AddHeroSkillButtonListener();
        }


        #region Public Methods
        /// <summary>
        /// 队伍动作选择提示
        /// </summary>
        public void TeamHint()
        {
            // 队伍获得聚光灯
            //TeamGetSpotlight(teamObj);
            // 队伍技能板激活并加载队伍数据
            teamPanel.gameObject.SetActive(true);
            LoadTeamDataToTeamPanel();

            //技能准备就绪提示
            if (heroSkillButton.enabled == true && teamObj.nationality == countryObj.nationality)
            {
                HeroSkillReadyHit();
            }            
        }

        /// <summary>
        /// 加载队伍数据到队伍面板
        /// </summary>
        void LoadTeamDataToTeamPanel()
        {
            teamInfoText.color = Color.yellow;
            teamInfoText.text = "英雄：" + teamObj.hero + " 兵种：" + WorldObj.SoldierTypeDictionary[teamObj.soldiertype] + "\r\n"
                                + "士兵数量：" + teamObj.soldiernumber + " 军心：" + teamObj.teammorale;
            //WordObj teamHeroWord = WorldObj.MatchWord(teamObj.hero);
            //heroInfoButton.image.sprite = teamHeroWord.sprite;
            HeroObj teamHero = WorldObj.MatchHero(teamObj.hero);
            Text skillButtonText = heroSkillButton.transform.Find("Text").GetComponent<Text>();

            switch (teamObj.soldiertype)
            {
                case WorldObj.SoldierType.none:            
                case WorldObj.SoldierType.SwordMan:              
                case WorldObj.SoldierType.Archer:                   
                case WorldObj.SoldierType.Cavalry:
                    skillButtonText.text = WorldObj.HeroSkillDictionary[teamHero.heroskill];
                    break;
                case WorldObj.SoldierType.Tank:
                    skillButtonText.text = "停用";
                    break;
                case WorldObj.SoldierType.Worker:
                    skillButtonText.text = "建造";
                    break;
                case WorldObj.SoldierType.Griffin:
                    break;
                default:
                    break;
            }
            
            heroSkillButton.transform.Find("Text").GetComponent<Text>().color = Color.red;
        }

        /// <summary>
        /// 队伍动作选择提示关闭
        /// </summary>
        public void TeamHintClose()
        {
            clickSkill = false;
            if (selectRangeObj)
            {
                Destroy(selectRangeObj.gameObject);
            }
            /*
            if (teamObj.hintModel)
            {
                Destroy(teamObj.hintModel.gameObject,2f);
            }
            */
            teamPanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// 向城市行军或入城
        /// </summary>
        /// <param name="_city">城市</param>
        public void TeamToCity(CityObj _city)
        {
            if (teamObj)
            {
                if (Vector3.Distance(teamObj.transform.position, _city.transform.position)> WorldObj.TeamGoinCityDistance)
                {
                    GoToPosition(_city.transform.position);
                }
                else
                {
                    GoInCity(_city);
                }
            }
        }
        /// <summary>
        /// 入城，队伍解散，士兵归城
        /// </summary>
        /// <param name="_city">要进入的城市</param>
        public void GoInCity(CityObj _city)
        {
            //如果是己方城市或者是未被占领城市，则入城(Tank队伍不能入城)
            if ((_city.nationality == teamObj.nationality || _city.nationality == WorldObj.Nationality.none)
                && teamObj.soldiertype != WorldObj.SoldierType.Tank)
            {
                teamObj.CaptureCity(_city);
            }
        }

        /// <summary>
        /// 向队伍进军
        /// </summary>
        /// <param name="_team">队伍</param>
        public void TeamToTeam(TeamObj _team)
        {
            if (teamObj)
            {
                if (Vector3.Distance(teamObj.transform.position, _team.transform.position) > WorldObj.TeamGoinTeamDistance)
                {
                    GoToPosition(_team.transform.position);
                }
                else
                {
                    GoInTeam(_team);
                }
            }

        }


        /// <summary>
        /// 入队，本队伍解散，士兵归装载型队伍
        /// </summary>
        /// <param name="_team">要进入的装载型队伍</param>
        public void GoInTeam(TeamObj _team)
        {
            //如果是己方队伍并且是被建造中未解锁的装载型队伍，则入队(Tank队伍不能入队)
            if (_team.nationality == teamObj.nationality && _team.soldiertype == WorldObj.SoldierType.Tank && _team.locking
                && teamObj.soldiertype != WorldObj.SoldierType.Tank)
            {
                _team.UpdateTeamHero(teamObj.hero);
                _team.UpdateSoldierNumber(teamObj.soldiernumber);
                _team.UpdateSliderMaxValue(teamObj.soldiernumber);
                _team.UpdateSliderValue(teamObj.soldiernumber);
                _team.UpdateDriverSoldierType(teamObj.soldiertype);
                teamObj.DestroySelf(0f);
                //同步解锁被造队伍模型
                photonView.RPC("LockUnLockBuildingTeamRPC", PhotonTargets.All, newTeams.IndexOf(_team.gameObject),false);
                //如果该队伍是空军，则提高位置
                if (_team.features.Contains(WorldObj.TeamFeature.AirForce))
                {
                    _team.transform.position += new Vector3(0f, 2f, 0f);
                }
            }
        }


        /// <summary>
        /// 释放队伍，本队锁定，本队英雄重新带队
        /// </summary>
        /// <param name="_team">要释放队伍的装载型队伍</param>
        public void GoOutTeam(TeamObj _team)
        {
            if (_team && _team.soldiertype == WorldObj.SoldierType.Tank && !_team.locking)
            {                
                cityController.BuildNewTeam(_team.nationality, _team.soldiernumber, _team.hero, _team.driversoldiertype, null, _team.transform.position);
                _team.UpdateTeamHero("");    
                _team.UpdateSoldierNumber(1000);    
                _team.UpdateSliderMaxValue(1000);    
                _team.UpdateSliderValue(1000);    
                _team.UpdateDriverSoldierType(WorldObj.SoldierType.none);
                photonView.RPC("LockUnLockBuildingTeamRPC", PhotonTargets.All, newTeams.IndexOf(_team.gameObject), true);  
                //如果该队伍是空军，则减低位置    
                if (_team.features.Contains(WorldObj.TeamFeature.AirForce))    
                {    
                    StopCoroutine(_team.coroutineFollow);    
                    StopCoroutine(_team.coroutineTurnToTarget);    
                    _team.transform.position -= new Vector3(0f, 2f, 0f);    
                }    
                
            }

        }


         /// <summary>       
         /// 行军到某位置       
         /// </summary>       
         /// <param name="_position">某位置</param>       
        public void GoToPosition(Vector3 _position)       
        {
            //如果技能选择目标状态关闭，则队伍前进到某位置
            if (teamObj && !clickSkill)
            {
                //停止 去建造 协程，并销毁提示建造模型
                if (teamObj.coroutineGoAndBuild != null)
                {
                    StopCoroutine(teamObj.coroutineGoAndBuild);
                    teamObj.coroutineGoAndBuild = null;
                }
                
                if (teamObj.hintModel)
                {
                    Destroy(teamObj.hintModel.gameObject);
                }

                //teamObj.GetComponent<NavMeshAgent>().SetDestination(_position);

                teamObj.Follow(_position);

            }
        }

        /// <summary>
        /// 英雄使用建造技能，每个英雄只要带队的工人士兵，其技能都会被替换为建造技能
        /// </summary>
        public void HeroUseBuildSkill()
        {
            cityController.BuildWeapon();
        }

        /// <summary>
        /// 英雄使用释放队伍技能，每个英雄只要带队的TANK士兵，其技能都会被替换为释放队伍技能
        /// </summary>
        public void HeroUseTeamOutSkill()
        {
            GoOutTeam(teamObj);
        }


        //模型跟随闪烁提示建造(在ModelController最后调用）
        public void ModelFlashHint(Transform _hintModel, Transform _ownerModel)
        {
            teamObj.coroutineHintModelFlash = StartCoroutine(ModelFlashHintIE(_hintModel, _ownerModel));
        }
        IEnumerator ModelFlashHintIE(Transform _hintModel, Transform _ownerModel)
        {
            while (_hintModel && _ownerModel)
            {
                yield return new WaitForSeconds(0.5f);
                if (!_hintModel || !_ownerModel)
                {
                    break;
                }
                foreach (var item in _hintModel.GetComponentsInChildren<MeshRenderer>())
                {
                    item.enabled = false;
                }

                yield return new WaitForSeconds(0.5f);
                if (!_hintModel || !_ownerModel)
                {
                    break;
                }
                foreach (var item in _hintModel.GetComponentsInChildren<MeshRenderer>())
                {
                    item.enabled = true;
                }
                if (!_hintModel || !_ownerModel)
                {
                    break;
                }
                _hintModel.position = _ownerModel.position + new Vector3(0f,1.5f,2f);                
            }
            if (!_ownerModel && _hintModel)
            {
                Destroy(_hintModel.gameObject);
            }
        }                

        /// <summary>
        /// 队伍走到一个位置并开始建造
        /// </summary>
        public void GoAndBuild(Vector3 _buildPosition)
        {
            if (!clickSkill)
            {
                GoToPosition(_buildPosition);
                return;
            }
            //clickSkill = false;
            //开始一个新的去指定位置建造协程
            //保存该工人到builders列表并获取其序号在后面的协程中用
            if (!builders.Contains(teamObj))
            {
                builders.Add(teamObj);
            }            
            int builderIndex = builders.IndexOf(teamObj);
            builders[builderIndex].coroutineGoAndBuild = StartCoroutine(GoAndBuildIE(builderIndex,_buildPosition));
        }
        IEnumerator GoAndBuildIE(int _builderIndex,Vector3 _buildPosition)
        {
            UIController uiController = countryObj.GetComponent<UIController>();
            //测试是否能建造
            if (!CanBuild(_builderIndex ,_buildPosition))
            {
                uiController.ScoutInfoShow(countryObj.nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.CannotBuild, builders[_builderIndex].gameObject);
                //无法建造警告
                CannotBuildWarning(_builderIndex,_buildPosition);
                //则设置该标志为真，使得再次点击可以直接重选建造位置
                countryObj.reGoBuild = true;
                yield break;                
            }
            //开始行走向建造目标位置，移动提示模型到该位置并使其半透明
            if (builders[_builderIndex].hintModel)
            {
                builders[_builderIndex].hintModel.position = _buildPosition;
                StopCoroutine(builders[_builderIndex].coroutineHintModelFlash);
                builders[_builderIndex].coroutineHintModelFlash = null;
                //使模型可见
                Funcs.SetObjectVisuality(builders[_builderIndex].hintModel,true);
                //使模型半透明
                Funcs.ChangeObjectAlphaValue(builders[_builderIndex].hintModel, 0.4f);
            }
            //队伍开始行走直到接近目标位置            
            builders[_builderIndex].GetComponent<NavMeshAgent>().SetDestination(_buildPosition);

            uiController.ScoutInfoShow(countryObj.nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.GoBuild, builders[_builderIndex].gameObject);
      
            while (Vector3.Distance(builders[_builderIndex].transform.position, _buildPosition) > 1.2f)
            {
                yield return new WaitForSeconds(Time.deltaTime*75);
            }

            //测试是否能建造
            //开始建造
            if (!CanBuild(_builderIndex, _buildPosition))
            {
                uiController.ScoutInfoShow(countryObj.nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.CannotBuild, builders[_builderIndex].gameObject);

                yield break;
            }

            cityController.BuildNewTeam(builders[_builderIndex].nationality, 1000, "", WorldObj.SoldierType.Tank, builders[_builderIndex].partsnames, _buildPosition);            
            //保存并得到保存的已建造的队伍序号    
            photonView.RPC("SaveNewTeamRPC",PhotonTargets.All, null);    
            //马上取出该序号防止其他进程改变其值    
            int thisNewTeamIndex = newTeamIndex;    
            //将Work带的配件表赋值给新建造的队伍，以便在新建队伍的初始化中调用    
            photonView.RPC("GivePartsNamesRPC", PhotonTargets.All, builders[_builderIndex].partsnames);    
            //开始建造后，使得工程队转向建造位置，静止不动并不可选取    
            builders[_builderIndex].transform.LookAt(new Vector3(_buildPosition.x, builders[_builderIndex].transform.position.y, _buildPosition.z));    
            //锁住队伍    
            LockUnLockTeam(builders[_builderIndex], true);    
            //准备建造模型逐步建造    
            photonView.RPC("PrepareStepBuildRPC", PhotonTargets.All,WorldObj.SoldierType.Tank, builders[_builderIndex].nationality);    
            //逐步建造队伍模型    
            StepBuild(thisNewTeamIndex, _builderIndex, builders[_builderIndex].partsnames, 10f);    
            

            //删除提示模型
            if (builders[_builderIndex].hintModel)
            {
                Destroy(builders[_builderIndex].hintModel.gameObject);
            }

            //协程结束则清空协程变量
            builders[_builderIndex].coroutineGoAndBuild = null;
        }
        /// <summary>
        /// //测试是否能建造
        /// </summary>
        /// <param name="_buildPosition">建造位置</param>
        /// <returns></returns>
        bool CanBuild(int _builderIndex,Vector3 _buildPosition)
        {
            bool canBuild = true;
            foreach (var item in WorldObj.allCities)
            {
                if (Vector3.Distance(item.transform.position, _buildPosition) < 1.5f)
                {
                    canBuild = false;
                    break;
                }
            }
            foreach (var item in WorldObj.allTeams)
            {
                if (Vector3.Distance(item.transform.position, _buildPosition) < 1.5f && item != builders[_builderIndex])
                {
                    canBuild = false;
                    break;
                }
            }
            return canBuild;
        }
        /// <summary>
        /// 无法建造警告
        /// </summary>
        /// <param name="_buildPosition"></param>
        /// <returns></returns>
        void CannotBuildWarning(int _builderIndex, Vector3 _buildPosition) 
        {
            if (!builders[_builderIndex].hintModel)
            {
                return;
            }
            Transform warningHintModel = Instantiate(builders[_builderIndex].hintModel,_buildPosition,Quaternion.Euler(35f,0f,0f));
            Destroy(warningHintModel.gameObject, 1f);
            //使警告模型可见
            Funcs.SetObjectVisuality(builders[_builderIndex].hintModel, true);
            //使模型半透明 
            Funcs.ChangeObjectAlphaValue(warningHintModel, 0.3f);


            /*
            Vector3 hintModelPosition = teamObj.hintModel.position;
            teamObj.hintModel.position = _buildPosition;
            //使模型半透明 
            Funcs.ChangeObjectAlphaValue(teamObj.hintModel, 0.3f);
            yield return new WaitForSeconds(Time.deltaTime * 150f);
            //恢复模型透明度
            Funcs.ChangeObjectAlphaValue(teamObj.hintModel, 1f);
            teamObj.hintModel.position = hintModelPosition;
            */
        }
        /// <summary>
        /// 锁住和解锁队伍
        /// </summary>
        /// <param name="_lock"></param>
        void LockUnLockTeam(TeamObj _teamObj,bool _lock)
        {            
            _teamObj.GetComponent<NavMeshAgent>().SetDestination(_teamObj.transform.position);
            _teamObj.PlayAnimator(WorldObj.AnimatorAction.Victory, !_lock);
            _teamObj.PlayAnimator(WorldObj.AnimatorAction.Work, _lock);
            _teamObj.GetComponent<NavMeshAgent>().isStopped = _lock;
        }

        /// <summary>
        /// 同步保存新建造的队伍
        /// </summary>
        /// <returns>返回保存的已建造的队伍序号</returns>
        [PunRPC]
        void SaveNewTeamRPC()
        {
            newTeams.Add(cityController.newTeam);            
            newTeamIndex = newTeams.IndexOf(cityController.newTeam);
        }
        //将建造工人的partsnames赋值给被建队伍
        [PunRPC]
        void GivePartsNamesRPC(string[] _partsnames)
        {
            //newTeams[newTeamIndex].GetComponent<TeamObj>().partsnames = builders[_builderIndex].partsnames;            
            cityController.newTeam.GetComponent<TeamObj>().partsnames = _partsnames;
        }

        /// <summary>
        /// 准备逐步建造模型
        /// </summary>
        /// <param name="_soldierType">要建造的模型类型</param>
        [PunRPC]
        void PrepareStepBuildRPC(WorldObj.SoldierType _soldierType, WorldObj.Nationality _nationality)
        {
            if (!cityController)
            {
                Debug.Log("组件cityController没找到!");
            }
            //初始化刚开始建造的模型
            GameObject _newTeam = cityController.newTeam;
            if (_newTeam)
            {
                TeamObj newTeamObj = _newTeam.GetComponent<TeamObj>();
                //锁住被建队伍模型的动画和隐藏他的血条
                LockUnLockBuildingTeam(newTeamObj,true);
            }
            else
            {
                return;
            }

            switch (_soldierType)
            {
                case WorldObj.SoldierType.Tank:
                    Transform model = null;
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            model = _newTeam.transform.Find("Tank_Red");
                            break;
                        case WorldObj.Nationality.blue:
                            model = _newTeam.transform.Find("Tank_Blue");
                            break;
                        case WorldObj.Nationality.monster:
                            break;
                        default:
                            break;
                    }
                    //激活基础配件
                    cityController.SetActiveParts(model, new string[] { "Cylinder020", "Object031" });
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 锁住和解锁被建造的队伍模型
        /// </summary>
        /// <param name="_teamObj">队伍模型</param>
        /// <param name="_lock">锁住，解锁</param>
        void LockUnLockBuildingTeam(TeamObj _teamObj, bool _lock)
        {
            _teamObj.locking = _lock;
            if (_teamObj.teamHealthCanvas)
            {
                //隐藏或恢复血条
                _teamObj.teamHealthCanvas.gameObject.SetActive(!_lock);
            }

            //空军在解锁时禁用导航组件
            if (_teamObj.GetComponent<NavMeshAgent>() && _teamObj.features.Contains(WorldObj.TeamFeature.AirForce))
            {
                _teamObj.GetComponent<NavMeshAgent>().enabled = _lock;
            }           
            //暂停或启动所有动画
            Animator[] modelAnimators = _teamObj.GetComponentsInChildren<Animator>();
            foreach (var item in modelAnimators)
            {
                item.enabled = !_lock;
            }
            Animation[] modelAnimations = _teamObj.GetComponentsInChildren<Animation>();
            foreach (var item in modelAnimations)
            {
                item.enabled = !_lock;
            }
        }
        /// <summary>
        /// 开始逐步建造
        /// </summary>
        /// <param name="_partsnames">要激活的配件名数组</param>
        /// <param name="_buildTime">建造每个配件需要的时间</param>
        void StepBuild(int _newTeamIndex,int _builderIndex, string[] _partsnames, float _buildTime)
        {
            Debug.Log("3被建队伍序列号是：" + _newTeamIndex + " 建造者序列号："+_builderIndex+"      建造字符串列表"+ _partsnames.ToStringFull());
            builders[_builderIndex].coroutineStepBuild = StartCoroutine(StepBuildIE(_newTeamIndex, _builderIndex, _partsnames, _buildTime));
        }
        /// <summary>
        /// 开始逐步建造队伍的协程
        /// </summary>
        /// <param name="_newTeamIndex"></param>
        /// <param name="_partsnames"></param>
        /// <param name="_buildTime"></param>
        /// <returns></returns>
        IEnumerator StepBuildIE(int _newTeamIndex, int _builderIndex, string[] _partsnames, float _buildTime)
        {
            //同步得到model
            photonView.RPC("GetTeamModelRPC", PhotonTargets.All,_newTeamIndex);
            //关键语句，立刻用新变量取得临时存放的modelIndex的值，以免其他进程改变。
            int thisModelIndex = modelIndex;

            foreach (var item in _partsnames)
            {
                if (!(item == "Cylinder020" || item == "Object031"))
                {
                    yield return new WaitForSeconds(_buildTime);

                    //同步激活配件
                    photonView.RPC("PartOKRPC", PhotonTargets.All, item, _newTeamIndex, thisModelIndex);
                }                             
            }
            //解锁Worker
            LockUnLockTeam(builders[_builderIndex], false);

            /*
            //同步解锁被造队伍模型
            photonView.RPC("UnLockBuildingTeam", PhotonTargets.All,_newTeamIndex);

            //如果该队伍是空军，则提高位置
            if (newTeams[_newTeamIndex].GetComponent<TeamObj>().features.Contains(WorldObj.TeamFeature.AirForce))
            {
                newTeams[_newTeamIndex].transform.position += new Vector3(0f, 1f, 0f);
            }
            */

            //协程结束则清空协程变量
            builders[_builderIndex].coroutineStepBuild = null;
        }
        //同步解锁被造队伍模型
        [PunRPC]
        void LockUnLockBuildingTeamRPC(int _newTeamIndex, bool _lock)
        {
            //解锁被造队伍模型
            LockUnLockBuildingTeam(newTeams[_newTeamIndex].GetComponent<TeamObj>(), _lock);
        }
        /// <summary>
        /// //同步得到model
        /// </summary>
        /// <param name="_newTeamIndex">当前被建造的队伍模型序号</param>
        [PunRPC]
        void GetTeamModelRPC(int _newTeamIndex)
        {
            GameObject _newTeam = newTeams[_newTeamIndex];
            Transform model = null;
            switch (_newTeam.GetComponent<TeamObj>().soldiertype)
            {
                case WorldObj.SoldierType.Tank:
                    switch (_newTeam.GetComponent<TeamObj>().nationality)
                    {
                        case WorldObj.Nationality.red:
                            model = _newTeam.transform.Find("Tank_Red");
                            break;
                        case WorldObj.Nationality.blue:
                            model = _newTeam.transform.Find("Tank_Blue");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            models.Add(model);
            modelIndex = models.IndexOf(model);
        }
        /// <summary>
        /// 找到就激活配件
        /// </summary>
        /// <param name="_item">待查找的一个待激活的配件项字符串</param>
        [PunRPC]
        void PartOKRPC(string _item, int _newTeamIndex, int _thisModelIndex)
        {
            bool findPart = false;
            Transform finalModel = models[_thisModelIndex];

            if (finalModel)
            {
                Transform part = finalModel.Find(_item);
                if (part)
                {
                    findPart = true;
                }
                else
                {
                    for (int i = 0; i < finalModel.childCount; i++)
                    {
                        if (finalModel.GetChild(i).childCount > 0)
                        {
                            part = finalModel.GetChild(i).Find(_item);
                            if (part)
                            {
                                findPart = true;
                                break;
                            }
                        }
                    }
                }

                if (findPart)
                {
                    part.gameObject.SetActive(true);
                    if (newTeams[_newTeamIndex].GetComponent<TeamObj>().visible)
                    {
                        StartCoroutine(Funcs.SlowDisappearIE(part, 2f, 80f, true));
                    }
                    else
                    {
                        Funcs.ChangeObjectAlphaValue(part, 0f);
                    }
                    
                }
            }
        }
        


        /// <summary>
        /// 英雄使用英雄技
        /// </summary>
        /// <param name="_objTeam">目标队伍</param>
        public void HeroUseSkill(TeamObj _objTeam)
        {
            if (!clickSkill){return;}
            clickSkill = false;

            //如果本队伍是工兵，则不适用英雄技
            if (teamObj.soldiertype == WorldObj.SoldierType.Worker
                || teamObj.soldiertype == WorldObj.SoldierType.Tank)
            {
                return;
            }
            //判断目标队伍距离
            if (Vector3.Distance(_objTeam.transform.position, teamObj.transform.position)> WorldObj.SkillDistance)
            {
                return;
            }

            switch (WorldObj.MatchHero(teamObj.hero).heroskill)
            {
                case WorldObj.HeroSkill.none:
                    break;
                case WorldObj.HeroSkill.IncreaseOwnSpeed:
                    if (_objTeam.nationality == countryObj.nationality)
                    {                        
                        //急行军倒计时，计时完毕恢复初始速度
                        _objTeam.UpdateTeamNAVSpeed(WorldObj.IncreaseOwnSpeedCoefficient * _objTeam.GetComponent<NavMeshAgent>().speed);
                        StartCoroutine(TeamSpeedCountDownRestore(WorldObj.IncreaseOwnSpeedTime,_objTeam));
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                case WorldObj.HeroSkill.ReduceEnemySpeed:
                    if (_objTeam.nationality != countryObj.nationality)
                    {
                        //缓慢倒计时，计时完毕恢复初始速度
                        _objTeam.UpdateTeamNAVSpeed(WorldObj.ReduceEnemySpeedCoefficient * _objTeam.GetComponent<NavMeshAgent>().speed);
                        StartCoroutine(TeamSpeedCountDownRestore(WorldObj.ReduceEnemySpeedTime, _objTeam));
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                case WorldObj.HeroSkill.Disorient:
                    if (_objTeam.nationality != countryObj.nationality)
                    {
                        //迷惑减低军心
                        _objTeam.UpdateTeamMorale(Mathf.RoundToInt(WorldObj.DisorientCoefficient * _objTeam.GetComponent<TeamObj>().teammorale));                        
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                case WorldObj.HeroSkill.Encourage:
                    if (_objTeam.nationality == countryObj.nationality)
                    {
                        //鼓舞提升军心
                        _objTeam.UpdateTeamMorale(Mathf.RoundToInt(WorldObj.EncourageCoefficient * _objTeam.GetComponent<TeamObj>().teammorale));
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                case WorldObj.HeroSkill.Silence:
                    if (_objTeam.nationality != countryObj.nationality)
                    {   //保存原技能
                        HeroObj objHero = WorldObj.MatchHero(_objTeam.hero);
                        WorldObj.HeroSkill originalSkill = objHero.heroskill;
                        //沉默对手
                        objHero.GiveHeroSkill(WorldObj.HeroSkill.none);
                        //计时结束后恢复对手
                        StartCoroutine(HeroSkillSilenceCountDownRestore(WorldObj.SilenceTime, objHero, originalSkill));
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                case WorldObj.HeroSkill.Disease:
                    if (_objTeam.nationality != countryObj.nationality)
                    {
                        //瘟疫降低敌方队伍人数
                        _objTeam.UpdateSoldierNumber(Mathf.RoundToInt(WorldObj.DiseaseCoefficient * _objTeam.GetComponent<TeamObj>().soldiernumber));
                        // 英雄技能进入冷却
                        HeroSkillCoolDown();
                    }
                    break;
                default:
                    break;
            }            
        }
        /// <summary>
        /// 变速倒计时，计时完毕恢复初始速度
        /// </summary>
        /// <param name="_totaltime">倒计时总时间</param>
        /// <param name="_objTeam">队伍</param>
        /// <returns></returns>
        IEnumerator TeamSpeedCountDownRestore(float _totaltime,TeamObj _objTeam)
        {
            yield return new WaitForSeconds(_totaltime);
            if (_objTeam)
            {
                _objTeam.UpdateTeamNAVSpeed(_objTeam.teamspeed);
            }            
        }

        /// <summary>
        /// 沉默倒计时，计时完毕恢复
        /// </summary>
        /// <param name="_totaltime">倒计时总时间</param>
        /// <param name="_objTeam">英雄</param>
        /// <returns></returns>
        IEnumerator HeroSkillSilenceCountDownRestore(float _totaltime, HeroObj _objHero, WorldObj.HeroSkill _originalSkill)
        {
            yield return new WaitForSeconds(_totaltime);
            _objHero.GiveHeroSkill(_originalSkill);
        }
        /// <summary>
        /// 英雄技能冷却
        /// </summary>
        void HeroSkillCoolDown()
        {
            heroSkillButton.enabled = false;
            float cooltime = 0;
            switch (WorldObj.MatchHero(teamObj.hero).heroskill)
            {
                case WorldObj.HeroSkill.none:
                    cooltime = 0;
                    break;
                case WorldObj.HeroSkill.IncreaseOwnSpeed:
                    cooltime = WorldObj.IncreaseOwnSpeedCoolDown;
                    break;
                case WorldObj.HeroSkill.ReduceEnemySpeed:
                    cooltime = WorldObj.ReduceEnemySpeedCoolDown;
                    break;
                case WorldObj.HeroSkill.Disorient:
                    cooltime = WorldObj.DisorientCoolDown;
                    break;
                case WorldObj.HeroSkill.Encourage:
                    cooltime = WorldObj.EncourageCoolDown;
                    break;
                case WorldObj.HeroSkill.Silence:
                    cooltime = WorldObj.SilenceCoolDown;
                    break;
                case WorldObj.HeroSkill.Disease:
                    cooltime = WorldObj.DiseaseCoolDown;
                    break;
                default:
                    break;
            }
            StartCoroutine(HeroSkillCoolDownReady(cooltime));
        }
        IEnumerator HeroSkillCoolDownReady(float _cooltime)
        {
            Material cooldownMaterial = heroSkillButton.transform.Find("Image").GetComponent<Image>().material;
            cooldownMaterial.SetFloat("_Progress", 1);
            cooldownMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
            float timecount = _cooltime / Time.fixedDeltaTime;
            for (int i = 0; i < timecount; i++)
            {                
                cooldownMaterial.SetFloat("_Progress", i / timecount);
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            
            heroSkillButton.enabled = true;
            //冷却完成技能准备就绪提示
            HeroSkillReadyHit();
        }
        /// <summary>
        /// //冷却完成技能准备就绪提示
        /// </summary>
        void HeroSkillReadyHit()
        {
            //冷却完成技能准备就绪提示1秒            
            Transform heroSkillReadyHitObj = Instantiate(heroSkillReadyHit, heroSkillButton.transform.position, Quaternion.identity);
            heroSkillReadyHitObj.SetParent(heroSkillButton.transform);
            heroSkillReadyHitObj.localPosition = new Vector3(-50f, 50f, 0f);//修正提示的出现位置
            Destroy(heroSkillReadyHitObj.gameObject, 1f);
        }


        #endregion

        #region Private Methods 

        /*
        /// <summary>
        /// 英雄信息按钮点击事件添加
        /// </summary>
        void AddHeroInfoButtonListener()
        {
            heroInfoButton.onClick.AddListener(delegate () 
            {
                //调用英雄招募成功面板显示英雄信息
                cityController.LoadWordInfoShow(WorldObj.MatchWord(teamObj.hero), CityController.buttonAction.RecruitConfirm, true);
            });
        }
        */

        /// <summary>
        /// 英雄技能按钮点击事件添加
        /// </summary>
        void AddHeroSkillButtonListener()
        {
            heroSkillButton.onClick.AddListener(delegate ()
            {
                clickSkill = true;

                if (!teamObj)
                {
                    clickSkill = false;
                    return;
                }
                //如果带队的工程兵，则打开建造武器面板
                if (teamObj.soldiertype == WorldObj.SoldierType.Worker )
                {
                    //如果不在其他建造进程中，则可打开建造武器面板
                    if (teamObj.coroutineGoAndBuild == null
                        && teamObj.coroutineStepBuild == null)
                    {
                        HeroUseBuildSkill();
                    }
                    else
                    {
                        clickSkill = false;
                        return;
                    }
                    
                }
                else if (teamObj.soldiertype == WorldObj.SoldierType.Tank)
                {
                    HeroUseTeamOutSkill();
                }else
                {
                    //生成范围效果对象
                    if (!selectRangeObj)
                    {
                        selectRangeObj = Instantiate(selectRange, teamObj.transform.position, Quaternion.identity);
                        //借助临时变量才能改变localScale，直接Set无效，目前不知道原因
                        Vector3 scale = selectRangeObj.localScale;
                        scale.Set(WorldObj.SkillDistance, selectRange.localScale.y, WorldObj.SkillDistance);
                        selectRangeObj.localScale = scale;
                        //直接Set无效
                        //selectRangeObj.localScale.Set(WorldObj.SkillDistance, selectRange.localScale.y, WorldObj.SkillDistance);
                        selectRangeObj.SetParent(teamObj.transform);
                    }
                }

            });
        }


        #endregion
    }
}
