using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Aionline.WordsCities
{


    public class OptionsController : Photon.MonoBehaviour
    {

        #region Var
        //选项菜单面板
        public Transform OptionsMenuPanel;
        //所有选项按钮所在的面板
        Transform allOptions;
        //退出程序按钮
        Button quitGame;

        //国家对象上的组件
        UIController uiController;

        //世界空间UI
        Transform WorldUI;
        //屏幕左下角选项菜单按钮
        Button optionsButton;

        #endregion


        void Start()
        {
            if (!photonView.isMine)
            {
                return;
            }
            //获得UIController组件
            uiController = GetComponent<UIController>();
            //获得选项菜单面板及面板上的其它UI
            OptionsMenuPanel = GameObject.Find("Canvas(Clone)").transform.Find("OptionsMenuPanel");
            allOptions = OptionsMenuPanel.Find("AllOptions");
            quitGame = allOptions.Find("QuitGame").GetComponent<Button>();

            WorldUI = GameObject.Find("WorldUI(Clone)").transform;
            optionsButton = WorldUI.Find("Panel").Find("Options").GetComponent<Button>();



            //点击事件添加
            //打开选项面板
            AddOptionsButtonListener();
            //退出程序
            AddOptionMenuPanelListener();
        }

        #region Public Methods
        
        /// <summary>
        /// 选项菜单显示
        /// </summary>
        public void OptionsMenu()
        {
            //打开选项菜单面板
            OpenOptionsMenuPanel();
        }

        #endregion

        #region Private Methods 选项菜单面板

        /// <summary>
        /// 选项菜单按钮点击事件添加
        /// </summary>
        void AddOptionsButtonListener()
        {
            optionsButton.onClick.AddListener(delegate ()
            {
                if (!WorldObj.UIRun)
                {
                    OptionsMenu();
                }
                else
                {
                    //uiController.CloseAllPanels();
                }
                
            });

        }
        /// <summary>
        /// 选项菜单面板上的按钮点击事件添加
        /// </summary>
        void AddOptionMenuPanelListener()
        {
            //退出程序的按钮
            quitGame.onClick.AddListener(delegate () 
            {
                Application.Quit();
            });


        }

        /// <summary>
        /// 打开选项菜单面板
        /// </summary>
        void OpenOptionsMenuPanel()
        {
            //关闭所有面板
            uiController.CloseAllPanels();
            //设置UI运行为真
            WorldObj.UIRun = true;
            //激活选项菜单面板
            OptionsMenuPanel.gameObject.SetActive(true);
        }

        #endregion

        #region Private Methods 声音面板
        #endregion
    }
}