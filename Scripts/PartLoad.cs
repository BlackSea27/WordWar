using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class PartLoad : MonoBehaviour
    {
        public Transform allModelsRoot; //整个模型成品和模型配件选择栏的界面的总根        
        public Transform partRedRoot; //红方模型配件选择栏的配件根
        public Transform partBlueRoot;//蓝方模型配件选择栏的配件根
        public Transform partColorlessRoot;//无色模型配件选择栏的配件根
        public Transform modelRedRoot; //模型成品的根节点
        public Transform modelBlueRoot;
        public Transform[] modelRedChildren; //模型成品的子根节点
        public Transform[] modelBlueChildren;        

        //打开模型配件总程序
        public void OpenModelParts()
        {
            //初始化模型和配件的激活信息（根据模型自带的信息空物体，用空物体名字的方式保存激活信息）
            InitializeActiveParts();
            // 设置应激活的模型和配件,并增加操作响应代码组件
            ActivePartsAndAddComponent();
        }

        /// <summary>
        /// 设置应激活的模型和配件,并增加操作响应代码组件
        /// </summary>
        void ActivePartsAndAddComponent()
        {
            //确定所在国籍
            CountryObj selfCountry = null;
            foreach (var item in WorldObj.allCountries)
            {
                if (item.photonView.isMine)
                {
                    selfCountry = item;
                }
            }

            //为模型成品增加操作响应代码组件ModelController
            if (!modelRedRoot.GetComponent<ModelController>())
            {
                ModelController modelController = modelRedRoot.gameObject.AddComponent<ModelController>();
                modelController.modelChildren = modelRedChildren;
                modelController.model = modelRedRoot;
            }
            if (!modelBlueRoot.GetComponent<ModelController>())
            {
                ModelController modelController = modelBlueRoot.gameObject.AddComponent<ModelController>();
                modelController.modelChildren = modelBlueChildren;
                modelController.model = modelBlueRoot;
            }


            //为无色配件增加操作响应代码组件PartController
            for (int i = 0; i < partColorlessRoot.childCount; i++)
            {
                GameObject partColorless = partColorlessRoot.GetChild(i).gameObject;
                if (!partColorless.GetActive())
                {
                    partColorless.SetActive(true);
                }
                if (!partColorless.GetComponent<PartController>())
                {
                    PartController partController = partColorless.AddComponent<PartController>();
                    partController.modelRedRoot = modelRedRoot;
                    partController.modelBlueRoot = modelBlueRoot;
                    partController.modelRedChildren = modelRedChildren;
                    partController.modelBlueChildren = modelBlueChildren;
                }
            }

            //根据国籍激活配件和模型的根节点,并增加操作响应代码组件
            switch (selfCountry.nationality)
            {
                case WorldObj.Nationality.red:
                    modelRedRoot.gameObject.SetActive(true);
                    partRedRoot.gameObject.SetActive(true);
                    //为红方配件增加操作响应代码组件
                    for (int i = 0; i < partRedRoot.childCount; i++)
                    {
                        GameObject partRed = partRedRoot.GetChild(i).gameObject;
                        if (!partRed.GetActive())
                        {
                            partRed.SetActive(true);
                        }
                        if (!partRed.GetComponent<PartController>())
                        {
                            PartController partController = partRed.AddComponent<PartController>();
                            partController.modelRedRoot = modelRedRoot;
                            partController.modelBlueRoot = modelBlueRoot;
                            partController.modelRedChildren = modelRedChildren;
                            partController.modelBlueChildren = modelBlueChildren;
                        }
                    }
                    break;
                case WorldObj.Nationality.blue:
                    modelBlueRoot.gameObject.SetActive(true);
                    partBlueRoot.gameObject.SetActive(true);
                    //为蓝方配件增加操作响应代码组件
                    for (int i = 0; i < partBlueRoot.childCount; i++)
                    {
                        GameObject partBlue = partBlueRoot.GetChild(i).gameObject;
                        if (!partBlue.GetActive())
                        {
                            partBlue.SetActive(true);
                        }
                        if (!partBlue.GetComponent<PartController>())
                        {
                            PartController partController = partBlue.AddComponent<PartController>();
                            partController.modelRedRoot = modelRedRoot;
                            partController.modelBlueRoot = modelBlueRoot;
                            partController.modelRedChildren = modelRedChildren;
                            partController.modelBlueChildren = modelBlueChildren;
                        }
                    }
                    break;
                case WorldObj.Nationality.monster:
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// 设置前初始化，根据模型内自带的ActiveInfo，用空物体名字的方式保存激活信息
        /// 模型自带初始化激活信息规则：
        /// 所有激活信息存储在名字是info子物体的ActiveInfo子物体下的子物体的名字里
        /// 以“ ”分割激活信息，分割后不带“/”的信息段字符串就是要激活的模型或配件名字
        /// 分割后不带“/”的信息段字符串包含：
        ///     1.“/”前面的是根节点模型或配件，但不表示要激活该模型或配件
        ///     2.“/”后面的是以“，”分割的要激活的配件名字
        /// </summary>
        public void InitializeActiveParts()
        {
            //先将所有激活的模型和配件关闭激活
            for (int i = 0; i < modelRedRoot.childCount; i++)
            {
                modelRedRoot.GetChild(i).gameObject.SetActive(false);
            }
            for (int i = 0; i < modelBlueRoot.childCount; i++)
            {
                modelBlueRoot.GetChild(i).gameObject.SetActive(false);
            }
            for (int i = 0; i < partRedRoot.childCount; i++)
            {
                partRedRoot.GetChild(i).gameObject.SetActive(false);
            }
            for (int i = 0; i < partBlueRoot.childCount; i++)
            {
                partBlueRoot.GetChild(i).gameObject.SetActive(false);
            }
            for (int i = 0; i < partColorlessRoot.childCount; i++)
            {
                partColorlessRoot.GetChild(i).gameObject.SetActive(false);
            }
            foreach (var item in modelRedChildren)
            {
                for (int i = 0; i < item.childCount; i++)
                {
                    item.GetChild(i).gameObject.SetActive(false);
                }
            }
            foreach (var item in modelBlueChildren)
            {
                for (int i = 0; i < item.childCount; i++)
                {
                    item.GetChild(i).gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < allModelsRoot.childCount; i++)
            {
                allModelsRoot.GetChild(i).gameObject.SetActive(false);
            }
            //再根据激活信息激活
            allModelsRoot.Find("Info").gameObject.SetActive(true);
            allModelsRoot.Find("Info").Find("ActiveInfo").gameObject.SetActive(true);
            allModelsRoot.Find("Info").Find("ActiveInfo").GetChild(0).gameObject.SetActive(true);
            string initializeInfo = allModelsRoot.Find("Info").Find("ActiveInfo").GetChild(0).name;
            string[] infos = initializeInfo.Split(' ');
            foreach (var item in infos)
            {
                if (!item.Contains("/"))
                {
                    if (allModelsRoot.Find(item))
                    {
                        allModelsRoot.Find(item).gameObject.SetActive(true);
                    }                    
                }
                else
                {
                    string[] childRootAndChildren = item.Split('/');
                    string childRoot = childRootAndChildren[0];
                    string children = childRootAndChildren[1];
                    string[] childrenArray = children.Split(',');
                    foreach (var member in childrenArray)
                    {
                        Transform childRootTransform = allModelsRoot.Find(childRoot);
                        if (childRootTransform && childRootTransform.Find(member))
                        {
                            childRootTransform.Find(member).gameObject.SetActive(true);
                        }                        
                    }
                }
            }
        }
    }
}
