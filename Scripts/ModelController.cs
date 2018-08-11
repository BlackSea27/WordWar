using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class ModelController : MonoBehaviour
    {
        //队伍预制体的根节点
        public Transform model { get; set; }
        //模型的子节点数组
        public Transform[] modelChildren { get; set; }

        //城堡控制器组件
        CityController cityController;
        //UI控制器
        UIController uiController;
        //队伍控制器组件
        TeamController teamController;
        //国家组件
        CountryObj selfCountry;
        //世界UI
        Transform WorldUI;
        //队伍提示面板
        Transform teamPanel;

        void Start()
        {
            WorldUI = GameObject.Find("PlayerCamera").transform.Find("WorldUI(Clone)").transform;
            teamPanel = WorldUI.Find("TeamPanel");
            //确定所在国籍
            selfCountry = WorldObj.SelfCountry();

            cityController = selfCountry.GetComponent<CityController>();
            teamController = selfCountry.GetComponent<TeamController>();
            uiController = selfCountry.GetComponent<UIController>();
        }

        //点击该脚本所在的模型执行
        void OnClick()
        {
            if (teamController.hintModel)
            {
                Destroy(teamController.hintModel.gameObject);
            }
            //记录已选择的模型样式（配件选择）
            RecordModel();
            //关闭建造窗口并提示建造
            CloseSelectAndHintBuild();



        }
        //记录已选择的模型样式（配件选择）
        void RecordModel()
        {
            List<string> partsnames = new List<string>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.GetActive())
                {
                    partsnames.Add(transform.GetChild(i).name);
                }                
            }

            foreach (var item in modelChildren)        
            {        
                for (int i = 0; i < item.childCount; i++)        
                {        
                    if (item.GetChild(i).gameObject.GetActive())        
                    {        
                        partsnames.Add(item.GetChild(i).name);        
                    }        
                }        
            }        
            
            //保存配件字符串数组
            teamController.teamObj.partsnames = partsnames.ToArray();
                 
        }
        //关闭建造窗口并提示建造
        void CloseSelectAndHintBuild()
        {
            //关闭建造窗口
            uiController.CloseAllPanels();
            //打开世界UI
            WorldUI.gameObject.SetActive(!cityController.BuildWeaponPanel.gameObject.GetActive());
            //队伍提示面板关闭
            teamPanel.gameObject.SetActive(false);
            //提示选择位置建造信息
            uiController.ScoutInfoShow(selfCountry.nationality, WorldObj.AnimatorAction.Victory, WorldObj.ScoutInfoType.WhereBuild, teamController.teamObj.gameObject);

            //生成提示建造模型
            if (model)
            {
                Transform aHintModel = teamController.teamObj.hintModel;
                if (aHintModel)
                {
                    Destroy(teamController.teamObj.hintModel.gameObject);
                }
                //生成提示模型
                aHintModel = Instantiate(model, teamController.teamObj.transform.position + new Vector3(0f, 1.5f, 2f), Quaternion.Euler(20f, 0f, 0f));
                //按配件信息字符串数组激活模型的部分配件
                cityController.SetActiveParts(aHintModel, teamController.teamObj.partsnames);
                //删除提示模型上的组件
                Destroy(aHintModel.GetComponent<Animator>());
                Destroy(aHintModel.GetComponent<Collider>());
                Destroy(aHintModel.GetComponent<ModelController>());
                Destroy(aHintModel.GetComponentInChildren<Animator>());
                Destroy(aHintModel.GetComponentInChildren<Animation>());
                //使提示模型半透明
                /*
                MeshRenderer[] meshRenderers = teamController.hintModel.GetComponentsInChildren<MeshRenderer>();                
                foreach (var item in meshRenderers)
                {
                    foreach (var child in item.materials)
                    {
                       child.shader = Shader.Find("Transparent/Diffuse");
                        Color c = child.color;
                        c = new Color(c.r, c.g, c.b, c.a*2/ 5);
                        child.color = c;
                    }                    
                }
                */
                //一定时间后删除
                //Destroy(teamController.hintModel.gameObject,10f);
                //模型跟随闪烁提示建造
                teamController.ModelFlashHint(aHintModel, teamController.teamObj.transform);

                teamController.teamObj.hintModel = aHintModel;


            }
        }

    }
}