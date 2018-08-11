using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class PartController : MonoBehaviour
    {
        public Transform modelRedRoot { get; set; }
        public Transform modelBlueRoot { get; set; }
        public Transform[] modelRedChildren { get; set; }
        public Transform[] modelBlueChildren { get; set; }
        //国籍
        CountryObj selfCountry;
        void Start()
        {
            //确定所在国籍
            selfCountry = WorldObj.SelfCountry();

        }
        //点击该脚本所在的配件执行
        void OnClick()
        {
            if (!modelRedRoot || !modelRedRoot)
            {
                Debug.LogError("模型根节点未设置，请在属性面板设置模型根节点。");
                return;
            }


            //根据国籍和点击的该脚本所在配件的名字选择模型配件
            switch (selfCountry.nationality)
            {

                case WorldObj.Nationality.red:
                    Transform partRed = modelRedRoot.Find(this.gameObject.name);
                    if (partRed) //在模型的根节点，根据点击的配件名称，找到需要的配件并激活或屏蔽
                    {
                        partRed.gameObject.SetActive(!partRed.gameObject.GetActive());
                    }
                    else //在模型的所有子节点，根据点击的配件名称，找到需要的配件并激活或屏蔽
                    {
                        foreach (var item in modelRedChildren)
                        {
                            Transform partChildRed = item.Find(this.gameObject.name);
                            if (partChildRed)
                            {
                                partChildRed.gameObject.SetActive(!partChildRed.gameObject.GetActive());
                            }
                        }
                    }
                    break;
                case WorldObj.Nationality.blue:
                    Transform partBlue = modelBlueRoot.Find(this.gameObject.name);
                    if (partBlue)
                    {
                        partBlue.gameObject.SetActive(!partBlue.gameObject.GetActive());
                    }
                    else
                    {
                        foreach (var item in modelBlueChildren)
                        {
                            Transform partChildBlue = item.Find(this.gameObject.name);
                            if (partChildBlue)
                            {
                                partChildBlue.gameObject.SetActive(!partChildBlue.gameObject.GetActive());
                            }
                        }

                    }

                    break;
                case WorldObj.Nationality.monster:
                    break;
                default:
                    break;
            }




        }

    }
}
