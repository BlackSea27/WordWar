using System.Collections;
using System.Collections.Generic;
using System.IO;
/*
#if UNITY_STANDALONE_WIN
using NAudio.Wave;
#endif
*/
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace Com.Aionline.WordsCities
{
    public class WorldObj : Photon.MonoBehaviour //,IPunObservable
    {
        #region Const
        /// <summary>
        /// 网络文件服务器空间根目录
        /// </summary>
        public const string FileServerRootPath = "http://192.168.0.102/";
        /// <summary>
        /// 游戏开始前的存放文件夹
        /// </summary>
        public const string BeginUIPath = "http://192.168.0.102/BeginUI/";

        /// <summary>
        /// AssetBundle存放的网络路径
        /// </summary>
        //Windows路径
        public const string AssetBundlePath = "http://192.168.0.102/AssetBundles/Windows/";
        //Android路径
        public const string AssetBundleAndroidPath = "http://192.168.0.102/AssetBundles/Android/";

        /// <summary>
        /// AssetBundle存放的本地路径
        /// </summary>
        //Windows路径
        public const string AssetBundleLocalPath = "/AssetBundles/Windows/";
        //Android路径
        public const string AssetBundleAndroidLocalPath = "/AssetBundles/Android/";

        /// <summary>
        /// 单词包存放的网络路径
        /// </summary>
        public const string WordBookPath = "http://192.168.0.102/WordBooks/";
        //子路径
        //单词内容（包括词形，词意，音标，例句）
        public const string WordContentPath = "http://192.168.0.102/WordBooks/Content/";

        /// <summary>
        /// 单词包的本地路径
        /// </summary>
        public const string WordBookLocalPath = "/WordBooks/";
        //子路径
        //单词内容（包括词形，词意，音标，例句）
        public const string WordContentLocalPath = "/WordBooks/Content/";

        /// <summary>
        /// 单词包格式后缀名
        /// </summary>
        public const string WordpackeFormat = ".txt";
        /// <summary>
        /// 图片格式后缀名
        /// </summary>
        public const string ImageFormat = ".jpg";
        /// <summary>
        /// 读音格式后缀名
        /// </summary>
        public const string SoundFormat = ".wav";
        /// <summary>
        /// 用来记录信息的服务器和本地的XML文件名
        /// </summary>
        public const string XMLRecords = "Records.xml";


        /// <summary>
        /// 城市数量
        /// </summary>
        public const int CityCount = 10;

        /// <summary>
        /// 武将数量
        /// </summary>
        public const int HeroCount = 15;



        /// <summary>
        /// 武将政治上下限
        /// </summary>
        public const int HeroPoliticsMax = 100;
        public const int HeroPoliticsMin = 40;

        /// <summary>
        /// 武将武力上下限
        /// </summary>
        public const int HeroForceMax = 100;
        public const int HeroForceMin = 20;

        /// <summary>
        /// 武将智力上下限
        /// </summary>
        public const int HeroIntelligenceMax = 100;
        public const int HeroIntelligenceMin = 40;

        /// <summary>
        /// 武将统治力上下限
        /// </summary>
        public const int HeroDominanceMax = 50000;
        public const int HeroDominanceMin = 5000;

        /// <summary>
        /// 武将统治力计算系数
        /// </summary>
        public const float HeroDominanceCoefficient = 0.002f;

        /// <summary>
        /// 武将忠诚度上下限
        /// </summary>
        public const int HeroAllegianceMax = 100;
        public const int HeroAllegianceMin = 30;

        /// <summary>
        /// 武将魅力值上下限
        /// </summary>
        public const int HeroCharmMax = 100;
        public const int HeroCharmMin = 20;

        /// <summary>
        /// 武将价格系数
        /// </summary>
        public const int HeroPriceCoefficient = 10;

        /// <summary>        
        /// 税率常量        
        /// </summary>        
        public const float BeginTax = 0.017f;

        /// <summary>
        /// 初始金钱
        /// </summary>
        public const int BeginMoney = 8000;

        /// <summary>
        /// 初始人口
        /// </summary>
        public const int BeginPopulation = 3000;

        /// <summary>
        /// 人口增长系数
        /// </summary>
        public const int PopulationIncreaseCoefficient = 3000;

        /// <summary>
        /// 初始士兵数量
        /// </summary>
        public const int BeginSwordMan = 40000;
        public const int BeginArcher = 20000;
        public const int BeginCavalry = 5000;
        public const int BeginWorker = 10000;

        /// <summary>
        /// 征兵费用系数（乘以征兵数量得到征兵费用）
        /// </summary>
        public const float ConscriptFeeCoefficient = 1;
        /// <summary>
        /// 初始城防
        /// </summary>
        public const int BeginCityDefence = 100;

        /// <summary>
        /// 城防费用系数（乘以修筑城防数值得到城防费用）
        /// </summary>
        public const float CityDefenceFeeCoefficient = 100;

        /// <summary>
        /// 金钱封顶常量
        /// </summary>
        public const int TopMoney = 100000;

        /// <summary>
        /// 队伍建立费用
        /// </summary>
        public const int BuildTeamFee = 1080;

        /// <summary>
        /// 队伍速度
        /// </summary>
        public const float BeginTeamSpeed = 2f;
        public const float SwordManSpeed = 0.3f;
        public const float ArcherSpeed = 0.3f;
        public const float CavalrySpeed = 0.6f;
        public const float WorkerSpeed = 0.3f;
        public const float IncreaseOwnSpeedCoefficient = 2f;
        public const float ReduceEnemySpeedCoefficient = 0.5f;
        public const float IncreaseOwnSpeedTime = 4f;
        public const float ReduceEnemySpeedTime = 4f;

        /// <summary>
        /// 队伍初始军心值
        /// </summary>
        public const int BeginTeamMorale = 80;

        /// <summary>
        /// 队伍入城距离
        /// </summary>
        public const float TeamGoinCityDistance = 2f;

        /// <summary>
        /// 队伍进入装载型队伍距离
        /// </summary>
        public const float TeamGoinTeamDistance = 1.5f;

        /// <summary>
        /// Monster Griffin 的金钱奖励
        /// </summary>
        public const int GriffinBonus = 10000;

        /// <summary>
        /// 技能迷惑的系数（降低军心，系数越大，效果越小）
        /// </summary>
        public const float DisorientCoefficient = 0.8f;

        /// <summary>
        /// 技能鼓舞的系数（提升军心，系数越大，效果越大）
        /// </summary>
        public const float EncourageCoefficient = 1.15f;

        /// <summary>
        /// 技能沉默的持续时间
        /// </summary>
        public const float SilenceTime = 4f;

        /// <summary>
        /// 技能瘟疫的系数（降低地方队伍士兵数，系数越大，效果越小）
        /// </summary>
        public const float DiseaseCoefficient = 0.7f;

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public const float IncreaseOwnSpeedCoolDown = 20f;
        public const float ReduceEnemySpeedCoolDown = 20f;
        public const float DisorientCoolDown = 20f;
        public const float EncourageCoolDown = 20f;
        public const float SilenceCoolDown = 20f;
        public const float DiseaseCoolDown = 20f;



        /// <summary>
        /// 攻击距离
        /// </summary>
        public const float AttackDistance = 2f;
        /// <summary>
        /// 英雄施法距离
        /// </summary>
        public const float SkillDistance = 6f;

        /// <summary>
        /// 兵种克制系数（系数越大，效果越小）
        /// </summary>
        public const float restraintCoefficient = 0.8f;


#endregion

        #region Enum var 枚举变量
        /// <summary>       
        /// 国家枚举       
        /// </summary>       
        public enum Nationality       
         {       
             none,       
             red,       
             blue,
             monster
        }
        /// <summary>
        /// 武将技枚举
        /// </summary>
        public enum HeroSkill
        {
            none,
            IncreaseOwnSpeed,
            ReduceEnemySpeed,
            Disorient,
            Encourage,
            Silence,
            Disease
        }
        /// <summary>
        /// 士兵种类枚举
        /// </summary>
        public enum SoldierType
        {
            none,
            SwordMan,
            Archer,
            Cavalry,
            Tank,
            Worker,
            Griffin
        }

        /// <summary>
        /// 队伍特征
        /// </summary>
        public enum TeamFeature
        {
            Army,           //陆军
            AirForce,       //空军
            Tower,          //塔
            AttackGround,   //对地攻击
            AttackAir,      //对空攻击
        }

        /// <summary>
        /// 动画动作
        /// </summary>
        public enum AnimatorAction
        {
            none,
            Idle,
            Walk,
            Run,
            GetHit,
            Attack01,
            Attack02,
            Victory,
            Death,
            Work
        }
        /// <summary>
        /// 侦查信息类型枚举
        /// </summary>
        public enum ScoutInfoType
        {     
            none,
            TeamBeBuilt,            
            EnemyBeFound,
            TeamBeAttacked,
            CityBeAttacked,
            CannotBuild,
            WhereBuild,
            GoBuild
        }
        #endregion

        #region Public var 公有变量
            //队伍预制体
        public GameObject team { set; get; }
        #endregion

        #region Public var 公有静态变量
        //是否是对抗人工智能
        public static bool AIWar = false;
        //是否是匹配对战
        public static bool MatchWar = false;
        //UI界面是否在运行
        public static bool UIRun = false;
        //国家对象列表
        public static List<CountryObj> allCountries = new List<CountryObj>();
        //城市对象列表
        public static List<CityObj> allCities = new List<CityObj>();
        //单词对象列表
        public static List<WordObj> allWords = new List<WordObj>();
        //英雄对象列表
        public static List<HeroObj> allHeros = new List<HeroObj>();
        //队伍对象列表
        public static List<TeamObj> allTeams = new List<TeamObj>();
        //侦查信息来源
        public static GameObject scoutInfoSourceObj { get; set; }

        /// <summary>
        /// 国籍中文名
        /// </summary>
        public static Dictionary<Nationality, string> NationalityDictionary = new Dictionary<Nationality, string>()
        { { Nationality.none, "无国籍" },
          { Nationality.red, "红精灵国" },
          { Nationality.blue, "蓝精灵国" },
          { Nationality.monster, "中立野怪" },
        };

        /// <summary>
        /// 武将技中文名
        /// </summary>
        public static Dictionary<HeroSkill, string> HeroSkillDictionary = new Dictionary<HeroSkill, string>()
        { { HeroSkill.none, "沉默" },
          { HeroSkill.IncreaseOwnSpeed, "急行军" },
          { HeroSkill.ReduceEnemySpeed, "减速" },
          { HeroSkill.Disorient, "迷惑" },
          { HeroSkill.Encourage, "鼓舞" },
          { HeroSkill.Silence, "沉默" },
          { HeroSkill.Disease, "瘟疫" },
        };
        /// <summary>
        /// 士兵种类中文名
        /// </summary>
        public static Dictionary<SoldierType, string> SoldierTypeDictionary = new Dictionary<SoldierType, string>()
        { { SoldierType.SwordMan, "剑士" },
          { SoldierType.Archer, "弓箭兵" },
          { SoldierType.Cavalry, "骑兵" },
          { SoldierType.Tank, "坦克兵" },
          { SoldierType.Worker, "工程兵" },
        };

        public static Dictionary<ScoutInfoType, string> ScoutInfoDictionary = new Dictionary<ScoutInfoType, string>()
        {
            {ScoutInfoType.none,"全力侦查中......" },
            {ScoutInfoType.TeamBeBuilt,"我国有新的队伍出征了！" },
            {ScoutInfoType.EnemyBeFound,"前方发现敌军！"},
            {ScoutInfoType.TeamBeAttacked,"我们有队伍遭到了攻击！"},
            {ScoutInfoType.CityBeAttacked,"我们的城堡受到了攻击！" },
            {ScoutInfoType.CannotBuild,"此处无法建造！" },
            {ScoutInfoType.WhereBuild,"请选择建造地点！" },
            {ScoutInfoType.GoBuild,"马上去指定地点建造！" }
        };

        #endregion

        
        #region Public Methods 公有方法
        /// <summary>
        /// 找到可招募武将
        /// </summary>
        /// <returns>可招募武将</returns>
        public List<string> AvailableHeros()
        {
            List<string> availableheros = new List<string>();
            availableheros.Clear();
            foreach (var item in FindObjectsOfType<HeroObj>())
            {
                if (item.nationality == Nationality.none)
                {
                    availableheros.Add(item.word);
                }
            }
            return availableheros;
        }
        #endregion

        #region Public static methods 自定义静态方法

        /// <summary>
        /// 计算武将招募价格
        /// </summary>
        /// <param name="_heroPriceCoefficient">武将价格系数</param>
        /// <param name="_force">武将武力</param>
        /// <param name="_intelligence">武将智力</param>
        /// <param name="_dominance">武将统力</param>
        /// <param name="_politics">武将政治</param>
        /// <param name="_heroDominanceCoefficient">武将统力系数</param>
        /// <returns></returns>
        public static int calculateHeroPrice(int _heroPriceCoefficient,int _force, int _intelligence, int _dominance ,int _politics, float _heroDominanceCoefficient)
        {
            int heroPrice = _heroPriceCoefficient * (_force + _intelligence + (int)(_dominance * _heroDominanceCoefficient) +_politics);
            return heroPrice;
        }

        /// <summary>
        /// 计算城市最近人口
        /// </summary>
        /// <param name="_population">原人口</param>
        /// <param name="_politics">城内政治最高武将政治</param>
        /// <param name="_taxrate">国家税率</param>
        /// <returns></returns>
        public static int calculatePopulationIncrease(int _population, int _politics, float _taxrate)
        {
            int newpopulation = 0;
            newpopulation = _population + Mathf.RoundToInt(PopulationIncreaseCoefficient * _politics /10000) +Mathf.RoundToInt(PopulationIncreaseCoefficient * (1- _taxrate)/10000);
            return newpopulation;
        }

        /// <summary>
        /// 查找单词返回匹配到的单词实例
        /// </summary>
        /// <param name="_word">需查找的单词</param>
        /// <returns>单词实例</returns>
        public static WordObj MatchWord(string _word)
        {
            foreach (var item in allWords)
            {
                if (item.word == _word)
                {
                    return item;
                }
            }
            return null;

            /*
            WordObj[] wordObjs = FindObjectsOfType<WordObj>();
            foreach (var item in wordObjs)
            {
                if (item.word == _word)
                {
                    return item;
                }
            }
            return null;
            */
        }
        /// <summary>
        /// 查找单词返回匹配到的武将实例
        /// </summary>
        /// <param name="_word">需查找的单词</param>
        /// <returns>武将实例</returns>
        public static HeroObj MatchHero(string _word)
        {
            foreach (var item in allHeros)
            {
                if (item.word == _word)
                {
                    return item;
                }
            }
            return null;
            /*
            HeroObj[] heroObjs = FindObjectsOfType<HeroObj>();
            foreach (var item in heroObjs)
            {
                if (item.word == _word)
                {
                    return item;
                }
            }
            return null;
            */
        }
        /// <summary>
        /// 查找武将单词返回匹配到的队伍实例
        /// </summary>
        /// <param name="_word">需查找的单词</param>
        /// <returns>队伍实例</returns>
        public static TeamObj MatchTeam(string _word)
        {
            foreach (var item in allTeams)
            {
                if (item.hero == _word)
                {
                    return item;
                }
            }
            
            //如果静态列表里查不到就换个方式查找（适用于队伍刚建立就调用UI，UI的代码里队伍静态列表还没来得及加上去）
            TeamObj[] teamObjs = FindObjectsOfType<TeamObj>();
            foreach (var item in teamObjs)
            {
                if (item.hero == _word)
                {
                    return item;
                }
            }
            
            return null;

        }

        /// <summary>
        /// 查找城市单词返回匹配到的城市实例
        /// </summary>
        /// <param name="_word">需查找的单词</param>
        /// <returns>城市实例</returns>
        public static CityObj MatchCity(string _word)
        {
            foreach (var item in allCities)
            {
                if (item.word == _word)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找国家国籍返回匹配到的国家实例
        /// </summary>
        /// <param name="_word">需查找的国籍</param>
        /// <returns>国家实例</returns>
        public static CountryObj MatchCountry(Nationality _nationality)
        {
            foreach (var item in allCountries)
            {
                if (item.nationality == _nationality)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 返回自己控制的国家
        /// </summary>
        /// <returns></returns>
        public static CountryObj SelfCountry()
        {
            foreach (var item in allCountries)
            {
                if (item.photonView.isMine)
                {
                    return item;
                }
            }
            return null;
        }


        #endregion

        #region Private var 私有变量
        //当前国家对象
        CountryObj countryobj;

        List<string> words = new List<string>();              //所有单词列表
        List<string> cities = new List<string>();             //所有城市列表
        List<Vector3> citiespositions = new List<Vector3>();  //所有城市位置列表
        List<string> heros = new List<string>();              //所有武将列表

        //string[] wordContents;                                //单词内容的按行存放的字符串数组
        //Sprite sprite;                                        //图片的Sprite缓存变量
        //AudioClip audioClip;                                  //读音的AudioClip缓存变量
        //WordObj wordObj;

        Transform Ground;                                     //地板
        #endregion



        #region Private Methods
        
        /// <summary>
        /// 给words列表赋值
        /// </summary>
        [PunRPC]
        private void AssignWordsList()
        {
            foreach (var item in allWords)
            {
                words.Add(item.word);
            }
        }

        /// <summary>
        /// 给cities城市列表赋值
        /// </summary>
        [PunRPC]
        void AssignCitiesList()
        {            
            if (words.Count>CityCount)
            {
                for (int i = 0; i < CityCount; i++)
                {
                    cities.Add(words[i]);
                }
            }
        }

        /// <summary>
        /// 给heros武将列表赋值
        /// </summary>
        [PunRPC]
        void AssignHerosList()
        {
            if (words.Count == CityCount + HeroCount)
            {
                for (int i = CityCount; i < words.Count; i++)
                {
                    heros.Add(words[i]);
                }
            }
        }

        /// <summary>
        /// 随机分配所有城市位置
        /// </summary>        
        void AssignCitiesPositions()
        {
            for (int i = 0; i < cities.Count; i++)
            {
                bool rightposition = false;
                Vector3 pos = Vector3.zero;
                while (!rightposition)
                {
                    pos = RandomPosition();
                    rightposition = true;
                    for (int j = 0; j < citiespositions.Count; j++)
                    {
                        if (Vector3.Distance(citiespositions[j], pos) < 4.5f)
                        {
                            rightposition = false;
                            break;
                        }
                    }
                }
                photonView.RPC("AssignCitiesPositionsRPC", PhotonTargets.All, pos);                
            }
        }
        /// <summary>
        /// 随机产生Ground上的位置
        /// </summary>
        /// <returns>位置</returns>
        Vector3 RandomPosition()
        {
            float xside = Ground.localScale.x * 10 - 4;
            float zside = Ground.localScale.z * 10 - 4;
            float x = UnityEngine.Random.Range(-xside / 2, xside / 2);
            float z = UnityEngine.Random.Range(-zside / 2, zside / 2);
            Vector3 pos = new Vector3(x, 0.25f, z);
            return pos;
        }
        [PunRPC]
        void AssignCitiesPositionsRPC(Vector3 _pos)
        {
            citiespositions.Add(_pos);
        }


        /// <summary>
        /// 生成所有城市实例
        /// </summary>
        bool SpawnCityObjects()
        {
            try
            {
                for (int i = 0; i < cities.Count; i++)
                {
                    int cityViewID = PhotonNetwork.AllocateViewID();
                    photonView.RPC("SpawnCityObjectRPC", PhotonTargets.All, cities.IndexOf(cities[i]), cityViewID);
                }
                return true;
            }
            catch (Exception)
            {
                throw;                
            }

            
        }
        [PunRPC]
        void SpawnCityObjectRPC(int _index,int _cityViewID)
        {
            if (citiespositions.Count != cities.Count)
            {
                Application.Quit();
                return;
            }
            GameObject newCity = Instantiate(PhotonGameManager.assetDictionary["City"], citiespositions[_index], Quaternion.identity);
            newCity.GetComponent<PhotonView>().viewID = _cityViewID;
        }
        


        /// <summary>
        /// 生成所有武将实例
        /// </summary>
        bool SpawnHeroObjects()
        {
            try
            {
                foreach (var item in heros)
                {
                    PhotonNetwork.Instantiate("Hero", Vector3.zero, Quaternion.identity, 0);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 生成所有野怪实例
        /// </summary>
        void SpawnMonsterObjects()
        {
            //手动分配PhotonViewID
            int viewId = PhotonNetwork.AllocateViewID();
            int viewIdModel = PhotonNetwork.AllocateViewID();

            photonView.RPC("SpawnMonsterObjectsRPC", PhotonTargets.All, WorldObj.Nationality.monster, 50000, WorldObj.SoldierType.Griffin, 80,  viewId, viewIdModel);
                        
        }
        [PunRPC]
        void SpawnMonsterObjectsRPC(WorldObj.Nationality _nationality, int _soldiernumber , WorldObj.SoldierType _soldiertype , int _teamMorola , int _viewId , int _viewIdModel)
        {
            //计算出野怪位置
            Vector3 GriffinPosition = Vector3.zero;
            switch (_soldiertype)
            {
                case SoldierType.Griffin:
                    bool rightposition = false;
                    while (!rightposition)
                    {
                        GriffinPosition = RandomPosition();
                        rightposition = true;
                        for (int j = 0; j < citiespositions.Count; j++)
                        {
                            if (Vector3.Distance(citiespositions[j], GriffinPosition) < 2f )
                            {
                                rightposition = false;
                                break;
                            }
                        }
                    }

                    break;
                default:
                    break;
            }            
            GameObject newMonster = Instantiate(team, GriffinPosition, Quaternion.identity);
            newMonster.GetComponent<PhotonView>().viewID = _viewId;
            //给新派生的队伍对象赋值
            TeamObj teamObj = newMonster.GetComponent<TeamObj>();
            teamObj.nationality = _nationality;
            teamObj.soldiernumber = _soldiernumber;
            teamObj.soldiertype = _soldiertype;
            teamObj.hero = "";
            teamObj.teamspeed = WorldObj.BeginTeamSpeed;
            teamObj.teammorale = WorldObj.BeginTeamMorale;
            //根据士兵类型激活队伍模型
            Transform teamModel = null;

            switch (teamObj.soldiertype)        
            {        
                case WorldObj.SoldierType.Griffin:        
                    teamModel = teamObj.transform.Find("Griffin_Yellow");        
                break;        
            }
            if (teamModel)
            {
                //激活队伍模型
                teamModel.gameObject.SetActive(true);
                //手动分配队伍模型的PhotonViewID                
                teamModel.GetComponent<PhotonView>().viewID = _viewIdModel;
            }

            //使其不可见
            Funcs.ChangeObjectAlphaValue(teamObj.teamHealthCanvas, 0f);
            Funcs.ChangeObjectAlphaValue(teamObj.transform, 0f);
            

        }

        /// <summary>
        /// 给所有城市对象的单词属性一项赋值
        /// </summary>
        //[PunRPC]
        private void AssignCitiesWords()
        {            
            for (int i = 0; i < allCities.Count; i++)
            {
                allCities[i].word = cities[i];
            }
        }

        /// <summary>
        /// 给所有武将对象的单词属性一项赋值
        /// </summary>
        //[PunRPC]
        private void AssignHerosWords()
        {            
            for (int i = 0; i < allHeros.Count; i++)
            {
                allHeros[i].word = heros[i];
            }
        }

        /// <summary>
        /// 给所有武将对象的属性随机赋值
        /// </summary>
        [PunRPC]
        private void AssignHeros()
        {            
            for (int i = 0; i < allHeros.Count; i++)
            {
                allHeros[i].GiveHeroSkill((HeroSkill)UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(WorldObj.HeroSkill)).Length));
                allHeros[i].GiveForce(UnityEngine.Random.Range(HeroForceMin,HeroForceMax+1));
                allHeros[i].GiveIntelligence(UnityEngine.Random.Range(HeroIntelligenceMin,HeroIntelligenceMax+1));
                allHeros[i].GiveDominance(UnityEngine.Random.Range(HeroDominanceMin, HeroDominanceMax + 1));
                allHeros[i].GivePolitics(UnityEngine.Random.Range(HeroPoliticsMin, HeroPoliticsMax + 1));
                allHeros[i].GiveAllegiance(UnityEngine.Random.Range(HeroAllegianceMin, HeroAllegianceMax + 1));
                allHeros[i].GiveCharm(UnityEngine.Random.Range(HeroCharmMin, HeroCharmMax + 1));                
                int heroPrice = calculateHeroPrice(HeroPriceCoefficient, allHeros[i].force, allHeros[i].intelligence, allHeros[i].dominance, allHeros[i].politics, HeroDominanceCoefficient);
                allHeros[i].GivePrice(heroPrice);

            }
        }

        /// <summary>
        /// 给场景中所有生成的对象实例赋值单词属性
        /// </summary>
        private bool AssignAll()
        {
            try
            {
                AssignCitiesWords();
                AssignHerosWords();
                return true;
            }
            catch (Exception)
            {
                throw;
            }

            //photonView.RPC("AssignCitiesWords", PhotonTargets.All, null);
            //photonView.RPC("AssignHerosWords", PhotonTargets.All, null);            

        }


        /// <summary>
        /// 生成国家
        /// </summary>
        void SpawnCountry()
        {
            GameObject country;
             
            switch (PhotonNetwork.player.GetTeam())
            {

                case PunTeams.Team.red:
                    country = PhotonNetwork.Instantiate("Country", new Vector3(0f,0f,-1f), Quaternion.identity, 0);
                    countryobj = country.GetComponent<CountryObj>();
                    countryobj.nationality = Nationality.red;
                    Debug.Log("该国国籍是：" + countryobj.nationality);
                    break;
                case PunTeams.Team.blue:
                    country = PhotonNetwork.Instantiate("Country", new Vector3(0f, 0f, 1f), Quaternion.identity, 0);
                    countryobj = country.GetComponent<CountryObj>();
                    countryobj.nationality = Nationality.blue;
                    Debug.Log("该国国籍是：" + countryobj.nationality);
                    break;
                default:
                    break;
            }
            
        }

        #endregion

        private void Awake()
        {
            Transform sceneGround = PhotonGameManager.assetDictionary["Ground"].transform;
            Ground = Instantiate(sceneGround);
            Ground.SetParent(transform);
            Ground.localScale = new Vector3(4f, 1f, 1.5f);

            Transform sceneTopGates = PhotonGameManager.assetDictionary["TopGates"].transform;
            Transform topGates = Instantiate(sceneTopGates);
            topGates.localPosition = new Vector3(-9.11f,0.8f,7.2f);
            topGates.localRotation = Quaternion.Euler(45f,0f,0f);
            topGates.localScale = new Vector3(0.15f,0.15f,0.15f);
        }

        IEnumerator Start()
        {
            team = PhotonGameManager.assetDictionary["TeamGriffin"];                      

            if (PhotonNetwork.isMasterClient)
            {

                //给words列表赋值
                photonView.RPC("AssignWordsList", PhotonTargets.All, null);

                // 给cities城市列表赋值
                photonView.RPC("AssignCitiesList", PhotonTargets.All, null);

                // 随机分配所有城市位置
                AssignCitiesPositions();

                //给heros武将列表赋值
                photonView.RPC("AssignHerosList", PhotonTargets.All, null);


                // 生成所有城市实例
                yield return SpawnCityObjects();

                // 生成所有武将实例
                yield return SpawnHeroObjects();

            }



            // 给场景中生成的城市和英雄对象实例赋值单词属性
            yield return AssignAll();


            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("AssignHeros", PhotonTargets.All, null);
            }

            // 生成国家
            SpawnCountry();

            if (PhotonNetwork.isMasterClient)
            {
                //生成所有野怪实例
                Invoke("SpawnMonsterObjects",1f);
            }        

        }

    }


}
