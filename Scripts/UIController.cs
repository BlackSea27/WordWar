using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Com.Aionline.WordsCities
{
    public class UIController : MonoBehaviour
    {
        #region Public var
        //UI的Assets
        public GameObject uiWorld { get; set; }
        public GameObject uiScreen { get; set; }
        public GameObject uiCamera { get; set; }
        #endregion
        #region Private var
        //国家对象上的组件
        CityController cityController;
        OptionsController optionsController;
        Animator scoutAnimator;

        //UI
        Transform WorldUI;
        Transform teamScout;
        Text scoutInfoText; //侦查信息显示UI
        Transform ScreenUI;
        Transform CameraUI;


        #endregion
        private void Awake()
        {
            // 初始化世界UI
            BeginWorldUI();
            //初始化UI
            BeginScoutUI();
            //初始化屏幕UI
            BeginScreenUI();
            // 初始化摄像头UI
            BeginCameraUI();
        }
        void Start()
        {                                   
            cityController = GetComponent<CityController>();
            optionsController = GetComponent<OptionsController>();

            scoutInfoText = teamScout.Find("ScoutInfo").Find("ScoutInfoText").GetComponent<Text>();
            scoutAnimator = teamScout.GetComponentInChildren<Animator>();
            
        }

        #region Public Methods

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            //设置UI运行为假
            WorldObj.UIRun = false;
            //关闭城市菜单面板
            cityController.CityMenuPanel.gameObject.SetActive(false);
            //关闭出征面板
            cityController.GoWarPanel.gameObject.SetActive(false);
            //关闭招募面板
            cityController.RecruitPanel.gameObject.SetActive(false);
            //关闭征兵面板
            cityController.ConscriptPanel.gameObject.SetActive(false);
            //关闭城防面板
            cityController.CityDefencePanel.gameObject.SetActive(false);
            //关闭单词测试面板
            cityController.wordTest.gameObject.SetActive(false);
            //关闭单词信息显示面板
            cityController.WordInfoShow.gameObject.SetActive(false);
            //关闭建造武器面板
            cityController.BuildWeaponPanel.gameObject.SetActive(false);
            cityController.uiCamera.gameObject.SetActive(false);

            //关闭选项菜单面板
            optionsController.OptionsMenuPanel.gameObject.SetActive(false);
            
        }

        /// <summary>
        /// 侦查信息显示
        /// </summary>
        /// <param name="_nationality">国籍</param>
        /// <param name="_action"></param>
        /// <param name="_scoutInfoType"></param>
        public void ScoutInfoShow(WorldObj.Nationality _nationality, WorldObj.AnimatorAction _action, WorldObj.ScoutInfoType _scoutInfoType, GameObject _infoSourceObj)
        {
            //没有模型激活的情况下按国籍激活模型
            Transform horsemanRed = teamScout.Find("Horseman_Red");
            Transform horsemanBlue = teamScout.Find("Horseman_Blue");
            if (!horsemanRed.gameObject.GetActive() && !horsemanBlue.gameObject.GetActive())
            {
                switch (_nationality)
                {
                    case WorldObj.Nationality.red:
                        horsemanRed.gameObject.SetActive(true);
                        break;
                    case WorldObj.Nationality.blue:
                        horsemanBlue.gameObject.SetActive(true);
                        break;
                    case WorldObj.Nationality.monster:
                        break;
                    default:
                        break;
                }
            }

            //播放动画
            if (!teamScout)
            {
                Debug.Log("没找到teamScout!");
                return;
            }
            if (!scoutAnimator)
            {
                Debug.Log("没找到动画组件");
                return;
            }
            scoutAnimator.SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], false);
            scoutAnimator.SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)_action], true);
            //输出侦查信息
            scoutInfoText.text = WorldObj.ScoutInfoDictionary[_scoutInfoType];
            //存储侦查信息源对象
            WorldObj.scoutInfoSourceObj = _infoSourceObj;
            //5秒后恢复以前的动画和侦查信息
            StartCoroutine(ResetAnimatorRun(5f, WorldObj.AnimatorAction.Run, _action, WorldObj.ScoutInfoType.none));
        }
        /// <summary>
        /// 侦查模型在延迟后恢复以前的动画
        /// </summary>
        /// <param name="_delay">延迟时间</param>
        /// <param name="_animatorActionBefore">以前动画</param>
        /// <param name="_animatorActionNow">当前动画</param>
        /// <returns></returns>
        IEnumerator ResetAnimatorRun(float _delay, WorldObj.AnimatorAction _animatorActionBefore, WorldObj.AnimatorAction _animatorActionNow, WorldObj.ScoutInfoType _scoutInfoType)
        {
            yield return new WaitForSeconds(_delay);
            scoutAnimator.SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)_animatorActionNow], false);
            scoutAnimator.SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)_animatorActionBefore], true);
            scoutInfoText.text = WorldObj.ScoutInfoDictionary[_scoutInfoType];
            WorldObj.scoutInfoSourceObj = null;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 初始化世界UI
        /// </summary>
        void BeginWorldUI()
        {
            uiWorld = PhotonGameManager.assetDictionary["WorldUI"];
            WorldUI = Instantiate(uiWorld).transform;
            WorldUI.SetParent(GameObject.Find("PlayerCamera").transform);
            WorldUI.localPosition = new Vector3(0f, -7.8f, 6);
            WorldUI.localRotation = Quaternion.Euler(0f, 0f, 0f);
            WorldUI.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            WorldUI.GetComponent<Canvas>().worldCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        }

        /// <summary>
        /// 初始化ScoutUI
        /// </summary>
        void BeginScoutUI()
        {            
            //ScoutInfoShow(WorldObj.SelfCountry().nationality,WorldObj.AnimatorAction.Run,WorldObj.ScoutInfoType.none,);

            Transform countryScout = PhotonGameManager.assetDictionary["TeamScout"].transform;
            teamScout = Instantiate(countryScout);
            teamScout.SetParent(WorldUI.Find("Panel"));
            teamScout.localScale = new Vector3(70f, 70f, 70f);
            teamScout.localPosition = new Vector3(-57f, 2f, -3f);
            Quaternion scoutRotation = Quaternion.Euler(0f, 90f, -60f);
            teamScout.localRotation = scoutRotation;
            /*
            switch (WorldObj.SelfCountry().nationality)
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
            */
        }

        /// <summary>
        /// 初始化屏幕UI
        /// </summary>
        void BeginScreenUI()
        {
            uiScreen = PhotonGameManager.assetDictionary["Canvas"];
            ScreenUI = Instantiate(uiScreen).transform;
            ScreenUI.localPosition = new Vector3(282f, 475f, 0f);
            ScreenUI.localRotation = Quaternion.Euler(0f, 0f, 0f);
            ScreenUI.localScale = new Vector3(0.51339f, 0.51339f, 0.51339f);

        }

        /// <summary>
        /// 初始化摄像头UI
        /// </summary>
        void BeginCameraUI()
        {
            uiCamera = PhotonGameManager.assetDictionary["CameraUI"];
            CameraUI = Instantiate(uiCamera).transform;
            CameraUI.localPosition = new Vector3(0f, 0f, 0f);
            CameraUI.localRotation = Quaternion.Euler(0f, 0f, 0f);
            CameraUI.localScale = new Vector3(0.06347f, 0.06347f, 0.06347f);

        }
        #endregion
    }
}
