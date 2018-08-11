using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class Funcs : MonoBehaviour
    {

        /// <summary>
        /// 物体缓慢出现或消失
        /// </summary>
        /// <param name="_object">物体</param>
        /// <param name="_disappearTime">缓慢消失或出现的时间</param>
        /// <param name="_frameNumber">希望用的帧数</param>
        /// <param name="_appear">出现或消失选择</param>
        public static IEnumerator SlowDisappearIE(Transform _object, float _disappearTime, float _frameNumber, bool _appear)
        {
            float alphaValue;
            if (_appear)
            {
                alphaValue = 0.1f;
            }
            else
            {
                alphaValue = 0.95f;
            }

            Color c;
            while (_object && alphaValue>0 && alphaValue<1)
            {                
                CanvasGroup canvasGroup = _object.GetComponent<CanvasGroup>();
                if (canvasGroup) //如果是UI
                {
                    canvasGroup.alpha = alphaValue;
                }
                else //如果不是普通物体
                {
                    foreach (var item in _object.GetComponentsInChildren<MeshRenderer>())
                    {
                        foreach (var child in item.materials)
                        {
                            c = child.color;
                            c = new Color(c.r, c.g, c.b, alphaValue);
                            child.color = c;
                        }
                    }
                    foreach (var item in _object.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        foreach (var child in item.materials)
                        {
                            c = child.color;
                            c = new Color(c.r, c.g, c.b, alphaValue);
                            child.color = c;                            
                        }
                    }
                }

                yield return new WaitForSeconds(_disappearTime / _frameNumber);
                if (_appear)
                {
                    alphaValue += 1 / _frameNumber;
                }
                else
                {
                    alphaValue -= 1 / _frameNumber;
                }
                
            }
        }



        /// <summary>
    /// 改变物体的透明度
    /// </summary>
    /// <param name="_object">物体</param>
    /// <param name="_alpha">透明度0到1</param>
        public static void ChangeObjectAlphaValue(Transform _object, float _alpha)
        {
            /*
            MeshRenderer[] meshRenderers = _object.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in meshRenderers)
            {
                foreach (var child in item.materials)
                {
                    child.shader = Shader.Find("Transparent/Diffuse");
                    Color c = child.color;
                    c = new Color(c.r, c.g, c.b, _alpha);
                    child.color = c;
                }
            }
            */
            
            Color c;
            CanvasGroup canvasGroup = _object.GetComponent<CanvasGroup>();
            if (canvasGroup) //如果是UI
            {
                canvasGroup.alpha = _alpha;
            }
            else //如果不是普通物体
            {
                foreach (var item in _object.GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (var child in item.materials)
                    {
                        if (child.shader == Shader.Find("Unlit/Transparent")
                            || child.shader == Shader.Find("Specular"))
                        {
                            child.shader = Shader.Find("Transparent/Diffuse");
                        }
                        c = child.color;
                        c = new Color(c.r, c.g, c.b, _alpha);
                        child.color = c;
                        //Debug.Log(child.color.a);

                    }
                }
                foreach (var item in _object.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    foreach (var child in item.materials)
                    {
                        if (child.shader == Shader.Find("Unlit/Texture"))
                        {
                            child.shader = Shader.Find("Toon/Basic"); //此处的Toon/Basic是手动更改过的
                        }
                        c = child.color;
                        c = new Color(c.r, c.g, c.b, _alpha);
                        child.color = c;
                        //Debug.Log(child.color.a);
                    }
                }
            }
        }

        /// <summary>
        /// 改变物体的色彩
        /// </summary>
        /// <param name="_object">物体</param>
        /// <param name="_color">色彩</param>
        public static void ChangeObjectColor(Transform _object, Color _color)
        {

            MeshRenderer[] meshRenderers = _object.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in meshRenderers)
            {
                foreach (var child in item.materials)
                {
                    child.shader = Shader.Find("Transparent/Diffuse");
                    child.color = _color;
                }
            }
        }

        /// <summary>
        /// 改变物体的可视性（隐形或可见）
        /// </summary>
        /// <param name="_object">物体</param>
        /// <param name="_visuality">可视性</param>
        public static void SetObjectVisuality(Transform _object,bool _visuality)
        {
            foreach (var item in _object.GetComponentsInChildren<MeshRenderer>())
            {
                item.enabled = _visuality;
            }
        }

        //****************************************************************************************************************************************
        /// <summary>
        /// 下载文件从网络路径到本地路径
        /// </summary>
        /// <param name="_fileName">文件名，带后缀</param>
        /// <param name="_netPath">网络存储路径，最后带斜杠“/”</param>
        /// <param name="_localPath">本地路径，最后带斜杠“/”</param>
        public static void DownLoadFileFromTo(string _fileName, string _netPath, string _localPath)
        {
            //定义_webClient对象
            WebClient _webClient = new WebClient();
            //使用默认的凭据——读取的时候，只需默认凭据就可以
            _webClient.Credentials = CredentialCache.DefaultCredentials;
            //下载的链接地址（文件服务器）
            Uri _uri = new Uri(@_netPath+_fileName);
            //注册下载进度事件通知
            _webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
            //注册下载完成事件通知
            _webClient.DownloadFileCompleted += _webClient_DownloadFileCompleted;
            //异步下载到D盘
            _webClient.DownloadFileAsync(_uri, @_localPath+ _fileName);
            Debug.Log(_uri + "   " + @_localPath + _fileName);
            //Console.ReadKey();
        }

 
         //下载完成事件处理程序
         private static void _webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
         {
            Debug.Log("下载完成......");
            //Console.WriteLine("Download Completed...");
        }
 
         //下载进度事件处理程序
         private static void _webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
         {
            Debug.Log("下载进度："+e.ProgressPercentage + "%");
             //Console.WriteLine($"{e.ProgressPercentage}:{e.BytesReceived}/{e.TotalBytesToReceive}");   
         }


    

        /// <summary>
        /// 读取资源返回字符数组
        /// </summary>
        /// <param name="path">文件存储路径</param>
        /// <returns></returns>
        public static byte[] ReadTexture(string path)
        {
            Debug.Log(" @ ! the texture path is + !!    " + path);
            FileStream fileStream = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[fileStream.Length]; //创建文件长度的buffer   
            fileStream.Read(buffer, 0, (int)fileStream.Length);

            fileStream.Close();

            fileStream.Dispose();

            fileStream = null;

            return buffer;
        }



        /// <summary>
        /// 函数：用WWW类从本地文件获取AssetBundle (弃用AssetBundle.LoadFromFile(u_bundlenamepath)，安卓平台下不运作，原因未知)
        /// </summary>
        /// <param name="u_bundlenamepath">本地AssetBundle文件路径和文件名字符串</param>
        /// <returns>返回一个AssetBundle实例</returns>
        public static AssetBundle WWWBundleFromLocalFile(string u_bundlenamepath)
        {

            WWW www01 = new WWW(u_bundlenamepath);
            while (!www01.isDone) { }

            if (www01.error != null)
            {
                Debug.Log(www01.error);
                return null;
                //GameObject.Find("Bat1").GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                AssetBundle m_bundle = www01.assetBundle;
                www01.Dispose();
                return m_bundle;

            }

        }


        //************************************************************************************************************
        /// <summary>
        /// 转向到目标
        /// </summary>
        /// <param name="originalObj">原物体</param>
        /// <param name="targetPoint">目标点</param>
        /// <param name="speed">每指定间隔时间旋转幅度</param>
        /// <param name="perTime">间隔时间</param>
        /// <returns></returns>
        public static IEnumerator TurnToTarget(Transform originalObj, Vector3 targetPoint,float speed, float perTime)
        {
            Vector3 final = GetLookAtEuler(originalObj, targetPoint);
            while (originalObj.rotation.eulerAngles != final)
            {
                originalObj.rotation = Quaternion.RotateTowards(originalObj.rotation, Quaternion.Euler(final), speed);
                yield return new WaitForSeconds(perTime);
            }
        }


        /// <summary>
        /// 获取物体LookAt后的旋转值
        /// </summary>
        /// <param name="originalObj"></param>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public static Vector3 GetLookAtEuler(Transform originalObj, Vector3 targetPoint)
        {
            //计算物体在朝向某个向量后的正前方
            Vector3 forwardDir = targetPoint - originalObj.position;

            //计算朝向这个正前方时的物体四元数值
            Quaternion lookAtRot = Quaternion.LookRotation(forwardDir);

            //把四元数值转换成角度
            Vector3 resultEuler = lookAtRot.eulerAngles;

            return resultEuler;
        }

        //************************************************************************************************************

        /// <summary>
        /// 函数：从指定摄像头发送射线指定屏幕位置到场景位置，返回射线碰到对象的RaycastHit
        /// </summary>
        /// <param name="u_camera">指定摄像头</param>
        /// <param name="u_pos">屏幕位置（可以是鼠标或触摸点击位置）</param>
        /// <returns>发送的射线碰到对象的RaycastHit</returns>
        public static RaycastHit RayScreenPoint(Transform u_camera, Vector3 u_pos)
        {
            //从摄像头发送射线指定屏幕位置到场景位置
            Ray m_ray = u_camera.GetComponentInChildren<Camera>().ScreenPointToRay(u_pos);

            RaycastHit hit;

            Physics.Raycast(m_ray, out hit);

            return hit;

        }





        /// <summary>    
        /// 函数：射线查找前方与tag数组有相同tag的单位，返回第一个找到的单位    
        /// </summary>    
        /// <param name="u_orginpoint">射线发射点</param>    
        /// <param name="u_tags">可匹配的tag数组</param>    
        /// <returns></returns>    
        public static GameObject RayFindByTag(Transform u_orginpoint, string[] u_tags)
        {
            GameObject m_target = null;
            RaycastHit hitinfo;

            if (Physics.Raycast(u_orginpoint.position, u_orginpoint.forward, out hitinfo))
            {
                foreach (var item in u_tags)
                {
                    if (hitinfo.collider.tag == item)
                    {
                        m_target = hitinfo.collider.gameObject;
                        break;
                    }
                }
            }
            return m_target;
        }


        /// <summary>
        /// 函数：射线查找指定点周围指定距离的指定角度（指定点正前方正负度数绝对值之和）的范围内与tag数组有相同tag的单位，返回第一个找到的单位。
        /// </summary>
        /// <param name="u_orginpoint">射线发射点</param>
        /// <param name="u_tags">可匹配的tag数组</param>
        /// <param name="u_angle">指定点周围指定角度(发射点正前方为0度)</param>
        /// <param name="u_distance">指定距离范围（即扫描半径)</param>
        /// <returns></returns>
        public static GameObject RayFindForwordAroundByTag(Transform u_orginpoint, string[] u_tags, int u_angle, float u_distance)
        {
            GameObject m_target = null;
            //探测正前方的左右正负度数和为指定度数的区域
            RaycastHit hitinfo;
            for (int i = -u_angle / 2; i <= u_angle / 2; i += 20)
            {
                if (Physics.Raycast(u_orginpoint.position, Quaternion.Euler(0f, i, 0f) * u_orginpoint.forward, out hitinfo, u_distance))
                {
                    //Debug.Log(hitinfo.collider.name);
                    foreach (var item in u_tags)
                    {
                        if (hitinfo.collider.tag == item)
                        {
                            m_target = hitinfo.collider.gameObject;
                            break;
                        }
                    }
                }
                if (m_target != null) { break; } //找到第一个目标就停止扫描
            }


            return m_target;
        }

    }
}


