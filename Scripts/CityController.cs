using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Com.Aionline.WordsCities
{
    public class CityController : Photon.MonoBehaviour
    {
        #region Var

        //信息显示面板
        public Transform WordInfoShow { get; set; }
        Transform wordInfoShowTitle;
        Transform wordInfoShowTitleText;
        Transform wordInfoShowButton;
        Transform wordInfoShowButtonImage;
        Transform wordInfoShowButtonText;

        //城市菜单面板
        public Transform CityMenuPanel { get; set; }
        Button GoWarActiveBtn;
        Button RecruitActiveBtn;
        Button ConscriptActiveBtn;
        Button CityDefenceActiveBtn;
        //城市菜单城市单词面板
        Transform cityWordTitle;
        //城市菜单面板城市单词文本
        Transform cityWordText;
        //城市菜单面板城市单词图片
        Transform cityWordImage;

        //单词测试面板
        public Transform wordTest { get; set; }
        Transform wordTestOptions;
        Transform wordOptionOne;
        Transform wordOptionTwo;
        Transform wordOptionThree;
        Transform wordTestText;
        Transform wordTestImage;
        Transform wordOptionOneText;
        Transform wordOptionTwoText;
        Transform wordOptionThreeText;
        Button wordTestCancelButton;

        buttonAction currentButtonAction;

        //出征面板
        public Transform GoWarPanel { get; set; }
        //出征面板单词文本
        Transform goWarWordText;
        //出征面板武将文本
        Transform goWarHeroText;
        //出征面板单词图片
        Transform goWarWordImage;
        //士兵兵种选择
        Transform checkboxSwordMan;
        Transform checkboxArcher;
        Transform checkboxCavalry;
        Transform checkboxWorker;
        //出征面板剑士士兵数量
        Transform swordManNumber;
        //出征面板弓箭士兵数量
        Transform archerNumber;
        //出征面板骑兵数量
        Transform cavalryNumber;
        //出征面板工程兵数量;
        Transform workerNumber;
        //出征面板武将列表盒子
        RectTransform goWarHerosListBox;
        //带兵数拖动条
        Transform sliderTakeSoldiers;
        //带兵数显示控件
        Transform takeSoldierNumber;
        //确认出征按钮
        Transform goWarButton;
        //取消出征按钮
        Transform goWarCancelButton;

        //招募面板
        public Transform RecruitPanel { get; set; }
        //招募面板单词文本
        Transform recruitWordText;
        //招募面板武将文本
        Transform recruitHeroText;
        //招募面板单词图片
        Transform recruitWordImage;
        //招募面板武将列表盒子
        RectTransform recruitHerosListBox;
        //确认招募按钮
        Transform recruitButton;
        //取消招募按钮
        Transform recruitCancelButton;

        //征兵面板
        public Transform ConscriptPanel { get; set; }
        //征兵面板单词文本
        Transform conscriptWordText;
        //征兵面板单词图片
        Transform conscriptWordImage;
        ////士兵兵种选择checkboxgroup
        Transform conscriptCheckboxGroup;
        //士兵兵种选择
        Transform conscriptCheckboxSwordMan;
        Transform conscriptCheckboxArcher;
        Transform conscriptCheckboxCavalry;
        Transform conscriptCheckboxWorker;
        //征兵面板剑士士兵数量
        Transform conscriptSwordManNumber;
        //征兵面板弓箭士兵数量
        Transform conscriptArcherNumber;
        //征兵面板骑兵数量
        Transform conscriptCavalryNumber;
        //征兵面板工兵数量
        Transform conscriptWorkerNumber;
        //征兵拖动条
        Transform sliderConscript;
        //本城剩余可征兵人口显示控件
        Transform availablePopulation;
        //确认征兵按钮
        Transform conscriptButton;
        //取消征兵按钮
        Transform conscriptCancelButton;

        //城防面板
        public Transform CityDefencePanel { get; set; }
        //城防面板单词文本
        Transform citydefenceWordText;
        //城防面板的单词图片
        Transform citydefenceWordImage;
        //要修筑的城防值
        Transform citydefenceValue;
        //要消耗的金钱
        Transform citydefenceMoneyValue;
        //要修筑城防的拖动条
        Transform sliderCityDefence;
        //确认修筑城防按钮
        Transform citydefenceButton;
        //取消按钮
        Transform citydefenceCancelButton;

        //建造武器面板
        public Transform BuildWeaponPanel { get; set; }
        public Transform uiCamera { get; set; }
        


        //当前的国家对象
        CountryObj countryObj;
        //当前的单词对象
        WordObj currentWord;
        //出征带兵数数值
        int takeSoldierNumberInt;
        //士兵种类数值
        WorldObj.SoldierType soldierType;
        //出征选择的士兵种类的剩余数值
        int soldierTypeNumber;
        //各兵种的征兵数量
        int conscriptSwordManInt;
        int conscriptArcherInt;
        int conscriptCavalryInt;
        int conscriptWorkerInt;
        //要修筑的城防数值
        int citydefenceInt;
        //要消耗的金钱数值
        int citydefenceMoneyInt;

        //UIController组件
        UIController uiController;
        //世界UI组件
        Transform WorldUI;
        #endregion


        #region Public Var

        //队伍的预制体
        //public GameObject team;
        //public GameObject teamWeapon;
        
        public GameObject teamSwordMan { get; set; }
        public GameObject teamArcher { get; set; }
        public GameObject teamCavalry { get; set; }
        public GameObject teamWorker { get; set; }
        public GameObject teamTank { get; set; }
        public GameObject teamGriffin { get; set; }
        
        //public GameObject teamAsset { get; set; }

        //选择的城市
        public CityObj cityObj { get; set; }

        //新建立的队伍
        public GameObject newTeam { get; set; }

        //进入单词测试前选择的按钮或条件
        public enum buttonAction
        {
            none,
            GoWar,
            Recruit,
            Conscript,
            CityDefence,
            GoWarConfirm,
            RecruitConfirm,
            ConscriptConfirm,
            CityDefenceConfirm
        }

        #endregion

        void Start()
        {
            //获取队伍对象预制体
            teamSwordMan = PhotonGameManager.assetDictionary["TeamSwordMan"];
            teamArcher = PhotonGameManager.assetDictionary["TeamArcher"];
            teamCavalry = PhotonGameManager.assetDictionary["TeamCavalry"];
            teamWorker = PhotonGameManager.assetDictionary["TeamWorker"];
            teamTank = PhotonGameManager.assetDictionary["TeamTank"];
            teamGriffin = PhotonGameManager.assetDictionary["TeamGriffin"];
            
            //获取世界UI
            WorldUI = GameObject.Find("PlayerCamera").transform.Find("WorldUI(Clone)").transform;
            //找到当前的国家对象组件
            countryObj = GetComponent<CountryObj>();
            //找到UIController组件
            uiController = GetComponent<UIController>();

            if (!photonView.isMine) { return; }

            //初始化信息面板控件变量
            WordInfoShow = GameObject.Find("Canvas(Clone)").transform.Find("WordInfoShow");
            wordInfoShowTitle = WordInfoShow.Find("Title");
            wordInfoShowTitleText = wordInfoShowTitle.Find("Text");
            wordInfoShowButton = WordInfoShow.Find("WordInfoButton");
            wordInfoShowButtonImage = wordInfoShowButton.Find("Image");
            wordInfoShowButtonText = wordInfoShowButton.Find("Text");

            //初始化城市菜单面板控件变量
            CityMenuPanel = GameObject.Find("Canvas(Clone)").transform.Find("CityMenuPanel");
            cityWordTitle = CityMenuPanel.Find("CityWordTitle");
            cityWordText = cityWordTitle.Find("Text");
            cityWordImage = cityWordTitle.Find("Image");
            //初始化单词测试面板
            wordTest = GameObject.Find("Canvas(Clone)").transform.Find("WordTest"); ;
            wordTestOptions = wordTest.Find("Options");
            wordOptionOne = wordTestOptions.Find("OptionOne");
            wordOptionTwo = wordTestOptions.Find("OptionTwo");
            wordOptionThree = wordTestOptions.Find("OptionThree");
            wordTestText = wordTest.Find("Text");
            wordTestImage = wordTest.Find("Image");
            wordOptionOneText = wordOptionOne.Find("Text");
            wordOptionTwoText = wordOptionTwo.Find("Text");
            wordOptionThreeText = wordOptionThree.Find("Text");
            wordTestCancelButton = wordTest.Find("ButtonRed").GetComponent<Button>();

            //初始化出征面板控件变量
            GoWarPanel = GameObject.Find("Canvas(Clone)").transform.Find("GoWarPanel");
            goWarWordText = GoWarPanel.transform.Find("Title").Find("Text");
            goWarHeroText = GoWarPanel.transform.Find("Title").Find("HeroText");
            goWarWordImage = GoWarPanel.transform.Find("Title");
            //checkbox group
            checkboxSwordMan = GoWarPanel.Find("Checkbox Group").Find("Checkbox1");
            checkboxArcher = GoWarPanel.Find("Checkbox Group").Find("Checkbox2");
            checkboxCavalry = GoWarPanel.Find("Checkbox Group").Find("Checkbox3");
            checkboxWorker = GoWarPanel.Find("Checkbox Group").Find("Checkbox4");
            swordManNumber = checkboxSwordMan.Find("SwordManNumber");
            archerNumber = checkboxArcher.Find("ArcherNumber");
            cavalryNumber = checkboxCavalry.Find("CavalryNumber");
            workerNumber = checkboxWorker.Find("WorkerNumber");

            goWarHerosListBox = GoWarPanel.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
            sliderTakeSoldiers = GoWarPanel.Find("Slider");
            takeSoldierNumber = sliderTakeSoldiers.Find("TakeSoldierNumber");

            goWarButton = GoWarPanel.Find("ButtonGreen");
            goWarCancelButton = GoWarPanel.Find("ButtonRed");

            // 初始化招募面板控件变量
            RecruitPanel = GameObject.Find("Canvas(Clone)").transform.Find("RecruitPanel");
            recruitWordText = RecruitPanel.Find("Title").Find("Text");
            recruitHeroText = RecruitPanel.Find("Title").Find("HeroText");
            recruitWordImage = RecruitPanel.Find("Title");
            recruitHerosListBox = RecruitPanel.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;

            recruitButton = RecruitPanel.Find("ButtonGreen");
            recruitCancelButton = RecruitPanel.Find("ButtonRed");

            //初始化征兵面板控件变量
            ConscriptPanel = GameObject.Find("Canvas(Clone)").transform.Find("ConscriptPanel");
            conscriptWordText = ConscriptPanel.Find("Title").Find("Text");
            conscriptWordImage = ConscriptPanel.Find("Title");
            //checkbox group
            conscriptCheckboxGroup = ConscriptPanel.Find("Checkbox Group");
            conscriptCheckboxSwordMan = conscriptCheckboxGroup.Find("Checkbox1");
            conscriptCheckboxArcher = conscriptCheckboxGroup.Find("Checkbox2");
            conscriptCheckboxCavalry = conscriptCheckboxGroup.Find("Checkbox3");
            conscriptCheckboxWorker = conscriptCheckboxGroup.Find("Checkbox4");
            conscriptSwordManNumber = conscriptCheckboxSwordMan.Find("SwordManNumber");
            conscriptArcherNumber = conscriptCheckboxArcher.Find("ArcherNumber");
            conscriptCavalryNumber = conscriptCheckboxCavalry.Find("CavalryNumber");
            conscriptWorkerNumber = conscriptCheckboxWorker.Find("WorkerNumber");

            sliderConscript = ConscriptPanel.Find("Slider");
            availablePopulation = sliderConscript.Find("AvailablePopulation");

            conscriptButton = ConscriptPanel.Find("ButtonGreen");
            conscriptCancelButton = ConscriptPanel.Find("ButtonRed");

            //初始化城防面板变量
            CityDefencePanel = GameObject.Find("Canvas(Clone)").transform.Find("CityDefencePanel");
            citydefenceWordText = CityDefencePanel.Find("Title").Find("Text");
            citydefenceWordImage = CityDefencePanel.Find("Title");
            sliderCityDefence = CityDefencePanel.Find("Slider");
            citydefenceValue = sliderCityDefence.Find("CityDefenceValue");
            citydefenceMoneyValue = sliderCityDefence.Find("CityDefenceMoneyValue");

            citydefenceButton = CityDefencePanel.Find("ButtonGreen");
            citydefenceCancelButton = CityDefencePanel.Find("ButtonRed");

            //初始化建造武器面板变量
            BuildWeaponPanel = GameObject.Find("CameraUI(Clone)").transform.Find("BuildWeaponPanel");
            uiCamera = GameObject.Find("CameraUI(Clone)").transform.Find("UICamera");



            //添加单词信息显示面板监听事件
            AddWordInfoShowPanelListeners();
            //添加单词测试面板监听事件
            AddWordTestPanelListeners();
            //添加城市菜单面板的事件
            AddCityMenuPanelListeners();
            // 添加出征面板的事件
            AddGoWarPanelListeners();
            //添加招募面板的事件
            AddRecruitPanelListeners();
            //添加征兵面板的事件
            AddConscriptPanelListeners();
            //添加城防面板的事件
            AddCityDefencePanelListeners();
        }

        #region Public Methods
        /// <summary>
        /// 城市菜单显示
        /// </summary>
        public void CityMenu()
        {

            // 打开城市菜单面板
            OpenCityMenuPanel();
            // 初始化清空城市菜单面板
            EmptyCityMenuPanel();
            // 加载城市数据到城市菜单面板
            LoadCityDataToCityMenuPanel();
        }



        #endregion

        #region Methods ——信息显示面板
        /// <summary>
        /// 单词信息显示面板的监听事件添加
        /// </summary>
        void AddWordInfoShowPanelListeners()
        {
            Button wordInfoShowBtn = wordInfoShowButton.GetComponent<Button>();
            CancelBtn(wordInfoShowBtn);            
        }
        /// <summary>
        /// 给单词信息显示面板的控件赋值
        /// </summary>
        /// <param name="_wordInfoTitle">信息内容</param>
        /// <param name="_word">要显示的单词</param>
        public void LoadWordInfoShow(WordObj _word, buttonAction _buttonAction, bool _success)
        {
            //给单词信息显示面板的显示控件赋值                                    
            wordInfoShowButtonImage.GetComponent<Image>().overrideSprite = _word.sprite;
            if (_success)
            {
                switch (_buttonAction)
                {
                    /*
                    case buttonAction.GoWarConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "英雄 " + _word.word + " 已出征！";
                        TeamObj selectedTeam = WorldObj.MatchTeam(_word.word);
                        wordInfoShowButtonText.GetComponent<Text>().text =
                                  "带队英雄：" + selectedTeam.hero + "\r\n"
                                + "\r\n"
                                + "军心：" + selectedTeam.teammorale + "\r\n"
                                + "行军速度：" + selectedTeam.teamspeed + "\r\n"
                                + "兵种：" + selectedTeam.soldiertype.ToString() + "\r\n"
                                + "士兵数量：" + selectedTeam.soldiernumber + "\r\n";
                        break;
                    */
                    case buttonAction.RecruitConfirm:                        
                        HeroObj selectedHero = WorldObj.MatchHero(_word.word);
                        wordInfoShowTitleText.GetComponent<Text>().text = "英雄: " + _word.word + " 国籍: " + WorldObj.NationalityDictionary[selectedHero.nationality]; // + " 已招募到您的麾下！";
                        string skillname = WorldObj.HeroSkillDictionary[selectedHero.heroskill];
                        wordInfoShowButtonText.GetComponent<Text>().text =
                                                      "英雄名：" + selectedHero.word + "\r\n"
                                                    + "\r\n"
                                                    + "武将技：" + skillname + "\r\n"
                                                    + "武力：" + selectedHero.force + "\r\n"
                                                    + "智力：" + selectedHero.intelligence + "\r\n"
                                                    + "统力：" + selectedHero.dominance + "\r\n"
                                                    + "政治：" + selectedHero.politics + "\r\n"
                                                    + "忠诚：" + selectedHero.allegiance + "\r\n"
                                                    + "魅力：" + selectedHero.charm + "\r\n"
                                                    + "招募金：" + selectedHero.price;
                        break;
                    case buttonAction.ConscriptConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "恭喜！征兵成功！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "一批新的士兵来报效国家！";
                        break;
                    case buttonAction.CityDefenceConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "恭喜！修筑城防成功！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "国家的城防固若金汤！";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (_buttonAction)
                {
                    /*
                    case buttonAction.GoWarConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "英雄 " + _word.word + " 无法建队！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "国家财政紧缺！"+ "\r\n" + "英雄 " + _word.word + " 无法建队！";
                        break;*/
                    case buttonAction.RecruitConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "英雄 " + _word.word + " 招募失败！";
                        HeroObj heroObj = WorldObj.MatchHero(_word.word);
                        if (heroObj.nationality == WorldObj.Nationality.none)
                        {
                            wordInfoShowButtonText.GetComponent<Text>().text = "国家财政紧缺！" + "\r\n" + "英雄 " + _word.word + " 招募失败！";
                        }
                        else
                        {
                            wordInfoShowButtonText.GetComponent<Text>().text = "英雄"+ _word.word + "已经投靠别国了！";
                        }
                        break;
                    case buttonAction.ConscriptConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "征兵失败！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "国家财政紧缺！";
                        break;
                    case buttonAction.CityDefenceConfirm:
                        wordInfoShowTitleText.GetComponent<Text>().text = "修筑城防失败！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "国家财政紧缺！";
                        break;
                    case buttonAction.none:
                        wordInfoShowTitleText.GetComponent<Text>().text = "招募失败！";
                        wordInfoShowButtonText.GetComponent<Text>().text = "世间已无英雄可用！";
                        break;
                    default:
                        break;
                }
            }

            uiController.CloseAllPanels();                 
            PlayWordSound(_word);
            //if (_success && _buttonAction == buttonAction.GoWarConfirm) { return; } //增加此代码是在考虑出征成功不弹出窗口
            //设置UI运行为真       
            WorldObj.UIRun = true;
            WordInfoShow.gameObject.SetActive(true);            
        }

        #endregion


        #region Private Methods ——单词测试面板
        /// <summary>
        /// 单词测试面板的监听事件添加
        /// </summary>
        void AddWordTestPanelListeners()
        {
            //城市单词测试按钮执行显示各类面板（增加单词测试按钮的点击监听事件）
            for (int i = 0; i < wordTestOptions.childCount; i++)
            {
                Transform cityWordTestOption = wordTestOptions.GetChild(i);
                cityWordTestOption.GetComponent<Button>().onClick.AddListener(delegate () { ButtonAction(cityWordTestOption, currentWord); });
            }
            //给城市单词测试面板的取消按钮添加监听
            CancelBtn(wordTestCancelButton);            
        }

        /// <summary>
        /// 显示单词测试
        /// </summary>
        void ShowWordTest(Transform _wordTitle, WordObj _word)
        {
            //设置UI运行为真
            WorldObj.UIRun = true;

            _wordTitle.gameObject.SetActive(false);
            wordTest.gameObject.SetActive(true);
            PlayWordSound(_word);
        }
        /// <summary>
        /// 单词测试按钮点击调用
        /// </summary>
        /// <param name="_selectedOption">选择点击的城市单词测试按钮</param>
        void ButtonAction(Transform _selected, WordObj _word)
        {
            if (cityObj.fighting)
            {
                Debug.Log("城市处于战争状态，无法执行。");
                return;
            }
            //如果选择的中文解释正确
            if (_selected.Find("Text").GetComponent<Text>().text == _word.chinese)
            {
                switch (currentButtonAction)
                {
                    case buttonAction.GoWar:
                        GoWar();
                        break;
                    case buttonAction.Recruit:
                        Recruit();
                        break;
                    case buttonAction.Conscript:
                        Conscript();
                        break;
                    case buttonAction.CityDefence:
                        CityDefence();
                        break;                    
                    case buttonAction.GoWarConfirm:
                        GoWarConfirm();
                        //LoadWordInfoShow(_word, buttonAction.GoWarConfirm, GoWarConfirm());                        
                        break;                    
                    case buttonAction.RecruitConfirm:
                        LoadWordInfoShow(_word, buttonAction.RecruitConfirm, RecruitConfirm());
                        break;
                    case buttonAction.ConscriptConfirm:
                        LoadWordInfoShow(_word, buttonAction.ConscriptConfirm, ConscriptConfirm());                    
                        break;
                    case buttonAction.CityDefenceConfirm:
                        LoadWordInfoShow(_word, buttonAction.CityDefenceConfirm, CityDefenceConfirm());
                        break;
                    default:
                        break;
                }
            }
            else //如果选择的中文解释错误
            {
                PlayWordSound(currentWord);
                switch (currentButtonAction)
                {
                    case buttonAction.GoWar:
                    case buttonAction.Recruit:
                    case buttonAction.Conscript:
                    case buttonAction.CityDefence:
                        cityWordTitle.gameObject.SetActive(true);
                        wordTest.gameObject.SetActive(false);
                        break;
                    case buttonAction.GoWarConfirm:
                        GoWarPanel.gameObject.SetActive(true);
                        wordTest.gameObject.SetActive(false);
                        break;
                    case buttonAction.RecruitConfirm:
                        RecruitPanel.gameObject.SetActive(true);
                        wordTest.gameObject.SetActive(false);
                        break;
                    case buttonAction.ConscriptConfirm:
                        ConscriptPanel.gameObject.SetActive(true);
                        wordTest.gameObject.SetActive(false);
                        break;
                    case buttonAction.CityDefenceConfirm:
                        CityDefencePanel.gameObject.SetActive(true);
                        wordTest.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 给单词测试面板的控件赋值
        /// </summary>
        void LoadWordTest(WordObj _word)
        {
            //给单词面板测试部分的显示控件赋值
            wordTestText.GetComponent<Text>().text = _word.word;
            wordTestImage.GetComponent<Image>().overrideSprite = _word.sprite;

            string rightchinese = _word.chinese;
            string wrongchineseone = "";
            string wrongchinesetwo = "";
            do
            {
                wrongchineseone = RandomWord().chinese;
                wrongchinesetwo = RandomWord().chinese;
            }
            while (rightchinese == wrongchineseone || rightchinese == wrongchinesetwo || wrongchineseone == wrongchinesetwo);

            //随机给某选项赋值正确的单词中文解释    
            int option = UnityEngine.Random.Range(0, 2);                
            wordTestOptions.GetChild(option).Find("Text").GetComponent<Text>().text = rightchinese;
            //给其他选项赋错误的单词中文解释
            int wrongone;
            int wrongtwo;
            switch (option)
            {
                case 0:
                    wrongone = 1;
                    wrongtwo = 2;
                    break;
                case 1:
                    wrongone = 0;
                    wrongtwo = 2;
                    break;
                case 2:
                    wrongone = 0;
                    wrongtwo = 1;
                    break;
                default:
                    wrongone = 1;
                    wrongtwo = 2;
                    break;
            }
            wordTestOptions.GetChild(wrongone).Find("Text").GetComponent<Text>().text = wrongchineseone;
            wordTestOptions.GetChild(wrongtwo).Find("Text").GetComponent<Text>().text = wrongchinesetwo;
        }
        /// <summary>
        /// 取随机单词
        /// </summary>
        /// <returns>随机单词对象</returns>
        WordObj RandomWord()
        {            
            WordObj randomWord = WorldObj.allWords[UnityEngine.Random.Range(0, WorldObj.allWords.Count - 1)];
            return randomWord;
        }
        #endregion


        #region Private Methods ——城市菜单面板 CityMenu()
        /// <summary>
        /// 添加城市菜单面板的事件
        /// </summary>
        void AddCityMenuPanelListeners()
        {
            //出征按钮执行显示城市单词测试（增加出征按钮的点击监听事件）
            GoWarActiveBtn = GameObject.Find("Canvas(Clone)").transform.Find("CityMenuPanel").transform.Find("GoWarActiveButton").GetComponent<Button>();
            GoWarActiveBtn.onClick.AddListener(delegate ()
            {
                ShowWordTest(cityWordTitle, currentWord);
                currentButtonAction = buttonAction.GoWar;

            });

            //招募按钮执行显示城市单词测试（增加招募按钮的点击监听事件）
            RecruitActiveBtn = GameObject.Find("Canvas(Clone)").transform.Find("CityMenuPanel").transform.Find("RecruitActiveButton").GetComponent<Button>();
            RecruitActiveBtn.onClick.AddListener(delegate ()
            {
                List<string> availableHeros = FindObjectOfType<WorldObj>().AvailableHeros();
                if (availableHeros.Count == 0)
                {
                    LoadWordInfoShow(currentWord, buttonAction.none, false);
                }
                else
                {
                    ShowWordTest(cityWordTitle, currentWord);
                    currentButtonAction = buttonAction.Recruit;
                }
            });

            //征兵按钮执行显示城市单词测试（增加出征按钮的点击监听事件）
            ConscriptActiveBtn= GameObject.Find("Canvas(Clone)").transform.Find("CityMenuPanel").transform.Find("ConscriptActiveButton").GetComponent<Button>();
            ConscriptActiveBtn.onClick.AddListener(delegate ()
            {
                ShowWordTest(cityWordTitle, currentWord);
                currentButtonAction = buttonAction.Conscript;

            });

            //城防按钮执行显示城市单词测试（增加城防按钮的点击监听事件）
            CityDefenceActiveBtn = GameObject.Find("Canvas(Clone)").transform.Find("CityMenuPanel").transform.Find("CityDefenceActiveButton").GetComponent<Button>();
            CityDefenceActiveBtn.onClick.AddListener(delegate ()
            {
                ShowWordTest(cityWordTitle, currentWord);
                currentButtonAction = buttonAction.CityDefence;
            });
        }

        /// <summary>
        /// 打开城市菜单面板
        /// </summary>
        void OpenCityMenuPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开城市菜单面板
            CityMenuPanel.gameObject.SetActive(true);
        }
        /// <summary>
        /// 初始化清空城市菜单面板
        /// </summary>
        void EmptyCityMenuPanel()
        {
            cityWordTitle.gameObject.SetActive(true);
            cityWordText.GetComponent<Text>().text = "";
            cityWordImage.GetComponent<Image>().overrideSprite = null;
            wordTestImage.GetComponent<Image>().overrideSprite = null;
            wordOptionOneText.GetComponent<Text>().text = "";
            wordOptionTwoText.GetComponent<Text>().text = "";
            wordOptionThreeText.GetComponent<Text>().text = "";
            wordTest.gameObject.SetActive(false);
        }

        /// <summary>
        /// 加载城市数据到城市菜单面板
        /// </summary>
        void LoadCityDataToCityMenuPanel()
        {
            // 加载城市单词的文本内容和图像等到城市菜单面板
            LoadCityWordContent();
        }
        /// <summary>
        /// 加载城市单词的文本内容和图像等到面板
        /// </summary>
        void LoadCityWordContent()
        {
            currentWord = WorldObj.MatchWord(cityObj.word);

            //给城市单词面板显示部分的显示控件赋值
            cityWordText.GetComponent<Text>().text = currentWord.word + "\r\n" + currentWord.soundmark + "\r\n" + currentWord.chinese + "\r\n" + currentWord.sentence;
            cityWordImage.GetComponent<Image>().overrideSprite = currentWord.sprite;
            PlayWordSound(currentWord);

            //给城市单词测试面板的控件赋值
            LoadWordTest(currentWord);
        }

        #endregion


        #region Private Methods ——出征面板 GoWar()
        /// <summary>
        /// 添加出征面板的事件
        /// </summary>
        void AddGoWarPanelListeners()
        {
            //添加checkbox兵种选择点击事件
            checkboxSwordMan.GetComponent<Toggle>().onValueChanged.AddListener(SelectSwordMan);
            checkboxArcher.GetComponent<Toggle>().onValueChanged.AddListener(SelectArcher);
            checkboxCavalry.GetComponent<Toggle>().onValueChanged.AddListener(SelectCavalry);
            checkboxWorker.GetComponent<Toggle>().onValueChanged.AddListener(SelectWorker);

            //添加带兵数拖动变化事件
            sliderTakeSoldiers.GetComponent<Slider>().onValueChanged.AddListener(SoldierChanged);

            //添加确认出征按钮的点击事件
            goWarButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                LoadWordTest(currentWord);
                ShowWordTest(GoWarPanel, currentWord);
                currentButtonAction = buttonAction.GoWarConfirm;
                
            });

            //添加取消按钮事件
            CancelBtn(goWarCancelButton.GetComponent<Button>());

        }

        /// <summary>
        /// 关闭所有面板后打开出征面板
        /// </summary>
        void OpenGoWarPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开出征面板
            GoWarPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// 初始化清空出征面板
        /// </summary>
        void EmptyGoWarPanel()
        {
            goWarWordText.GetComponent<Text>().text = "";
            goWarHeroText.GetComponent<Text>().text = "";
            goWarWordImage.GetComponent<Image>().overrideSprite = null;
            swordManNumber.GetComponent<Text>().text = "0";
            archerNumber.GetComponent<Text>().text = "0";
            cavalryNumber.GetComponent<Text>().text = "0";
            workerNumber.GetComponent<Text>().text = "0";
            takeSoldierNumber.GetComponent<Text>().text = "0";
            //清空武将显示盒子
            EmptyHeroListBox(goWarHerosListBox);
            //清空带兵拖动条
            sliderTakeSoldiers.GetComponent<Slider>().value = 0f;
            sliderTakeSoldiers.GetComponent<Slider>().maxValue = 0f;

            //带兵数数值清零
            takeSoldierNumberInt = 0;
            //士兵种类清空
            soldierType = WorldObj.SoldierType.none;
        }
        /// <summary>
        /// 清空武将滚动盒
        /// </summary>
        /// <param name="_herolistbox">武将滚动盒</param>
        void EmptyHeroListBox(RectTransform _herolistbox)
        {
            for (int i = 0; i < _herolistbox.childCount; i++)
            {
                _herolistbox.GetChild(i).Find("Label").GetComponent<Text>().text = "";
                _herolistbox.GetChild(i).Find("Sprite").GetComponent<Image>().sprite = null;
                _herolistbox.GetChild(i).gameObject.SetActive(false);
                _herolistbox.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 加载城市数据到出征面板
        /// </summary>
        void LoadCityDataToGoWarPanel()
        {
            //加载各种类士兵数量到出征面板
            int swordManValue = cityObj.swordmannumber;
            swordManNumber.GetComponent<Text>().text = swordManValue.ToString();
            int archerValue = cityObj.archernumber;
            archerNumber.GetComponent<Text>().text = archerValue.ToString();
            int cavalryValue = cityObj.cavalrynumber;
            cavalryNumber.GetComponent<Text>().text = cavalryValue.ToString();
            int workerValue = cityObj.workernumber;
            workerNumber.GetComponent<Text>().text = workerValue.ToString();

            //初始化士兵种类选择为剑士
            checkboxSwordMan.GetComponent<Toggle>().isOn = true;
            //士兵种类值默认剑士
            soldierType = WorldObj.SoldierType.SwordMan;
            soldierTypeNumber = cityObj.swordmannumber;

            //加载武将单词数据到出征面板
            LoadHerosToListBox(goWarHerosListBox, cityObj.heros, goWarWordText, goWarWordImage, goWarHeroText);

        }
        /// <summary>
        /// 兵种选择事件的回调函数
        /// </summary>
        /// <param name="arg0"></param>
        private void SelectSwordMan(bool arg0)
        {            
            soldierType = WorldObj.SoldierType.SwordMan;
            soldierTypeNumber = cityObj.swordmannumber;
            sliderTakeSoldiers.GetComponent<Slider>().value = 0;
            HeroObj currentHero = WorldObj.MatchHero(currentWord.word);
            if (currentHero == null)
            {
                Debug.Log("currentHero为空：swordman！");
                return;
            }
            sliderTakeSoldiers.GetComponent<Slider>().maxValue = Mathf.Min(currentHero.dominance, int.Parse(swordManNumber.GetComponent<Text>().text));
            
        }
        private void SelectArcher(bool arg0)
        {
            soldierType = WorldObj.SoldierType.Archer;
            soldierTypeNumber = cityObj.archernumber;
            sliderTakeSoldiers.GetComponent<Slider>().value = 0;
            HeroObj currentHero = WorldObj.MatchHero(currentWord.word);
            if (currentHero == null)
            {
                Debug.Log("currentHero为空：archer！");
                return;
            }
            sliderTakeSoldiers.GetComponent<Slider>().maxValue = Mathf.Min(currentHero.dominance, int.Parse(archerNumber.GetComponent<Text>().text));
            
        }
        private void SelectCavalry(bool arg0)
        {
            soldierType = WorldObj.SoldierType.Cavalry;
            soldierTypeNumber = cityObj.cavalrynumber;
            sliderTakeSoldiers.GetComponent<Slider>().value = 0;
            HeroObj currentHero = WorldObj.MatchHero(currentWord.word);
            if (currentHero == null)
            {
                Debug.Log("currentHero为空：cavalry！");
                return;
            }
            sliderTakeSoldiers.GetComponent<Slider>().maxValue = Mathf.Min(currentHero.dominance, int.Parse(cavalryNumber.GetComponent<Text>().text));
            
        }
        private void SelectWorker(bool arg0)
        {
            soldierType = WorldObj.SoldierType.Worker;
            soldierTypeNumber = cityObj.workernumber;
            sliderTakeSoldiers.GetComponent<Slider>().value = 0;
            HeroObj currentHero = WorldObj.MatchHero(currentWord.word);
            if (currentHero == null)
            {
                Debug.Log("currentHero为空：worker！");
                return;
            }
            sliderTakeSoldiers.GetComponent<Slider>().maxValue = Mathf.Min(currentHero.dominance, int.Parse(workerNumber.GetComponent<Text>().text));

        }
        /// <summary>
        /// 带兵数拖动变化事件的回调函数
        /// </summary>
        /// <param name="_soldierNumber">当前选择的士兵数量</param>
        private void SoldierChanged(float _soldierNumber)
        {
            takeSoldierNumberInt = Mathf.RoundToInt(_soldierNumber);
            takeSoldierNumber.GetComponent<Text>().text = takeSoldierNumberInt.ToString();
        }
        /// <summary>
        ///  加载武将的单词数据到滚动盒，添加武将选择按钮点击事件
        /// </summary>
        /// <param name="_listbox">滚动盒</param>
        ///  <param name="_heros">某对象的英雄列表（城市或国家）</param>
        /// <param name="_wordtext">选择的武将显示单词内容用的text</param>
        /// <param name="_wordimage">选择的武将显示单词图片用的image</param>
        void LoadHerosToListBox(RectTransform _listbox, List<string> _heros, Transform _wordtext, Transform _wordimage, Transform _herotext)
        {
            for (int i = 0; i < _heros.Count; i++)
            {
                //匹配获取武将单词对象
                currentWord = WorldObj.MatchWord(_heros[i]);                

                _listbox.GetChild(i).gameObject.SetActive(true);
                //加载武将数据
                _listbox.GetChild(i).Find("Label").GetComponent<Text>().text = currentWord.word;
                _listbox.GetChild(i).Find("Sprite").GetComponent<Image>().sprite = currentWord.sprite;

                //添加武将选择按钮点击事件
                Button selectedHeroButton = _listbox.GetChild(i).GetComponent<Button>();
                selectedHeroButton.onClick.AddListener(delegate ()
                {                    
                    WordObj selectedWord = WorldObj.MatchWord(selectedHeroButton.transform.Find("Label").GetComponent<Text>().text);
                    //将选择武将的单词和属性（包括读音）全部显示和播放
                    WordShow(selectedWord, _wordtext, _wordimage, _herotext);
                    //将当前英雄单词设置为选择的英雄单词
                    currentWord = selectedWord;

                });
            }
            //将第一个英雄作为默认选择英雄显示
            if (_heros.Count > 0)
            {
                //将当前英雄单词设置为该城市第一个英雄单词
                currentWord = WorldObj.MatchWord(_heros[0]);                
                //将默认武将的单词和属性（包括读音）全部显示和播放
                WordShow(currentWord, _wordtext, _wordimage, _herotext);
            }
        } 
        /// <summary>
        /// 将武将的单词和属性（包括读音）全部显示和播放
        /// </summary>
        /// <param name="_word">武将单词对象</param>
        /// <param name="_wordtext">显示武将单词内容用的Text</param>
        /// <param name="_wordimage">显示武将单词图片用的Image</param>
        /// <param name="_herotext">显示全部武将属性用的Text</param>
        void WordShow(WordObj _word, Transform _wordtext, Transform _wordimage, Transform _herotext)
        {
            
            HeroObj selectedHero = WorldObj.MatchHero(_word.word);

            //如果是出征面板
            if (GoWarPanel.gameObject.GetActive())
            {
                //初始化士兵数量拖动条最大值为：选择的士兵数量和默认英雄的统力的较小值。
                sliderTakeSoldiers.GetComponent<Slider>().maxValue = Mathf.Min(selectedHero.dominance, soldierTypeNumber);//int.Parse(swordManNumber.GetComponent<Text>().text));
            }

            //播放武将单词读音
            PlayWordSound(_word);
            //显示武将单词内容
            _wordtext.GetComponent<Text>().text = _word.word + "\r\n" + _word.soundmark + "\r\n" + _word.chinese + "\r\n" + _word.sentence;
            _wordimage.GetComponent<Image>().overrideSprite = _word.sprite;
            
            
            string skillname = WorldObj.HeroSkillDictionary[selectedHero.heroskill];
            _herotext.GetComponent<Text>().text = "武将名：" + selectedHero.word + "\r\n"
                                                + "武将技：" + skillname + "\r\n"
                                                + "武力：" + selectedHero.force + "\r\n"
                                                + "智力：" + selectedHero.intelligence + "\r\n"
                                                + "统力：" + selectedHero.dominance + "\r\n"
                                                + "政治：" + selectedHero.politics + "\r\n"
                                                + "忠诚：" + selectedHero.allegiance + "\r\n"
                                                + "魅力：" + selectedHero.charm + "\r\n"
                                                + "招募金：" + selectedHero.price;
        }

        /// <summary>
        /// 出征面板
        /// </summary>
        void GoWar()
        {
            // 打开出征面板
            OpenGoWarPanel();
            // 初始化清空出征面板
            EmptyGoWarPanel();
            // 加载城市数据到出征面板
            LoadCityDataToGoWarPanel();

        }

        /// <summary>
        /// 添加确认出征按钮点击事件（调用建立队伍方法BuildTeam()）
        /// </summary>
        void GoWarConfirm()
        {
            BuildNewTeam(countryObj.nationality, takeSoldierNumberInt, currentWord.word, soldierType, null, cityObj.transform.position);
            UpdateCityData();
            UpdateCountryData();
            //关闭所有面板    
            uiController.CloseAllPanels();
            //显示侦查信息
            uiController.ScoutInfoShow(cityObj.nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.TeamBeBuilt, cityObj.gameObject);
        }
        

        /// <summary>
        /// 添加取消按钮点击事件
        /// </summary>
        void CancelBtn(Button _cancelbutton)
        {
            _cancelbutton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                //关闭所有面板
                uiController.CloseAllPanels();
            });
        }

        /// <summary>
        /// 加载并播放单词读音
        /// </summary>
        void PlayWordSound(WordObj _wordobj)
        {
            AudioSource wordAudioSource = GameObject.Find("World").GetComponent<AudioSource>();
            wordAudioSource.clip = _wordobj.pronunciation;
            wordAudioSource.Play();
        }

        /// <summary>
        /// 建立军队  （在添加确认出征按钮点击事件的方法GoWarBtn()中被调用）
        /// </summary>
        /// <param name="_nationality">国籍</param>
        /// <param name="_soldiernumber">士兵数量</param>
        /// <param name="_soldiertype">士兵类型</param>
        /// <param name="_hero">英雄</param>
        /// <param name="_parts">配件（如果是组装型部队，可选择配件）</param>
        /// <param name="_teamPostion">默认使用出生位置</param>
        public void BuildNewTeam(WorldObj.Nationality _nationality, int _soldiernumber, string _hero, WorldObj.SoldierType _soldiertype, string[] _parts, Vector3 _teamPostion)
        {

            Vector3 teamPosition = _teamPostion; //默认使用出生位置   
            float offset = 0;
            switch (_nationality)
            {
                case WorldObj.Nationality.none:
                    offset = 0f;
                    break;
                case WorldObj.Nationality.red:
                    offset = 1f;
                    break;
                case WorldObj.Nationality.blue:
                    offset = -1f;
                    break;
                case WorldObj.Nationality.monster:
                    offset = 0f;
                    break;
                default:
                    break;
            }

            switch (_soldiertype)
            {
                case WorldObj.SoldierType.none:
                case WorldObj.SoldierType.SwordMan:
                case WorldObj.SoldierType.Archer:
                case WorldObj.SoldierType.Cavalry:
                case WorldObj.SoldierType.Worker:
                    teamPosition = _teamPostion + new Vector3(offset, 0f, 0f);
                    break;
                case WorldObj.SoldierType.Griffin:
                case WorldObj.SoldierType.Tank:
                    teamPosition = _teamPostion;
                    break;

                default:
                    break;
            }

            //手动分配PhotonViewID   
            int viewId = PhotonNetwork.AllocateViewID();
            int viewIdModel = PhotonNetwork.AllocateViewID();

            photonView.RPC("BuildNewTeamRPC", PhotonTargets.All, _nationality, _soldiernumber, _hero, _soldiertype, _parts, teamPosition, viewId, viewIdModel);
        }

        [PunRPC]
        void BuildNewTeamRPC(WorldObj.Nationality _nationality, int _soldiernumber, string _hero, WorldObj.SoldierType _soldiertype,  string[] _parts, Vector3 _teamPosition , int _viewId, int _viewIdModel)
        {
            GameObject teamPrefab = teamSwordMan;

            
            switch (_soldiertype)
            {
                case WorldObj.SoldierType.SwordMan:
                    teamPrefab = teamSwordMan;
                    break;
                case WorldObj.SoldierType.Archer:
                    teamPrefab = teamArcher;
                    break;
                case WorldObj.SoldierType.Cavalry:
                    teamPrefab = teamCavalry;
                    break;
                case WorldObj.SoldierType.Worker:
                    teamPrefab = teamWorker;
                    break;
                case WorldObj.SoldierType.Tank:
                    teamPrefab = teamTank;
                    break;
            }
            
            
            //派生一个队伍对象
            newTeam = Instantiate(teamPrefab, _teamPosition, Quaternion.identity) as GameObject;
            //手动分配ViewID
            newTeam.GetComponent<PhotonView>().viewID = _viewId;
            //给新派生的队伍对象赋值
            TeamObj teamObj = newTeam.GetComponent<TeamObj>();
            teamObj.nationality = _nationality;
            teamObj.soldiernumber = _soldiernumber;
            teamObj.soldiertype = _soldiertype;
            teamObj.hero = _hero;
            teamObj.teamspeed = WorldObj.BeginTeamSpeed;
            teamObj.teammorale = WorldObj.BeginTeamMorale;
            //根据士兵类型激活队伍模型
            Transform teamModel = null;
            // 根据国籍和士兵类型找出队伍的模型
            teamModel = FindModel(teamObj.transform,teamObj.nationality,teamObj.soldiertype);
            if (teamModel)
            {
                // 激活部分配件
                SetActiveParts(teamModel, _parts);

                //是否在这里根据配件列表计算生命值(士兵数soldiernumber)，火力值（军心Morola), 速度（teamspeed）

                //手动分配队伍模型的PhotonViewID                
                teamModel.GetComponent<PhotonView>().viewID = _viewIdModel;
            }

            //判断是否是本方国家生成的队伍，如果不是则使其不可见
            foreach (var item in WorldObj.allCountries)
            {
                if (item.photonView.isMine)
                {
                    if (teamObj.nationality != item.nationality)
                    {
                        Funcs.ChangeObjectAlphaValue(teamObj.teamHealthCanvas, 0f);
                        Funcs.ChangeObjectAlphaValue(teamObj.transform, 0f);
                    }
                }

            }
            

        }
        /// <summary>
        /// 根据国籍和士兵类型找出队伍的模型
        /// </summary>
        /// <param name="teamTransform">队伍的根节点</param>
        /// <param name="_nationality">国籍</param>
        /// <param name="_soldierType">士兵类型</param>
        /// <returns></returns>
        public Transform FindModel(Transform teamTransform,WorldObj.Nationality _nationality, WorldObj.SoldierType _soldierType)
        {
            Transform teamModel = null;
            switch (_nationality)
            {
                case WorldObj.Nationality.red:
                    switch (_soldierType)
                    {
                        case WorldObj.SoldierType.SwordMan:
                            teamModel = teamTransform.Find("Footman_Red");
                            break;
                        case WorldObj.SoldierType.Archer:
                            teamModel = teamTransform.Find("Archer_Red");
                            break;
                        case WorldObj.SoldierType.Cavalry:
                            teamModel = teamTransform.Find("Horseman_Red");
                            break;
                        case WorldObj.SoldierType.Worker:
                            teamModel = teamTransform.Find("Worker_Red");
                            break;
                        case WorldObj.SoldierType.Tank:
                            teamModel = teamTransform.Find("Tank_Red");
                            break;
                        default:
                            break;
                    }
                    break;
                case WorldObj.Nationality.blue:
                    switch (_soldierType)
                    {
                        case WorldObj.SoldierType.SwordMan:
                            teamModel = teamTransform.Find("Footman_Blue");
                            break;
                        case WorldObj.SoldierType.Archer:
                            teamModel = teamTransform.Find("Archer_Blue");
                            break;
                        case WorldObj.SoldierType.Cavalry:
                            teamModel = teamTransform.Find("Horseman_Blue");
                            break;
                        case WorldObj.SoldierType.Worker:
                            teamModel = teamTransform.Find("Worker_Blue");
                            break;
                        case WorldObj.SoldierType.Tank:
                            teamModel = teamTransform.Find("Tank_Blue");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return teamModel;
        }

        /// <summary>
        /// 激活部分配件
        /// </summary>
        /// <param name="_teamModel">模型</param>
        /// <param name="_parts">待激活的配件列表</param>
        public void SetActiveParts(Transform _teamModel,string[] _parts)
        {
            if (!_teamModel)
            {
                return;
            }

            //激活队伍模型
            _teamModel.gameObject.SetActive(true);

            if (_parts != null)
            {
                //根据配件列表激活模型的部分配件
                List<string> parts = new List<string>(_parts);
                Transform part = null;
                for (int i = 0; i < _teamModel.childCount; i++)
                {
                    part = _teamModel.GetChild(i);
                    part.gameObject.SetActive(parts.Contains(part.name));

                }
                //子模型里的配件也需要激活
                for (int i = 0; i < _teamModel.childCount; i++)
                {
                    if (_teamModel.GetChild(i).childCount > 0)
                    {
                        Transform teamChildModel = _teamModel.GetChild(i);
                        for (int j = 0; j < teamChildModel.childCount; j++)
                        {
                            part = teamChildModel.GetChild(j);
                            part.gameObject.SetActive(parts.Contains(part.name));
                        }
                    }
                }
            }
         }

        /// <summary>
        /// 更新城市数据
        /// </summary>
        void UpdateCityData()
        {
            //同步更新武将
            cityObj.AddOrRemoveHero(currentWord.word, false);
            //计算剩余士兵数量
            switch (soldierType)
            {
                case WorldObj.SoldierType.SwordMan:
                    cityObj.swordmannumber -= takeSoldierNumberInt;
                    break;
                case WorldObj.SoldierType.Archer:                    
                    cityObj.archernumber -= takeSoldierNumberInt;
                    break;
                case WorldObj.SoldierType.Cavalry:
                    cityObj.cavalrynumber -= takeSoldierNumberInt;
                    break;
                case WorldObj.SoldierType.Worker:
                    cityObj.workernumber -= takeSoldierNumberInt;
                    break;
                default:
                    break;
            }
            //同步更新士兵
            cityObj.UpdateSoldiers(cityObj.swordmannumber, cityObj.archernumber, cityObj.cavalrynumber, cityObj.workernumber);

            //如果武将弃城出征
            if (cityObj.heros.Count == 0)
            {
                //同步更新人口
                //cityObj.UpdatePopulation(cityObj.population + cityObj.swordmannumber+ cityObj.archernumber+ cityObj.cavalrynumber);
                //同步更新士兵清零
                //cityObj.UpdateSoldiers(0, 0, 0);
                //同步更新国籍
                cityObj.GiveNationality(WorldObj.Nationality.none);
                cityObj.GiveFlag();
            }

        }
        /// <summary>
        /// 更新国家数据,如果武将全部出征，国家城列表减少
        /// </summary>
        void UpdateCountryData()
        {
            if (cityObj.heros.Count == 0)
            {
                //countryObj.UpdateCities(cityObj.word, false);
                bool gameOver = true;
                foreach (var item in FindObjectsOfType<CityObj>())
                {
                    if (item.nationality == countryObj.nationality)
                    {
                        gameOver = false;
                        break;
                    }                    
                }
                if (gameOver)
                {
                    //为了测试方便暂时注销
                    //countryObj.CountryDestruction();
                }
            }
        }
        #endregion



        #region Private Methods ——招募面板 Recruit()
        /// <summary>
        /// 添加招募面板事件
        /// </summary>
        void AddRecruitPanelListeners()
        {
            //添加确认招募按钮的点击事件
            recruitButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                //给城市单词测试部分的控件赋值
                LoadWordTest(currentWord);
                ShowWordTest(RecruitPanel, currentWord);
                currentButtonAction = buttonAction.RecruitConfirm;
                //在单词测试面板部分的ButtonAction方法中根据条件执行RecruitConfirm();
            });
            //添加取消招募按钮的点击事件
            CancelBtn(recruitCancelButton.GetComponent<Button>());
        }

        /// <summary>
        /// 打开招募面板
        /// </summary>
        void OpenRecruitPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开招募面板
            RecruitPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// 初始化清空招募面板
        /// </summary>
        void EmptyRecruitPanel()
        {
            //清空武将显示盒子
            EmptyHeroListBox(recruitHerosListBox);
        }

        /// <summary>
        /// 加载城市数据到招募面板
        /// </summary>
        void LoadCityDataToRecruitPanel()
        {
            List<string> availableHeros = FindObjectOfType<WorldObj>().AvailableHeros();
            if (availableHeros.Count == 0)
            {
                LoadWordInfoShow(currentWord, buttonAction.none, false);
                //ShowWordInfoShow(currentWord);
            }
            else
            {
                //加载武将单词数据到招募面板
                LoadHerosToListBox(recruitHerosListBox, availableHeros, recruitWordText, recruitWordImage, recruitHeroText);
            }

        }

        /// <summary>
        /// 招募面板，
        /// </summary>
        void Recruit()
        {
            // 打开招募面板
            OpenRecruitPanel();

            //初始化清空招募面板
            EmptyRecruitPanel();

            // 加载城市数据到招募面板
            LoadCityDataToRecruitPanel();
        }
        /// <summary>
        /// 确认招募
        /// </summary>
        bool RecruitConfirm()
        {

            //调用招募方法增加武将到城市    
            if (RecruitHero(currentWord.word))    
            {
                //关闭所有面板    
                uiController.CloseAllPanels();    
                return true;
            }
            else
            {
                //关闭所有面板    
                uiController.CloseAllPanels();
                return false;
            }
        }
        /// <summary>
        /// 招募英雄
        /// </summary>
        /// <param name="_heroword">要招募的英雄名字</param>
        /// <returns></returns>
        bool RecruitHero(string _heroword)
        {
            HeroObj heroObj = WorldObj.MatchHero(_heroword);

            if (heroObj.nationality == WorldObj.Nationality.none)
            {
                //更新国家数据（使用金钱）
                if (!countryObj.MoneyReduce(heroObj.price))
                {
                    Debug.Log("国家财政紧缺，无法招募武将！");
                    return false;
                }
                heroObj.GiveNationality(cityObj.nationality);
                cityObj.AddOrRemoveHero(_heroword,true);
                return true;
            }
            else
            {
                Debug.Log("该英雄刚刚被其他国家招募了");
                return false;
            }
        }
        #endregion


        #region Private Methods ——征兵面板 Recruit()
        /// <summary>
        /// 添加征兵面板事件
        /// </summary>
        void AddConscriptPanelListeners()
        {
            //添加checkbox兵种选择点击事件
            conscriptCheckboxSwordMan.GetComponent<Toggle>().onValueChanged.AddListener(ConscriptSwordMan);
            conscriptCheckboxArcher.GetComponent<Toggle>().onValueChanged.AddListener(ConscriptArcher);
            conscriptCheckboxCavalry.GetComponent<Toggle>().onValueChanged.AddListener(ConscriptCavalry);
            conscriptCheckboxWorker.GetComponent<Toggle>().onValueChanged.AddListener(ConscriptWorker);

            //添加带兵数拖动变化事件
            sliderConscript.GetComponent<Slider>().onValueChanged.AddListener(ConscriptChanged);


            //添加确认征兵按钮的点击事件
            conscriptButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                //如果没有征兵，则点击无效
                if (conscriptSwordManNumber.GetComponent<Text>().text == "0"
                    && conscriptArcherNumber.GetComponent<Text>().text == "0"
                    && conscriptCavalryNumber.GetComponent<Text>().text == "0"
                    && conscriptWorkerNumber.GetComponent<Text>().text == "0")
                {
                    return;
                }
                //给城市单词测试部分的控件赋值
                LoadWordTest(currentWord);
                ShowWordTest(ConscriptPanel, currentWord);
                currentButtonAction = buttonAction.ConscriptConfirm;
                //在单词测试面板部分的ButtonAction方法中根据条件执行ConscriptConfirm();
            });
            //添加取消招募按钮的点击事件
            CancelBtn(conscriptCancelButton.GetComponent<Button>());
        }
        /// <summary>
        /// 征兵兵种选择事件的回调函数
        /// </summary>
        /// <param name="arg0"></param>
        private void ConscriptSwordMan(bool arg0)
        {
            sliderConscript.GetComponent<Slider>().value = 0;
            //国家金钱限制了可征兵的数量
            sliderConscript.GetComponent<Slider>().maxValue
                = Mathf.Min(cityObj.population, Mathf.FloorToInt(countryObj.money / WorldObj.ConscriptFeeCoefficient))
                  - int.Parse(conscriptWorkerNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptArcherNumber.GetComponent<Text>().text) 
                  - int.Parse(conscriptCavalryNumber.GetComponent<Text>().text);                
        }
        private void ConscriptArcher(bool arg0)
        {
            sliderConscript.GetComponent<Slider>().value = 0;
            sliderConscript.GetComponent<Slider>().maxValue 
                = Mathf.Min(cityObj.population, Mathf.FloorToInt(countryObj.money / WorldObj.ConscriptFeeCoefficient))
                  - int.Parse(conscriptWorkerNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptSwordManNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptCavalryNumber.GetComponent<Text>().text);

        }
        private void ConscriptCavalry(bool arg0)
        {
            sliderConscript.GetComponent<Slider>().value = 0;
            sliderConscript.GetComponent<Slider>().maxValue 
                = Mathf.Min(cityObj.population, Mathf.FloorToInt(countryObj.money / WorldObj.ConscriptFeeCoefficient))
                  - int.Parse(conscriptWorkerNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptArcherNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptSwordManNumber.GetComponent<Text>().text);

        }
        private void ConscriptWorker(bool arg0)
        {
            sliderConscript.GetComponent<Slider>().value = 0;
            sliderConscript.GetComponent<Slider>().maxValue
                = Mathf.Min(cityObj.population, Mathf.FloorToInt(countryObj.money / WorldObj.ConscriptFeeCoefficient))
                  - int.Parse(conscriptCavalryNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptArcherNumber.GetComponent<Text>().text)
                  - int.Parse(conscriptSwordManNumber.GetComponent<Text>().text);

        }

        /// <summary>
        /// 征兵数拖动变化事件的回调函数
        /// </summary>
        private void ConscriptChanged(float arg0)
        {
            //拖动sliderConscript改变各兵种征兵数量
            if (conscriptCheckboxSwordMan.GetComponent<Toggle>().isOn)
            {
                conscriptSwordManInt = Mathf.RoundToInt(sliderConscript.GetComponent<Slider>().value);
                conscriptSwordManNumber.GetComponent<Text>().text = conscriptSwordManInt.ToString();
            }else if (conscriptCheckboxArcher.GetComponent<Toggle>().isOn)
            {
                conscriptArcherInt = Mathf.RoundToInt(sliderConscript.GetComponent<Slider>().value);
                conscriptArcherNumber.GetComponent<Text>().text = conscriptArcherInt.ToString();
            } else if (conscriptCheckboxCavalry.GetComponent<Toggle>().isOn)
            {
                conscriptCavalryInt = Mathf.RoundToInt(sliderConscript.GetComponent<Slider>().value);
                conscriptCavalryNumber.GetComponent<Text>().text = conscriptCavalryInt.ToString();
            }
            else if (conscriptCheckboxWorker.GetComponent<Toggle>().isOn)
            {
                conscriptWorkerInt = Mathf.RoundToInt(sliderConscript.GetComponent<Slider>().value);
                conscriptWorkerNumber.GetComponent<Text>().text = conscriptWorkerInt.ToString();
            }
            //计算剩余人口数并显示
            availablePopulation.GetComponent<Text>().text = (cityObj.population 
                - int.Parse(conscriptSwordManNumber.GetComponent<Text>().text)
                - int.Parse(conscriptArcherNumber.GetComponent<Text>().text)
                - int.Parse(conscriptCavalryNumber.GetComponent<Text>().text)
                - int.Parse(conscriptWorkerNumber.GetComponent<Text>().text)).ToString();
        }

        /// <summary>
        /// 打开征兵面板
        /// </summary>
        void OpenConscriptPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开征兵面板
            ConscriptPanel.gameObject.SetActive(true);
        }
        /// <summary>
        /// 初始化清空征兵面板
        /// </summary>
        void EmptyConscriptPanel()
        {
            conscriptWordText.GetComponent<Text>().text = "";
            conscriptWordImage.GetComponent<Image>().sprite = null;
            //checkbox group
            conscriptCheckboxSwordMan.GetComponent<Toggle>().isOn = true;

            conscriptSwordManNumber.GetComponent<Text>().text = "0";
            conscriptArcherNumber.GetComponent<Text>().text = "0";
            conscriptCavalryNumber.GetComponent<Text>().text = "0";
            conscriptWorkerNumber.GetComponent<Text>().text = "0";

            availablePopulation.GetComponent<Text>().text = "0";
            sliderConscript.GetComponent<Slider>().value = 0;
            sliderConscript.GetComponent<Slider>().maxValue = 0;

            //各兵种的征兵数量清零
            conscriptSwordManInt = 0;
            conscriptArcherInt = 0;
            conscriptCavalryInt = 0;
            conscriptWorkerInt = 0;
        }
        /// <summary>
        /// 加载城市数据到征兵面板
        /// </summary>
        void LoadCityDataToConscriptPanel()
        {
            conscriptWordText.GetComponent<Text>().text = currentWord.word + "\r\n"+ currentWord.soundmark + "\r\n" + currentWord.chinese + "\r\n" + currentWord.sentence;
            conscriptWordImage.GetComponent<Image>().sprite = currentWord.sprite;
            PlayWordSound(currentWord);
            availablePopulation.GetComponent<Text>().text = cityObj.population.ToString();
            sliderConscript.GetComponent<Slider>().maxValue = Mathf.Min(cityObj.population, Mathf.FloorToInt(countryObj.money / WorldObj.ConscriptFeeCoefficient));

        }
        /// <summary>
        /// 征兵面板
        /// </summary>
        void Conscript()
        {
            // 打开征兵面板
            OpenConscriptPanel();

            //初始化清空征兵面板
            EmptyConscriptPanel();

            // 加载城市数据到征兵面板
            LoadCityDataToConscriptPanel();

        }

        /// <summary>
        /// 确认征兵
        /// </summary>
        bool ConscriptConfirm()
        {
            //使用国家金钱
            if (!countryObj.MoneyReduce(Mathf.FloorToInt(WorldObj.ConscriptFeeCoefficient * (conscriptSwordManInt + conscriptArcherInt + conscriptCavalryInt + conscriptWorkerInt))))
            {
                Debug.Log("国家财政紧缺，征兵失败！");
                uiController.CloseAllPanels();
                return false;
            }
            else
            {
                //城市人口减少
                cityObj.UpdatePopulation(cityObj.population - (conscriptSwordManInt + conscriptArcherInt + conscriptCavalryInt + conscriptWorkerInt));
                //增加各兵种数量
                cityObj.UpdateSoldiers
                    (
                    cityObj.swordmannumber + conscriptSwordManInt, 
                    cityObj.archernumber + conscriptArcherInt, 
                    cityObj.cavalrynumber + conscriptCavalryInt,
                    cityObj.workernumber + conscriptWorkerInt
                    );
                uiController.CloseAllPanels();
                return true;
            }                        
        }
        #endregion


        #region Private Methods ——城防面板 CityDefence()
        /// <summary>
        /// 添加城防面板事件
        /// </summary>
        void AddCityDefencePanelListeners()
        {
            //添加要修筑的城防值拖动变化事件
            sliderCityDefence.GetComponent<Slider>().onValueChanged.AddListener(CityDefenceChanged);


            //添加确认修筑城防按钮的点击事件
            citydefenceButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                //如果没有选择值，则点击无效
                if (citydefenceInt == 0)
                {
                    return;
                }
                //给城市单词测试部分的控件赋值
                LoadWordTest(currentWord);
                ShowWordTest(CityDefencePanel, currentWord);
                currentButtonAction = buttonAction.CityDefenceConfirm;
                //在单词测试面板部分的ButtonAction方法中根据条件执行CityDefenceConfirm();
            });
            //添加取消修筑城防按钮的点击事件
            CancelBtn(citydefenceCancelButton.GetComponent<Button>());
        }

        /// <summary>
        /// 修筑城防值拖动变化事件的回调函数
        /// </summary>
        /// <param name="arg0"></param>
        private void CityDefenceChanged(float arg0)
        {
            citydefenceInt = Mathf.RoundToInt(sliderCityDefence.GetComponent<Slider>().value);
            citydefenceValue.GetComponent<Text>().text = (cityObj.citydefence + citydefenceInt).ToString();
            citydefenceMoneyInt = Mathf.FloorToInt(citydefenceInt * WorldObj.CityDefenceFeeCoefficient);
            citydefenceMoneyValue.GetComponent<Text>().text = citydefenceMoneyInt.ToString();
        }

        /// <summary>
        /// 打开城防面板
        /// </summary>
        void OpenCityDefencePanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开城防面板
            CityDefencePanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// 初始化清空城防面板
        /// </summary>
        void EmptyCityDefencePanel()
        {
            citydefenceWordText.GetComponent<Text>().text = "";
            citydefenceWordImage.GetComponent<Image>().sprite = null;


            citydefenceValue.GetComponent<Text>().text = "0";
            sliderCityDefence.GetComponent<Slider>().value = 0;
            sliderCityDefence.GetComponent<Slider>().maxValue = 0;

            //要修筑的城防数值清零
            citydefenceInt = 0;
        }

        /// <summary>
        /// 加载城市数据到城防面板
        /// </summary>
        void LoadCityDataToCityDefencePanel()
        {
            citydefenceWordText.GetComponent<Text>().text = currentWord.word + "\r\n" + currentWord.soundmark + "\r\n" + currentWord.chinese + "\r\n" + currentWord.sentence;
            citydefenceWordImage.GetComponent<Image>().sprite = currentWord.sprite;
            PlayWordSound(currentWord);
            citydefenceValue.GetComponent<Text>().text = cityObj.citydefence.ToString();
            sliderCityDefence.GetComponent<Slider>().maxValue = Mathf.FloorToInt(countryObj.money / WorldObj.CityDefenceFeeCoefficient);

        }

        /// <summary>
        /// 城防面板
        /// </summary>
        void CityDefence()
        {
            //打开城防面板
            OpenCityDefencePanel();
            //初始化清空城防面板
            EmptyCityDefencePanel();
            //加载城市数据到城防面板
            LoadCityDataToCityDefencePanel();
        }

        /// <summary>
        /// 确认修筑城防
        /// </summary>
        bool CityDefenceConfirm()
        {
            //使用国家金钱
            if (!countryObj.MoneyReduce(Mathf.FloorToInt(WorldObj.CityDefenceFeeCoefficient * citydefenceInt)))
            {
                Debug.Log("国家财政紧缺，修筑城防失败！");
                uiController.CloseAllPanels();
                return false;
            }
            else
            {
                //增加城防数值
                cityObj.UpdateDefence(cityObj.citydefence+citydefenceInt);
                uiController.CloseAllPanels();
                return true;
            }
        }
        #endregion



        #region Private Methods ——建造武器面板 BuildWeapon()
        /// <summary>
        /// 建造武器面板
        /// </summary>
        public void BuildWeapon()
        {
            //打开建造武器面板
            OpenBuildWeaponPanel();
            // 初始化建造武器面板
            IntializeBuildWeaponPanel();
        }

        /// <summary>
        /// 打开建造武器面板
        /// </summary>
        void OpenBuildWeaponPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //打开建造武器面板            
            uiCamera.gameObject.SetActive(true);            
            BuildWeaponPanel.gameObject.SetActive(true);
            WorldUI.gameObject.SetActive(!BuildWeaponPanel.gameObject.GetActive());
        }

        /// <summary>
        /// 初始化建造武器面板
        /// </summary>
        void IntializeBuildWeaponPanel()
        {
            PartLoad partLoad = BuildWeaponPanel.Find("Parts").GetComponent<PartLoad>();
            partLoad.OpenModelParts();
        }

        #endregion


    }
}