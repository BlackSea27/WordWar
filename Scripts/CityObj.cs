using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com.Aionline.WordsCities
{
    public class CityObj : Photon.MonoBehaviour , IPunObservable
    {
        #region Public Var
        //旗帜预制体
        //public Transform flagRed { get; set; }
        //public Transform flagBlue { get; set; }

        //国籍
        public WorldObj.Nationality nationality = WorldObj.Nationality.none;//{ set; get; }
        //单词
        public string word;//{ set; get; }
        //人口
        public int population;//{ set; get; }
        //城防
        public int citydefence;//{ set; get; }
        //士兵
        public int swordmannumber; //{ set; get; }
        public int archernumber; //{ set; get; }
        public int cavalrynumber; //{ set; get; }
        public int workernumber; //{ set; get; }

        //public Dictionary<WorldObj.SoldierType, int> soldiers;
        //武将列表
        public List<string> heros;//{ set; get; }
        //是否战争状态
        public bool fighting = false;//{ set; get; }
        //正在交战列表
        public List<string> enemies;
        #endregion

        UIController uiController;

        void Start()
        {
            //flagRed = PhotonGameManager.assetDictionary["Flag_red"].transform;
            //flagBlue = PhotonGameManager.assetDictionary["Flag_blue"].transform;

            CityStart();

            if (!photonView.isMine)
            {
                return;
            }

            AddToCities();

            GiveFlag();


        }



        /// <summary>
        /// 同步增加城市到静态所有城市对象列表
        /// </summary>
        public void AddToCities()
        {
            photonView.RPC("AddToCitiesRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        void AddToCitiesRPC()
        {
            WorldObj.allCities.Add(this);
        }


        /// <summary>
        /// 该城市对象初始化
        /// </summary>           
        void CityStart()
        {
            //初始化默认人口
            population = WorldObj.BeginPopulation;
            //soldiers = WorldObj.BeginSoldierNumber;
            swordmannumber = WorldObj.BeginSwordMan;
            archernumber = WorldObj.BeginArcher;
            cavalrynumber = WorldObj.BeginCavalry;
            workernumber = WorldObj.BeginWorker;
            citydefence = WorldObj.BeginCityDefence;

        }

        #region Public Methods

        /// <summary>
        /// 分配国籍
        /// </summary>
        /// <param name="_nationality">国籍</param>
        public void GiveNationality(WorldObj.Nationality _nationality)
        {
            photonView.RPC("GiveNationalityRPC", PhotonTargets.All, _nationality);
        }
        [PunRPC]
        public void GiveNationalityRPC(WorldObj.Nationality _nationality)
        {
            nationality = _nationality;
        }

        /// <summary>
        /// 分配国籍标志（暂时用颜色代替）
        /// </summary>
        public void GiveFlag()
        {
            photonView.RPC("GiveFlagRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        public void GiveFlagRPC()
        {
            transform.Find("Sphere").gameObject.SetActive(true);
            Material flowShaderMaterial = transform.Find("Sphere").GetComponent<MeshRenderer>().material;
            switch (nationality)
            {
                case WorldObj.Nationality.red:
                    flowShaderMaterial.color = new Color(1,0,0,0.15f);
                    break;
                case WorldObj.Nationality.blue:
                    flowShaderMaterial.color = new Color(0, 0.5f, 0.5f, 0.15f);
                    break;
                case WorldObj.Nationality.none:
                    flowShaderMaterial.color = new Color(0, 1, 0, 0.08f);                    
                    break;
                default:                    
                    break;
            }
            flowShaderMaterial.SetFloat("_Period", 3f);

            //生成旗帜
            SpawnFlag(nationality);
        }
        /// <summary>
        /// 生成旗帜
        /// </summary>
        /// <param name="_nationality">国籍</param>
        void SpawnFlag(WorldObj.Nationality _nationality)
        {
            //生成旗帜前先删除可能有的旗帜
            DestroyAllFlags();
            Transform newFlag = null;
            switch (nationality)
            {
                case WorldObj.Nationality.red:
                    newFlag = Instantiate(PhotonGameManager.assetDictionary["Flag_red"].transform, transform.position, Quaternion.Euler(0f,-90f,-45f));
                    break;
                case WorldObj.Nationality.blue:
                    newFlag = Instantiate(PhotonGameManager.assetDictionary["Flag_blue"].transform, transform.position, Quaternion.Euler(0f, -90f, -45f));
                    break;
                case WorldObj.Nationality.none:                    
                    break;
                default:
                    break;
            }
            if (newFlag)
            {
                newFlag.SetParent(transform);
                newFlag.localPosition = new Vector3(-1.3f, 1f, 1f);
            }
        }

        /// <summary>
        /// 删除旗帜
        /// </summary>
        void DestroyAllFlags()
        {
            Transform exsitFlagRed = transform.Find("Flag_red(Clone)");
            Transform exsitFlagBlue = transform.Find("Flag_blue(Clone)");
            if (exsitFlagRed)
            {
                Destroy(exsitFlagRed.gameObject);
            }
            if (exsitFlagBlue)
            {
                Destroy(exsitFlagBlue.gameObject);
            }
        }


        /// <summary>
        /// 同步更新人口
        /// </summary>
        /// <param name="_newPopulation">新人口数字</param>
        public void UpdatePopulation(int _newPopulation)
        {
            photonView.RPC("UpdatePopulationRPC", PhotonTargets.All, _newPopulation);
        }
        [PunRPC]
        public void UpdatePopulationRPC(int _newPopulation)
        {
            population = _newPopulation;
        }

        /// <summary>
        /// 同步更新城防
        /// </summary>
        /// <param name="_newDefence">新城防数字</param>
        public void UpdateDefence(int _newDefence)
        {
            photonView.RPC("UpdateDefenceRPC", PhotonTargets.All, _newDefence);
        }
        [PunRPC]
        public void UpdateDefenceRPC(int _newDefence)
        {
            citydefence = _newDefence;
        }

        /// <summary>
        /// 同步更新士兵
        /// </summary>
        public void UpdateSoldiers(int _swordman, int _archer, int _cavalry, int _worker)
        {
            photonView.RPC("UpdateSoldiersRPC", PhotonTargets.All, _swordman, _archer, _cavalry, _worker);
        }
        [PunRPC]
        public void UpdateSoldiersRPC(int _swordman, int _archer, int _cavalry, int _worker)
        {
            swordmannumber = _swordman;
            archernumber = _archer;
            cavalrynumber = _cavalry;
            workernumber = _worker;
        }

        /// <summary>
        /// 同步更新战争状态
        /// </summary>
        /// <param name="_newDefence">新战争状态</param>
        public void UpdateFighting(bool _fighting)
        {
            CountryObj selfCountry = WorldObj.MatchCountry(nationality);            
            if (selfCountry)
            {
                uiController = selfCountry.GetComponent<UIController>();
                uiController.ScoutInfoShow(nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.CityBeAttacked, this.gameObject);
            }
            
            photonView.RPC("UpdateFightingRPC", PhotonTargets.All, _fighting);
        }
        [PunRPC]
        public void UpdateFightingRPC(bool _fighting)
        {
            fighting = _fighting;
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
        /// 添加或移除武将
        /// </summary>
        /// <param name="_hero">武将单词</param>
        /// <param name="_add">添加或删除</param>
        public void AddOrRemoveHero(string _hero, bool _add)
        {
            photonView.RPC("AddOrRemoveHeroRPC", PhotonTargets.All, _hero, _add);
        }
        [PunRPC]
        public void AddOrRemoveHeroRPC(string _hero, bool _add)
        {
            if (_add)
            {
                heros.Add(_hero);
            }
            else
            {
                heros.Remove(_hero);
            }
        }



        
        //城市属性同步
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {            
            if (stream.isWriting)
            {
                stream.SendNext(nationality);
                stream.SendNext(word);
                //stream.SendNext(population);
                stream.SendNext(citydefence);

                //stream.SendNext(swordmannumber);
                //stream.SendNext(archernumber);
                //stream.SendNext(cavalrynumber);

                //因为List<string>类型无法序列化，所以需要通过转化成string[]类型实现同步
                stream.SendNext(heros.ToArray());


            }
            else if(stream.isReading)
            {
                nationality = (WorldObj.Nationality)stream.ReceiveNext();
                word = (string)stream.ReceiveNext();
                //population = (int)stream.ReceiveNext();
                citydefence = (int)stream.ReceiveNext();

                //swordmannumber = (int)stream.ReceiveNext();
                //archernumber = (int)stream.ReceiveNext();
                //cavalrynumber = (int)stream.ReceiveNext();

                //因为List<string>类型无法序列化，所以需要通过转化成string[]类型实现同步
                string[] strs = (string[])stream.ReceiveNext();
                heros = strs.ToList();
            }
        }
        

        #endregion




    }
}
