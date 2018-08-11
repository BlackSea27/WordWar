using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml;
using System.Linq;
using System.Net;
using System;

namespace Com.Aionline.WordsCities
{


    public class PhotonGameManager : Photon.PunBehaviour
    {
        //Word预制体，需拖拽到面板属性
        public Transform Word;

        Camera MainCamera;
        bool connected = false;
        bool joined = false;
        //单词包的名称
        string wordPackageName;
        //界面
        Transform CanvasNet;
        Transform WordPackage;
        Transform ScrollFrame;
        Button WordPackageButton;
        Button BeginButton;
        Image SelectedPackageImage;
        Slider ProgressSlider;
        Transform ModePanel;
        Transform RacePanel;
        Transform MatchPanel;
        Button AIModeButton;
        Button MatchModeButton;
        Button WordSelectButton;
        Button QuitGameButton;
        Button AIReturnButton;
        Button AIBeginButton;
        Button MatchReturnButton;
        Button MatchButton;
        Button ConfirmMatchButton;
        Button CancelMatchButton;
        Transform ConfirmCircle;
        Image ConfirmCircleBar;

        //AI
        /*
        //协程
        Coroutine leftReadyCoroutine;
        Coroutine rightReadyCoroutine;
        */
        bool readyBeginAI = false;
        PunTeams.Team readyBeginAITeam;

        //存放临时变量
        string[] wordContents;                                //单词内容的按行存放的字符串数组
        Sprite sprite;                                        //图片的Sprite缓存变量
        AudioClip audioClip;                                  //读音的AudioClip缓存变量

        /*
        //单词包版本字典
        Dictionary<string, float[]> wordVersionDictionary = new Dictionary<string, float[]>();
        //下载单词包时用的WWW变量
        Dictionary<string, WWW> wordWWWDictionary = new Dictionary<string, WWW>();
        */


        //根据Asset本身的名称创建的所有Asset的Dictionary
        public static Dictionary<string, GameObject> assetDictionary = new Dictionary<string, GameObject>();
        //版本字典
        Dictionary<string, float[]> versionDictionary = new Dictionary<string, float[]>();
        //下载bundle时用的WWW变量
        Dictionary<string, WWW> bundleWWWDictionary = new Dictionary<string, WWW>();
        //将下载的每个bundle占总大小的比例的字典（小于等于1的浮点数）
        Dictionary<string, float> bundleWeightDictionary = new Dictionary<string, float>();
        //解压时用的变量
        Dictionary<string, AssetBundleRequest> bundleRequestDictionary = new Dictionary<string, AssetBundleRequest>();
        //将下载的bundles的总大小
        long bundlesTotalSize;
        //已完成的下载进度（小于等于1的浮点数）
        float completedProgress;

        //下载是否完成
        bool downCompleted = false;
        //解压是否完成
        bool loadAssetCompleted = false;


        #region GameNet Methods
        /*
        public void JoinWar()
        {
            if (PhotonNetwork.connected && PhotonNetwork.connectionStateDetailed == ClientState.Joined)
            {
                if (loadAssetCompleted)
                {
                    SceneManager.LoadScene(1);
                }
                else
                {
                    GameObject.Find("Text5").GetComponent<Text>().text = "游戏内容未下载完成，请等待...";
                }
                
            }
        }
        

        public void RedTeamSet() 
        {
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.player.SetTeam(PunTeams.Team.red);
                PhotonNetwork.JoinOrCreateRoom("WordsCities", null, null);
            }
        }

        public void BlueTeamSet()
        {
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
                PhotonNetwork.JoinOrCreateRoom("WordsCities", null, null);
            }
        }
        */
        /*
        public void MatchGame()
        {
            if (PhotonNetwork.connected && PhotonNetwork.connectionStateDetailed != ClientState.Joined)
            {
                if (loadAssetCompleted)
                {
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    GameObject.Find("Text5").GetComponent<Text>().text = "游戏内容未下载完成，请等待...";
                }
            }
        }
        */

        #endregion

        #region Pun Message
        public override void OnConnectedToMaster()
        {
            Debug.Log("连接成功");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("加入房间成功！");                     
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("加入房间失败！" + codeAndMsg[0] + codeAndMsg[1]);

        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            //Debug.Log("随机加入房间失败，开始自建房间！" + codeAndMsg[0] + codeAndMsg[1]);
            /*
            if (PhotonNetwork.connected)
                PhotonNetwork.CreateRoom("WordsCities", roomOptions: new RoomOptions { MaxPlayers = 2 }, typedLobby: null);
            */
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("创建房间成功！");
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("创建房间失败！");
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            /*
            if (newPlayer != null)
            {
                PhotonNetwork.player.SetTeam(PunTeams.Team.red);
                newPlayer.SetTeam(PunTeams.Team.blue);
                SceneManager.LoadScene(1);
            }
            */
        }
        #endregion

        #region 下载单词内容，图片，声音相关方法
        /// <summary>
        /// 载入单词内容到单词行内容数组
        /// </summary>
        /// /// <param name="_path">单词内容文件本地存放目录</param>
        /// <param name="_url">单词内容文件存放目录</param>
        /// <param name="_wordcontent">单词内容文件名</param>
        bool LoadWordContent(string _path, string _url, string _wordcontent)
        {            
            bool result = true;
            WWW wordContentText;

            wordContentText = new WWW(_url + _wordcontent + WorldObj.WordpackeFormat);
            while (!wordContentText.isDone) { }
            /*
            string localPath = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.IPhonePlayer:
                    localPath = _path;
                    break;
                case RuntimePlatform.Android:       //安卓平台更改路径
                    localPath = "file://" + _path;
                    break;
                default:
                    break;
            }

            if (!File.Exists(_path + _wordcontent + WorldObj.WordpackeFormat))// || versionDictionary[_wordcontent][1] > versionDictionary[_wordcontent][0])
            {                
                wordContentText = new WWW(_url + _wordcontent + WorldObj.WordpackeFormat);
                while (!wordContentText.isDone) { }

                if (wordContentText != null && string.IsNullOrEmpty(wordContentText.error))
                {
                    try
                    {
                        if (!Directory.Exists(_path))
                        {
                            Directory.CreateDirectory(_path);
                        }
                        File.WriteAllBytes(_path + _wordcontent + WorldObj.WordpackeFormat, wordContentText.bytes);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        throw;
                    }                    
                }
            }
            else
            {
                wordContentText = new WWW(localPath + _wordcontent + WorldObj.WordpackeFormat);
                while (!wordContentText.isDone) { }
            }
            */


            if (wordContentText != null && string.IsNullOrEmpty(wordContentText.error))
            {
                wordContents = wordContentText.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
                result = true;
            }
            else
            {
                Debug.Log("没找到单词文件:" + wordContentText.error + "|路径：" + _url + _wordcontent);
                result = false;
            }

            wordContentText.Dispose();
            wordContentText = null;
            return result;
        }


        /// <summary>
        /// 载入图片到缓存变量sprite
        /// </summary>
        /// <param name="_url">单词图片文件存放目录</param>
        /// <param name="_word">单词</param>
        IEnumerator LoadImage(string _path, string _url, string _word)//, string _wordPackageName)
        {
            WWW wwwImage;
            /*
            string localPath = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.IPhonePlayer:
                    localPath = _path;
                    break;
                case RuntimePlatform.Android:       //安卓平台更改路径
                    localPath = "file://" + _path;
                    break;
                default:
                    break;
            }
            if (!File.Exists(_path + _word + WorldObj.ImageFormat))// || versionDictionary[_wordPackageName][1]> versionDictionary[_wordPackageName][0])
            {
                wwwImage = new WWW(_url + _word + WorldObj.ImageFormat);
                yield return wwwImage;
                //while (!wwwImage.isDone) { }
                if (wwwImage != null && string.IsNullOrEmpty(wwwImage.error))
                {
                    try
                    {
                        if (!Directory.Exists(_path))
                        {
                            Directory.CreateDirectory(_path);
                        }
                        File.WriteAllBytes(_path + _word + WorldObj.ImageFormat, wwwImage.bytes);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                wwwImage = new WWW(localPath + _word + WorldObj.ImageFormat);
                yield return wwwImage;
                //while (!wwwImage.isDone) { }

            }
            */
            wwwImage = new WWW(_url + _word + WorldObj.ImageFormat);
            yield return wwwImage;
            if (wwwImage != null && string.IsNullOrEmpty(wwwImage.error))
            {
                Texture2D texture = wwwImage.texture;
                //创建 Sprite 实例
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("未能准备好单词图片文件" + wwwImage.error + "|路径：" + _url + _word + WorldObj.ImageFormat);

            }

            wwwImage.Dispose();
            wwwImage = null;
        }

        /// <summary>
        /// 载入声音到缓存变量audioClip
        /// </summary>
        /// <param name="_url">单词图片文件存放目录</param>
        /// <param name="_word">单词</param>
        IEnumerator LoadSound(string _path ,string _url, string _word)//, string _wordPackageName)
        {
            /*
#if UNITY_STANDALONE_WIN
            var stream = File.Open(_filepath + _word + ".mp3", FileMode.Open);    
            var reader = new Mp3FileReader(stream);    

            WaveFileWriter.CreateWaveFile(_filepath + _word + ".wav", reader);    
            reader.Dispose();    
#endif
            */

            WWW wwwWave;
            /*
            string localPath = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.IPhonePlayer:
                    localPath = _path;
                    break;
                case RuntimePlatform.Android:       //安卓平台更改路径
                    localPath = "file://" + _path;
                    break;
                default:
                    break;
            }

            if (!File.Exists(_path + _word + WorldObj.SoundFormat))// ||  versionDictionary[_wordPackageName][1] > versionDictionary[_wordPackageName][0])
            {
                wwwWave = new WWW(_url + _word + WorldObj.SoundFormat);
                //while (!wwwWave.isDone) { }
                yield return wwwWave;
                if (wwwWave != null && string.IsNullOrEmpty(wwwWave.error))
                {
                    try
                    {
                        if (!Directory.Exists(_path))
                        {
                            Directory.CreateDirectory(_path);
                        }
                        File.WriteAllBytes(_path + _word + WorldObj.SoundFormat, wwwWave.bytes);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                wwwWave = new WWW(localPath + _word + WorldObj.SoundFormat);
                //while (!wwwWave.isDone) { }
                yield return wwwWave;
            }
            */

            wwwWave = new WWW(_url + _word + WorldObj.SoundFormat);
            yield return wwwWave;
            if (wwwWave != null && string.IsNullOrEmpty(wwwWave.error))
            {
                audioClip = wwwWave.GetAudioClip();
            }
            else
            {
                Debug.LogError("未能准备好单词声音文件");
            }
            wwwWave.Dispose();
            wwwWave = null;
        }

        #endregion

        #region 生成单词对象并赋值的方法
        /// <summary>
        /// 生成所有单词实例（在调用LoadWordContent方法后获取单词行内容数组wordContents后调用）
        /// </summary>
        private void SpawnWordObjects()
        {
            //初始化清空所有单词实例
            for (int i = 0; i < WorldObj.allWords.Count; i++)
            {
                Destroy(WorldObj.allWords[i]);
            }
            WorldObj.allWords.Clear();
            //错误判断
            if (wordContents == null || wordContents.Length <= 1)
            {
                Debug.LogError("未能生成单词行内容数组！");
                return;
            }

            //生成所有单词实例
            foreach (var item in wordContents) //按行内容数组wordContents的长度生成单词实例个数
            {
                //生成单词实例
                WordObj newWord = Instantiate(Word).GetComponent<WordObj>();
                WorldObj.allWords.Add(newWord);
            }            
        }

        /// <summary>
        /// 将所有单词对象属性赋值
        /// </summary>        
        IEnumerator AssignAllWordsValues()
        {
            for (int i = 0; i < wordContents.Length; i++)
            {
                //给每个单词赋值属性
                yield return StartCoroutine(AssignWordValues(WorldObj.allWords[i], wordContents[i]));
                //显示单词加载进度
                ProgressSlider.value += 1f / 25f;
            }
            GameObject.Find("Text3").GetComponent<Text>().text = "单词包资源下载完成......";
        }
        /// <summary>
        /// 给每个单词赋值属性
        /// </summary>
        /// <param name="_line">单词行内容</param>
        IEnumerator  AssignWordValues(WordObj _wordObj,string _line)
        {
            //将单词行内容分割
            string[] lineContents = _line.Split(new string[] { "/" }, System.StringSplitOptions.None);

            //单词实例赋值，注意：lineContents[0]在文件里用一个行开头W占位了，因为UT8文件开头估计有字符不正确导致取出的单词去加载图片的时候提示找不到图片
            //将单词行内容的单词部分赋值给单词实例的单词属性
            _wordObj.word = lineContents[1];
            //将单词行内容的音标部分赋值给单词实例的音标属性
            _wordObj.soundmark = lineContents[2];
            //将单词行内容的中文释义部分赋值给单词实例的中文释义属性
            _wordObj.chinese = lineContents[3];
            //将单词行内容的例句部分赋值给单词实例的例句属性
            _wordObj.sentence = lineContents[4];

            //载入图片到缓存变量sprite
            yield return StartCoroutine( LoadImage(Application.persistentDataPath + WorldObj.WordBookLocalPath + wordPackageName + "/Image/", WorldObj.WordBookPath + wordPackageName + "/Image/", lineContents[1])); //, wordPackageName);

            //将图片缓存中的Sprite赋值给单词实例的sprite属性
            _wordObj.sprite = sprite;
            //释放图片缓存            
            sprite = null;

            //载入读音到缓存变量audioClip
            yield return StartCoroutine(LoadSound(Application.persistentDataPath + WorldObj.WordBookLocalPath + wordPackageName + "/Wave/", WorldObj.WordBookPath + wordPackageName + "/Wave/", lineContents[1]));//, wordPackageName);

            //将读音缓存中的AudioClip赋值给单词实例的pronuncation属性
            _wordObj.pronunciation = audioClip;
            //释放读音缓存
            audioClip = null;

        }

        #endregion

        #region 下载 解压 AssetBundle
        /// <summary>
        /// 获取下载的文件大小
        /// </summary>
        /// <returns>文件大小</returns>
        public long GetDownLoadFileSize(string _url)
        {
            HttpWebRequest request = WebRequest.Create(_url) as HttpWebRequest;
            request.Method = "HEAD";
            request.Timeout = 5000;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return response.ContentLength;
        }
        /// <summary>
        /// 获取全部要下载的bundle文件的大小
        /// </summary>
        /// <param name="_bundleNetPath"></param>
        /// <returns></returns>
        long GetAllBundleDownLoadFileSize(string _bundleNetPath)
        {
            long totalSize = 0;
            foreach (var item in bundleWWWDictionary.Keys)
            {
                //检查该bundle是否需要下载，如需要，则将其大小加进去
                if (versionDictionary[item][1]> versionDictionary[item][0])
                {
                    totalSize += GetDownLoadFileSize(_bundleNetPath + item);
                }                
            }
            return totalSize;
        }

        /// <summary>
        /// 获取全部要解压的bundle文件的大小
        /// </summary>
        /// <returns></returns>
        long GetAllBundleSize()
        {
            long totalSize = 0;
            for (int i = 0; i < bundleWWWDictionary.Count; i++)
            {
                WWW www = bundleWWWDictionary.Values.ToArray()[i];
                if ( www!= null)
                {
                    totalSize += www.bytesDownloaded;
                }
            }
            return totalSize;
        }

        /// <summary>
        /// 解压前重新赋值bundle比重字典
        /// </summary>
        void GetNewBundleWeightDictionary()
        {
            bundleWeightDictionary.Clear();
            long totalSize = GetAllBundleSize();
            for (int i = 0; i < bundleWWWDictionary.Count; i++)
            {
                WWW www = bundleWWWDictionary.Values.ToArray()[i];
                string key = bundleWWWDictionary.Keys.ToArray()[i];
                if (www!=null)
                {
                    bundleWeightDictionary.Add(key, www.bytesDownloaded / (float)totalSize);
                }
            }
        }

        /// <summary>
        /// 下载单个bundle
        /// </summary>
        /// <param name="_bundleNetPath">bundle的网络路径</param>
        /// <param name="_bundleLocalPath">bundle的本地路径</param>
        /// <param name="_bundleLocalPathWWW">供WWW使用的bundle的本地路径</param>
        /// <param name="_key">bundle的名字</param>
        IEnumerator BundleDownLoad(string _bundleNetPath, string _bundleLocalPath, string _bundleLocalPathWWW, string _key)
        {
            if (!File.Exists(_bundleLocalPath + _key) || versionDictionary[_key][1] > versionDictionary[_key][0])
            {
                //bundleWeight = GetDownLoadFileSize(_bundleNetPath + _key) / (float)bundlesTotalSize;
                bundleWWWDictionary[_key] = new WWW(_bundleNetPath + _key);
                yield return bundleWWWDictionary[_key];
                completedProgress += bundleWeightDictionary[_key];
                try
                {
                    if (!Directory.Exists(_bundleLocalPath))
                    {
                        Directory.CreateDirectory(_bundleLocalPath);
                    }
                    File.WriteAllBytes(_bundleLocalPath + _key, bundleWWWDictionary[_key].bytes);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    throw;
                }
                
            }
            else
            {
                bundleWWWDictionary[_key] = new WWW(_bundleLocalPathWWW + _key);
                while (!bundleWWWDictionary[_key].isDone) { }
            }

            if (bundleWWWDictionary[_key].error != null)
            {
                GameObject.Find("Text2").GetComponent<Text>().text = "WWWERROR:" + bundleWWWDictionary[_key].error;
                yield return 0;
            }
        }
        /// <summary>
        /// 解压所有bundle
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadAssets()
        {
            //初始化清空Asset存储字典
            assetDictionary.Clear();
            //从Bundle字典加载所有Asset
            int length = bundleWWWDictionary.Count;
            for (int i = 0; i < length; i++)
            {
                WWW www = bundleWWWDictionary.Values.ToArray()[i];
                string key = bundleWWWDictionary.Keys.ToArray()[i];
                bundleRequestDictionary.Add(key, www.assetBundle.LoadAllAssetsAsync<GameObject>());
                
                yield return bundleRequestDictionary[key];
                completedProgress += bundleWeightDictionary[key];

                UnityEngine.Object[] assets = bundleRequestDictionary[key].allAssets;

                foreach (var asset in assets)
                {
                    //创建Asset的存储字典
                    assetDictionary.Add(asset.name, (GameObject)asset);
                }
            }
        }

        #endregion


        #region 版本信息
        /// <summary>
        /// 获取版本字典
        /// </summary>
        void GetVersionDictionary()
        {
            //得到服务器版本号,根据服务器上的XML增加版本字典的键值
            GetServerVersion(WorldObj.FileServerRootPath + WorldObj.XMLRecords);
            //得到本地版本号，如果没有文件则新建一个XML
            GetLocalVersion(Application.persistentDataPath + "/"+ WorldObj.XMLRecords);



            Debug.Log(versionDictionary["sundryall"][0] + "  " + versionDictionary["sundryall"][1]);
            Debug.Log(versionDictionary["teamall"][0] + "  " + versionDictionary["teamall"][1]);
            Debug.Log(versionDictionary["uiall"][0] + "  " + versionDictionary["uiall"][1]);
            Debug.Log(versionDictionary["sceneobjall"][0] + "  " + versionDictionary["sceneobjall"][1]);
            Debug.Log(versionDictionary["city"][0] + "  " + versionDictionary["city"][1]);
            GameObject.Find("Text7").GetComponent<Text>().text = versionDictionary["sundryall"][0] + "  " + versionDictionary["sundryall"][1] + "\n" +
                                                                 versionDictionary["teamall"][0] + "  " + versionDictionary["teamall"][1] + "\n" +
                                                                 versionDictionary["uiall"][0] + "  " + versionDictionary["uiall"][1] + "\n" +
                                                                 versionDictionary["sceneobjall"][0] + "  " + versionDictionary["sceneobjall"][1] + "\n" +
                                                                 versionDictionary["city"][0] + "  " + versionDictionary["city"][1];           
                                                                 
        }

        /// <summary>
        /// 得到服务器版本号
        /// </summary>
        /// <param name="_pathXMLFile">保存版本号的XML文件</param>
        private void GetServerVersion(string _pathXMLFile)
        {                        
            WWW www = new WWW(_pathXMLFile);
            while (!www.isDone) { }

            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("服务器无版本表。");
                //测试阶段帮助建立XML，将来要删除这些代码
                versionDictionary.Add("sundryall", new float[2]);
                versionDictionary.Add("teamall", new float[2]);
                versionDictionary.Add("uiall", new float[2]);
                versionDictionary.Add("sceneobjall", new float[2]);
                versionDictionary.Add("city", new float[2]);
                return;
            }
            doc.InnerXml = www.text;

            XmlElement version = doc.SelectSingleNode("version") as XmlElement;
            //version的第一个子节点名是"#text"，所以i要从1开始而不是0
            versionDictionary.Clear();
            bundleWWWDictionary.Clear();
            for (int i = 1; i < version.ChildNodes.Count; i++)
            {
                //把version节点下面每个子节点的 名字作为版本字典的键值，他们的InnerText作为版本字典的服务器版本号
                versionDictionary.Add(version.ChildNodes[i].Name, new float[2] { 0, float.Parse(version.ChildNodes[i].InnerText)});
                //为budleWWW字典添加键值
                bundleWWWDictionary.Add(version.ChildNodes[i].Name, null);
            }
        }

        /// <summary>
        /// 得到本地版本号，如果没有文件则新建一个XML
        /// </summary>
        /// <param name="_pathXMLFile">保存版本号的XML文件</param>
        private void GetLocalVersion(string _pathXMLFile)
        {                      
            if (!File.Exists(_pathXMLFile))
            {
                XmlDocument doc = new XmlDocument();
                XmlElement version = doc.CreateElement("version");
                version.InnerText = "version";
                doc.AppendChild(version);
                foreach (var item in versionDictionary.Keys)
                {
                    XmlElement bundle = doc.CreateElement(item);
                    bundle.InnerText = "0";
                    version.AppendChild(bundle);
                }
                doc.Save(_pathXMLFile);
            }

            XmlDocument docRead = new XmlDocument();
            docRead.Load(_pathXMLFile);
            XmlElement versionRead = docRead.SelectSingleNode("version") as XmlElement;
            //把本地XML的键值赋给版本字典，如果版本字典的某个键值在XML中不存在对应的节点，则新建节点
            foreach (var item in versionDictionary.Keys)
            {
                if (versionRead.SelectSingleNode(item) == null)
                {
                    XmlElement newBundle = docRead.CreateElement(item);
                    newBundle.InnerText = "0";
                    versionRead.AppendChild(newBundle);
                    docRead.Save(_pathXMLFile);
                }
                versionDictionary[item][0] = float.Parse(versionRead.SelectSingleNode(item).InnerText);
            }

        }



        /// <summary>
        /// 用服务器版本号覆盖本地版本号
        /// </summary>
        /// <param name="_pathXMLFile">本地版本XML文件</param>
        private void OverrideLocalVersion(string _pathXMLFile)
        {
            XmlDocument docWrite = new XmlDocument();
            docWrite.Load(_pathXMLFile);
            XmlElement versionWrite = docWrite.SelectSingleNode("version") as XmlElement;
            foreach (var item in versionDictionary)
            {
                versionWrite.SelectSingleNode(item.Key).InnerText = item.Value[1].ToString();
            }
            docWrite.Save(_pathXMLFile);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 从文件服务器获取开始背景图并显示
        /// </summary>
        void GetBeginUIBackgroundFromServer()
        {
            WWW beginUIImage = new WWW(WorldObj.BeginUIPath + "BeginUIBackground.png");
            while (!beginUIImage.isDone) { }
            if (beginUIImage.error != null)
            {
                Application.Quit();
            }
            Sprite beginUISprite = Sprite.Create(beginUIImage.texture, new Rect(0, 0, beginUIImage.texture.width, beginUIImage.texture.height), new Vector2(0.5f, 0.5f));
            Image CanvasNetImage = CanvasNet.GetComponent<Image>();
            CanvasNetImage.sprite = beginUISprite;
            if (CanvasNetImage.sprite)
            {
                beginUIImage.Dispose();
                beginUIImage = null;
                beginUISprite = null;
            }
        }

        /// <summary>
        /// 添加单词包按钮监听
        /// </summary>
        void AddWordPackageButtonsListener()
        {
            Transform Content = GameObject.Find("Content").transform;
            foreach (var item in Content.GetComponentsInChildren<Button>())
            {
                item.onClick.AddListener(delegate () {
                    SelectedPackageImage.transform.gameObject.SetActive(true);
                    WordPackageButton.gameObject.SetActive(true);
                    // 选择单词包名称
                    ChooseWordPackageName(item);
                });
            }
        }
        /// <summary>
        /// 选择单词包名称
        /// </summary>
        public void ChooseWordPackageName(Button _buttonSelf)
        {
            if (_buttonSelf != null)
            {
                wordPackageName = _buttonSelf.transform.Find("Label").GetComponent<Text>().text;
            }
            SelectedPackageImage.sprite = _buttonSelf.GetComponentInChildren<Image>().sprite;
            SelectedPackageImage.GetComponentInChildren<Text>().text = wordPackageName;

        }

        /// <summary>
        /// 给下载单词包按钮添加点击事件
        /// </summary>
        void AddDownLoadWordPackageButtonListener()
        {
            WordPackageButton.onClick.AddListener(delegate () 
            {
                StartCoroutine(WordPackageButtonClick());
            });
        }
        IEnumerator WordPackageButtonClick()
        {
            //隐藏单词选择画面
            WordPackage.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            //下载某单词包（仅单词内容，不包含图片和读音）            
            if (!LoadWordContent(Application.persistentDataPath + WorldObj.WordContentLocalPath, WorldObj.WordContentPath, wordPackageName))
            {
                Debug.LogError("未能找到单词文件，可能网络未连接或服务器数据被改变。");
                Application.Quit();
            }
            //根据单词包生成单词实例并添加到WorldObj的单词静态对象列表     
            SpawnWordObjects();
            //将所有单词对象属性赋值,包括每个单词的图片和读音下载并赋值     
            yield return StartCoroutine(AssignAllWordsValues());
            if (assetDictionary.Count == 0)
            {
                BeginButton.gameObject.SetActive(true);
            }
            else
            {
                ModePanel.gameObject.SetActive(true);
            }
            
        }

        /// <summary>
        /// 给开始游戏按钮添加点击事件（加载游戏资源）
        /// </summary>
        void AddBeginButtonListener()
        {            
            BeginButton.onClick.AddListener(delegate () 
            {
                //下载及解压
                StartCoroutine(DownLoadAndGetObject());

                BeginButton.gameObject.SetActive(false);

            });
        }

        /// <summary>
        /// 给进入AI房间按钮添加点击事件监听
        /// </summary>
        private void AddAIModeButtonListener()
        {
            AIModeButton.onClick.AddListener(delegate ()
            {
                //先隐藏模式选择面板，避免出现重复点击产生多余协程
                ModePanel.gameObject.SetActive(false);
                //开始进入AI房间协程
                StartCoroutine(WaitIntoAIRoomIE());
                
            });
        }
        /// <summary>
        /// 进入AI房间协程
        /// </summary>
        IEnumerator WaitIntoAIRoomIE()
        {
            //如果连接超时，退出游戏
            int connectedTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
            {
                connectedTimes++;
                if (connectedTimes>100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }
            //进入AI对战种族选择面板
            ReadyForBeginAI();
        }
        /// <summary>
        /// 生成种族模型
        /// </summary>
        Transform SpawnRaceModel(WorldObj.SoldierType _soldierType, WorldObj.Nationality _nationality, Vector3 _scale, Vector3 _rotation,  Vector3 _pos)
        {
            //兵种类型
            string _soldierTypeString = "";
            //国籍
            string _nationalityString = "";
            //根据要生成的模型填充字符串
            switch (_soldierType)
            {
                case WorldObj.SoldierType.none:
                    break;
                case WorldObj.SoldierType.SwordMan:
                    _soldierTypeString = "TeamSwordMan";
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            _nationalityString = "Footman_Red";
                            break;
                        case WorldObj.Nationality.blue:
                            _nationalityString = "Footman_Blue";
                            break;
                    }                    
                    break;
                case WorldObj.SoldierType.Archer:
                    _soldierTypeString = "TeamArcher";
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            _nationalityString = "Archer_Red";
                            break;
                        case WorldObj.Nationality.blue:
                            _nationalityString = "Archer_Blue";
                            break;
                    }
                    break;
                case WorldObj.SoldierType.Cavalry:
                    _soldierTypeString = "TeamCavalry";
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            _nationalityString = "Horseman_Red";
                            break;
                        case WorldObj.Nationality.blue:
                            _nationalityString = "Horseman_Blue";
                            break;
                    }
                    break;
                case WorldObj.SoldierType.Tank:
                    _soldierTypeString = "TeamTank";
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            _nationalityString = "Tank_Red";
                            break;
                        case WorldObj.Nationality.blue:
                            _nationalityString = "Tank_Blue";
                            break;
                    }
                    break;
                case WorldObj.SoldierType.Worker:
                    _soldierTypeString = "TeamWorker";
                    switch (_nationality)
                    {
                        case WorldObj.Nationality.red:
                            _nationalityString = "Worker_Red";
                            break;
                        case WorldObj.Nationality.blue:
                            _nationalityString = "Worker_Blue";
                            break;
                    }
                    break;
            }
            GameObject team = Instantiate(assetDictionary[_soldierTypeString],new Vector3(0,-500,0),Quaternion.identity);
            Transform raceModel = team.transform.Find(_nationalityString);
            raceModel.gameObject.SetActive(true);
            raceModel.SetParent(null);
            Destroy(team);
            Destroy(raceModel.GetComponent<PhotonAnimatorView>());
            Destroy(raceModel.GetComponent<PhotonView>());
            raceModel.localRotation = Quaternion.Euler(_rotation);
            raceModel.localScale = _scale;
            raceModel.position = _pos;
            return raceModel;
        }
        /// <summary>
        /// 移动模型参数打包到一个结构
        /// </summary>
        struct ModelMoveParams
        {
            public Transform model;
            public Vector3 targetPos;
            public Vector3 firstEuler;
            public Vector3 secendEuler;
            public float maxDistanceDelta;
        }
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="_model">模型</param>
        /// <param name="_targetPos">目标位置</param>
        /// <param name="_maxDistanceDelta">每步距离或旋转角度</param>
        /// <param name="firstEuler">第一次转身角度</param>
        /// <param name="secendEuler">第二次转身角度</param>
        /// <returns></returns>
        IEnumerator MoveToPos(ModelMoveParams _modelMoveParams)
            //IEnumerator MoveToPos(Transform _model, Vector3 _targetPos, float _maxDistanceDelta, Vector3 _firstEuler, Vector3 _secendEuler)
        {
            Vector3 firstEuler = Funcs.GetLookAtEuler(_modelMoveParams.model, _modelMoveParams.firstEuler);
            while (_modelMoveParams.model.rotation.eulerAngles != firstEuler)
            {
                _modelMoveParams.model.rotation = Quaternion.RotateTowards(_modelMoveParams.model.rotation, Quaternion.Euler(firstEuler), 500 * _modelMoveParams.maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //_model.LookAt(_firstEuler);
            _modelMoveParams.model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Victory], false);
            _modelMoveParams.model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], true);

            while (_modelMoveParams.model.position != _modelMoveParams.targetPos)
            {
                _modelMoveParams.model.position = Vector3.MoveTowards(_modelMoveParams.model.position, _modelMoveParams.targetPos, _modelMoveParams.maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //_model.LookAt(_secendEuler);
            _modelMoveParams.model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], false);
            _modelMoveParams.model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Victory], true);

            Vector3 secendEuler = Funcs.GetLookAtEuler(_modelMoveParams.model, _modelMoveParams.secendEuler);
            while (_modelMoveParams.model.rotation.eulerAngles != secendEuler)
            {
                _modelMoveParams.model.rotation = Quaternion.RotateTowards(_modelMoveParams.model.rotation, Quaternion.Euler(secendEuler), 500 * _modelMoveParams.maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            /*
            Vector3 firstEuler = Funcs.GetLookAtEuler(_model, _firstEuler);
            while (_model.rotation.eulerAngles !=firstEuler)
            {
                _model.rotation = Quaternion.RotateTowards(_model.rotation, Quaternion.Euler(firstEuler), 500*_maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //_model.LookAt(_firstEuler);
            _model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Victory], false);
            _model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], true);

            while (_model.position != _targetPos)
            {
                _model.position = Vector3.MoveTowards(_model.position, _targetPos, _maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //_model.LookAt(_secendEuler);
            _model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Run], false);
            _model.GetComponent<Animator>().SetBool(Enum.GetNames(typeof(WorldObj.AnimatorAction))[(int)WorldObj.AnimatorAction.Victory], true);

            Vector3 secendEuler = Funcs.GetLookAtEuler(_model, _secendEuler);
            while (_model.rotation.eulerAngles != secendEuler)
            {
                _model.rotation = Quaternion.RotateTowards(_model.rotation, Quaternion.Euler(secendEuler), 500*_maxDistanceDelta);
                yield return new WaitForSeconds(Time.deltaTime);
            }
            */
        }
        /// <summary>
        /// 进入AI对战种族选择面板
        /// </summary>
        void ReadyForBeginAI()
        {           
            PhotonNetwork.CreateRoom(null, new RoomOptions { IsVisible = false }, null);
            ModePanel.gameObject.SetActive(false);
            WorldObj.AIWar = true;            
            RacePanel.gameObject.SetActive(true);
            //生成种族选择模型
            Transform blueModel = SpawnRaceModel(WorldObj.SoldierType.SwordMan, WorldObj.Nationality.blue, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 90, 0), new Vector3(-3, -2.8f, 89.2f));
            Transform redModel = SpawnRaceModel(WorldObj.SoldierType.SwordMan, WorldObj.Nationality.red, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, -90, 0), new Vector3(3, -2.8f, 89.2f));
            blueModel.name = "blueModel";
            redModel.name = "redModel";
            //将模型收纳到新建空对象，以便打包销毁
            GameObject models = new GameObject("Models");
            models.transform.SetParent(RacePanel);
            models.transform.localPosition = Vector3.zero;
            blueModel.SetParent(models.transform);
            redModel.SetParent(models.transform);

            ModelMoveParams modelMoveParamsLeft = new ModelMoveParams();
            ModelMoveParams modelMoveParamsRight = new ModelMoveParams();
            modelMoveParamsLeft.model = blueModel;
            modelMoveParamsRight.model = redModel;
            modelMoveParamsLeft.targetPos = new Vector3(-1f, -2.8f, 89.2f);
            modelMoveParamsRight.targetPos = new Vector3(1f, -2.8f, 89.2f);
            modelMoveParamsLeft.firstEuler = blueModel.position + new Vector3(1, 0, 0);//new Vector3(0f,90f,0f);//
            modelMoveParamsRight.firstEuler = redModel.position + new Vector3(-1, 0, 0);// new Vector3(0f, -90f, 0f);//
            modelMoveParamsLeft.secendEuler = modelMoveParamsLeft.targetPos + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f);//
            modelMoveParamsRight.secendEuler = modelMoveParamsRight.targetPos + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f);//
            modelMoveParamsLeft.maxDistanceDelta = 0.02f;
            modelMoveParamsRight.maxDistanceDelta = 0.02f;
            StartCoroutine("MoveToPos", modelMoveParamsLeft);
            StartCoroutine("MoveToPos", modelMoveParamsRight);
            /*
            Vector3 targetPosReadyLeft = new Vector3(-1f, -2.8f, 89.2f);
            Vector3 targetPosReadyRight = new Vector3(1f, -2.8f, 89.2f);
            Vector3 firstEulerReadyLeft = blueModel.position + new Vector3(1,0,0);//new Vector3(0f,90f,0f);//
            Vector3 firstEulerReadyRight =redModel.position + new Vector3(-1, 0, 0);// new Vector3(0f, -90f, 0f);//
            Vector3 secendEulerReadyLeft = targetPosReadyLeft + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f);//
            Vector3 secendEulerReadyRight = targetPosReadyRight + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f);//
            leftReadyCoroutine = StartCoroutine(MoveToPos(blueModel, targetPosReadyLeft, 0.01f, firstEulerReadyLeft, secendEulerReadyLeft));
            rightReadyCoroutine = StartCoroutine(MoveToPos(redModel, targetPosReadyRight, 0.01f, firstEulerReadyRight, secendEulerReadyRight));
            */
        }
        /// <summary>
        /// 给返回到模式选择按钮添加点击事件监听
        /// </summary>
        void AddAIReturnButtonListener()
        {
            AIReturnButton.onClick.AddListener(delegate ()
            {
                //隐藏种族选择面板返回按钮，避免重复点击
                RacePanel.gameObject.SetActive(false);
                StartCoroutine(WaitOutAIRoom());
            });
        }
        IEnumerator WaitOutAIRoom()
        {
            //如果连接超时，退出游戏
            int leaveTimes = 0;
            while(!PhotonNetwork.inRoom)
            {
                leaveTimes ++;
                if (leaveTimes>100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }
            //离开AI对战房间
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.LeaveRoom();
            }            
            WorldObj.AIWar = false;
            StopAllCoroutines();
            Destroy(RacePanel.Find("Models").gameObject);
            readyBeginAI = false;
            AIBeginButton.gameObject.SetActive(false);
            RacePanel.gameObject.SetActive(false);
            ModePanel.gameObject.SetActive(true);
        }
        /// <summary>
        /// 给AI对战开始按钮添加点击事件监听
        /// </summary>
        void AddAIBeginButtonListener()
        {
            AIBeginButton.onClick.AddListener(delegate ()
            {
                StopAllCoroutines();
                Destroy(RacePanel.Find("Models").gameObject);
                RacePanel.gameObject.SetActive(false);
                                
                SceneManager.LoadSceneAsync(1);
            });
        }

        /// <summary>
        /// 给匹配模式按钮添加点击事件监听
        /// </summary>
        private void AddMatchModeButtonListener()
        {
            MatchModeButton.onClick.AddListener(delegate ()
            {
                //先隐藏模式选择面板，避免出现重复点击产生多余协程
                ModePanel.gameObject.SetActive(false);
                //开始进入Match房间协程
                StartCoroutine(WaitIntoMatchRoomIE());

            });
        }
        /// <summary>
        /// 进入Match房间协程
        /// </summary>
        IEnumerator WaitIntoMatchRoomIE()
        {
            //如果连接超时，退出游戏
            int connectedTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
            {
                connectedTimes++;
                if (connectedTimes > 100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }
            //进入Match面板
            ReadyForBeginMatch();
        }


        /// <summary>
        /// 进入Match面板
        /// </summary>
        void ReadyForBeginMatch()
        {            
            ModePanel.gameObject.SetActive(false);            
            MatchPanel.gameObject.SetActive(true);
            //生成演示模型
            Transform blueModel = SpawnRaceModel(WorldObj.SoldierType.SwordMan, WorldObj.Nationality.blue, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 90, 0), new Vector3(-3, -2f, 89.1f));
            Transform redModel = SpawnRaceModel(WorldObj.SoldierType.SwordMan, WorldObj.Nationality.red, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, -90, 0), new Vector3(3, -2f, 89.1f));
            blueModel.name = "blueModel";
            redModel.name = "redModel";
            //将模型收纳到新建空对象，以便打包销毁
            GameObject models = new GameObject("Models");
            models.transform.SetParent(MatchPanel);
            models.transform.localPosition = Vector3.zero;
            blueModel.SetParent(models.transform);
            redModel.SetParent(models.transform);

            ModelMoveParams modelMoveParamsLeft = new ModelMoveParams();
            ModelMoveParams modelMoveParamsRight = new ModelMoveParams();
            modelMoveParamsLeft.model = blueModel;
            modelMoveParamsRight.model = redModel;
            modelMoveParamsLeft.targetPos = new Vector3(-0.45f, -2f, 88.8f);
            modelMoveParamsRight.targetPos = new Vector3(0.45f, -2f, 88.8f);
            modelMoveParamsLeft.firstEuler = blueModel.position + new Vector3(1, 0, 0);//new Vector3(0f,90f,0f);//
            modelMoveParamsRight.firstEuler = redModel.position + new Vector3(-1, 0, 0);// new Vector3(0f, -90f, 0f);//
            modelMoveParamsLeft.secendEuler = modelMoveParamsLeft.targetPos + new Vector3(1, 0, 0);//new Vector3(0f, 180f, 0f);//
            modelMoveParamsRight.secendEuler = modelMoveParamsRight.targetPos + new Vector3(-1, 0, 0);//new Vector3(0f, 180f, 0f);//
            modelMoveParamsLeft.maxDistanceDelta = 0.02f;
            modelMoveParamsRight.maxDistanceDelta = 0.02f;
            StartCoroutine("MoveToPos", modelMoveParamsLeft);
            StartCoroutine("MoveToPos", modelMoveParamsRight);
            /*
            Vector3 targetPosReadyLeft = new Vector3(-0.45f, -2f, 88.8f);
            Vector3 targetPosReadyRight = new Vector3(0.45f, -2f, 88.8f);
            Vector3 firstEulerReadyLeft = blueModel.position + new Vector3(1, 0, 0);//new Vector3(0f,90f,0f);//
            Vector3 firstEulerReadyRight = redModel.position + new Vector3(-1, 0, 0);// new Vector3(0f, -90f, 0f);//
            Vector3 secendEulerReadyLeft = targetPosReadyLeft + new Vector3(1, 0, 0);//new Vector3(0f, 180f, 0f);//
            Vector3 secendEulerReadyRight = targetPosReadyRight + new Vector3(-1, 0, 0);//new Vector3(0f, 180f, 0f);//
            leftReadyCoroutine = StartCoroutine(MoveToPos(blueModel, targetPosReadyLeft, 0.01f, firstEulerReadyLeft, secendEulerReadyLeft));
            rightReadyCoroutine = StartCoroutine(MoveToPos(redModel, targetPosReadyRight, 0.01f, firstEulerReadyRight, secendEulerReadyRight));
            */
        }
        /// <summary>
        /// 给Match面板返回到模式选择按钮添加点击事件监听
        /// </summary>
        void AddMatchReturnButtonListener()
        {
            MatchReturnButton.onClick.AddListener(delegate ()
            {
                WorldObj.MatchWar = false;
                //如果取消按钮正好在没隐藏状态，也要隐藏,
                //如果匹配按钮正好在隐藏状态，要显示。
                //隐藏匹配面板及返回按钮，避免重复点击。
                CancelMatchButton.gameObject.SetActive(false);
                MatchButton.gameObject.SetActive(true);
                MatchPanel.gameObject.SetActive(false);                
                StartCoroutine("WaitOutMatchRoom");
            });
        }
        IEnumerator WaitOutMatchRoom()
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            //如果连接超时，退出游戏
            int leaveTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
            {
                leaveTimes++;
                if (leaveTimes > 100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }       
            
            //StopCoroutine("MoveToPos");      
            Destroy(MatchPanel.Find("Models").gameObject);
            //MatchButton.gameObject.SetActive(false);
            MatchPanel.gameObject.SetActive(false);
            ModePanel.gameObject.SetActive(true);
            StopAllCoroutines();
        }
        /// <summary>
        /// 给开始匹配按钮添加点击事件监听
        /// </summary>
        void AddMatchButtonListener()
        {
            MatchButton.onClick.AddListener(delegate ()
            {                
                StartCoroutine("Match");
            });
        }
        IEnumerator Match()
        {
            WorldObj.MatchWar = true;
            MatchButton.gameObject.SetActive(false);            
            MatchReturnButton.gameObject.SetActive(false);
            PhotonNetwork.JoinRandomRoom();
            
            //如果加入随机超时，则新建房间
            int connectedTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
            {
                connectedTimes++;
                if (connectedTimes > 5)
                {
                    Debug.Log("加入随机房间失败，自建房间");
                    if (PhotonNetwork.connectionStateDetailed != ClientState.Joining && PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToMaster)
                    {
                        PhotonNetwork.CreateRoom(null, roomOptions: new RoomOptions { MaxPlayers = 2 }, typedLobby: null);
                    }                    
                    break;
                }
                yield return new WaitForSeconds(0.2f);
            }

            //如果新建房间不成功，则重新开始匹配协程
            connectedTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
            {
                connectedTimes++;
                if (connectedTimes > 100)
                {
                    StartCoroutine("Match");
                    yield break;
                    //Debug.Log("新建房间失败，退出游戏");
                    //Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }
            Debug.Log("进入房间成功！");
            //MatchReturnButton.gameObject.SetActive(true);
            CancelMatchButton.gameObject.SetActive(true);
            StartCoroutine("DetectMatch");
        }
        /// <summary>
        /// 侦测匹配协程
        /// </summary>
        /// <returns></returns>
        IEnumerator DetectMatch()
        {
            if (!PhotonNetwork.inRoom)
            {
                Debug.LogError("网络出现异常，退出程序");
                Application.Quit();
            }
            float detectTime = UnityEngine.Random.Range(20f,30f);
            float beginDetectTime = Time.time;
            while (PhotonNetwork.playerList.Count() != 2 && PhotonNetwork.inRoom)
            {
                
                if ((Time.time - beginDetectTime) < detectTime)
                {
                    Debug.Log("侦测中...");
                }
                else
                {                    
                    PhotonNetwork.LeaveRoom();
                    while (PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    StartCoroutine("Match");
                    yield break;                    
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            if (PhotonNetwork.inRoom && PhotonNetwork.playerList.Count() == 2 && WorldObj.MatchWar)
            {
                Debug.Log("有新的玩家加入！");
                GameObject.Find("Text5").GetComponent<Text>().text = "有新的玩家加入！";
                CancelMatchButton.gameObject.SetActive(false);
                MatchReturnButton.gameObject.SetActive(false);
                ConfirmMatchButton.gameObject.SetActive(true);
                StartCoroutine("WaitMatchConfirm");
            }            
        }
        /// <summary>
        /// 匹配成功，等待确认协程
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitMatchConfirm()
        {
            float waitConfirmTime = 20f;
            float waitBeginTime = Time.time;
            ConfirmCircle.gameObject.SetActive(true);
            while ((PhotonNetwork.player.GetTeam() == PunTeams.Team.none || PhotonNetwork.otherPlayers[0].GetTeam() == PunTeams.Team.none)
                && Time.time - waitBeginTime < waitConfirmTime)
            {
                //显示等待确认匹配倒计时
                ConfirmCircleBar.fillAmount = (Time.time - waitBeginTime) / waitConfirmTime;
                GameObject.Find("Text3").GetComponent<Text>().fontSize = 48;
                GameObject.Find("Text3").GetComponent<Text>().text = Mathf.RoundToInt(20-(Time.time - waitBeginTime)).ToString();
                yield return new WaitForSeconds(0.1f);
            }
            //倒计时秒表消失
            GameObject.Find("Text3").GetComponent<Text>().text = "";
            ConfirmCircle.gameObject.SetActive(false);
            if (Time.time - waitBeginTime >= waitConfirmTime)
            {
                ConfirmMatchButton.gameObject.SetActive(false);
                StartCoroutine("CancelMatch");
            }
            else if (PhotonNetwork.player.GetTeam() != PunTeams.Team.none && PhotonNetwork.otherPlayers[0].GetTeam() != PunTeams.Team.none)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    SceneManager.LoadSceneAsync(1);
                    yield break;                    
                }
            }
            
        }
        /// <summary>
        /// 给确认匹配按钮添加点击事件监听
        /// </summary>
        void AddConfirmMatchButtonListener()
        {
            ConfirmMatchButton.onClick.AddListener(delegate ()
            {
                if (PhotonNetwork.isMasterClient)
                {
                    PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
                }
                else
                {
                    PhotonNetwork.player.SetTeam(PunTeams.Team.red);
                }
                ConfirmMatchButton.gameObject.SetActive(false);
            });
        }
        /// <summary>
        /// 给取消匹配按钮添加点击事件监听
        /// </summary>
        void AddCancelMatchButtonListener()
        {
            CancelMatchButton.onClick.AddListener(delegate ()
            {                
                StartCoroutine("CancelMatch");
            });
        }
        IEnumerator CancelMatch()
        {
            WorldObj.MatchWar = false;
            StopCoroutine("Match");
            StopCoroutine("DetectMatch");
            PhotonNetwork.player.SetTeam(PunTeams.Team.none);          
            CancelMatchButton.gameObject.SetActive(false);
            MatchReturnButton.gameObject.SetActive(false);

            //如果连接超时，退出游戏
            int leaveTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.Joined 
                && PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
            {
                leaveTimes++;
                if (leaveTimes > 100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.1f);
            }
            //如果在房间,离开房间
            if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
            {
                PhotonNetwork.LeaveRoom();
            }
            

            //如果连接超时，退出游戏
            leaveTimes = 0;
            while (PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToMaster)
            {
                leaveTimes++;
                if (leaveTimes > 100)
                {
                    Debug.LogError("连接超时，程序退出");
                    Application.Quit();
                }
                yield return new WaitForSeconds(0.2f);
            }

            MatchButton.gameObject.SetActive(true);
            MatchReturnButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// 给返回单词包界面选择按钮添加点击监听事件
        /// </summary>
        private void AddWordSelectButtonListener()
        {
            WordSelectButton.onClick.AddListener(delegate ()
            {                
                ModePanel.gameObject.SetActive(false);
                WordPackage.gameObject.SetActive(true);
            });
                
        }
        /// <summary>
        /// 给退出游戏按钮添加点击事件监听
        /// </summary>
        private void AddQuitGameButtonListener()
        {
            QuitGameButton.onClick.AddListener(delegate ()
            {
                Application.Quit();
            });            
        }

        /// <summary>
        /// 下载及解压主协程
        /// </summary>
        /// <returns></returns>
        IEnumerator DownLoadAndGetObject()
        {
            //下载assetbundle并获取assets
            string bundleNetPath = "";
            string bundleLocalPath = "";
            string bundleLocalPathWWW = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    bundleNetPath = WorldObj.AssetBundlePath;
                    bundleLocalPath = Application.persistentDataPath + WorldObj.AssetBundleLocalPath;
                    bundleLocalPathWWW = bundleLocalPath;
                    Debug.Log(bundleLocalPath);
                    break;
                case RuntimePlatform.IPhonePlayer:
                    break;
                case RuntimePlatform.Android:
                    bundleNetPath = WorldObj.AssetBundleAndroidPath;
                    bundleLocalPath = Application.persistentDataPath + WorldObj.AssetBundleAndroidLocalPath;
                    bundleLocalPathWWW = "file://" + bundleLocalPath;
                    break;
                default:
                    bundleNetPath = WorldObj.AssetBundlePath;
                    break;
            }
            //获取服务器上全部要下载的bundle的总大小
            bundlesTotalSize = GetAllBundleDownLoadFileSize(bundleNetPath);
            //计算每个要下载的bundle的大小占总大小的比重,并赋值bundle比重字典
            if (bundlesTotalSize>0)
            {
                foreach (var item in bundleWWWDictionary.Keys)
                {
                    bundleWeightDictionary.Add(item, GetDownLoadFileSize(bundleNetPath + item) / (float)bundlesTotalSize);
                }
            }
            
            //下载所有bundle并将其www保存在bundleWWWDictionary中
            int count = bundleWWWDictionary.Count;
            for (int i = 0; i < count; i++)
            {
                yield return StartCoroutine(BundleDownLoad(bundleNetPath, bundleLocalPath, bundleLocalPathWWW, bundleWWWDictionary.Keys.ToArray()[i]));
            }
            //下载完成后将完成的下载进度变量归0；
            completedProgress = 0;
            ProgressSlider.value = 0;

            //将本地版本号用服务器版本号覆盖
            OverrideLocalVersion(Application.persistentDataPath + "/" + WorldObj.XMLRecords);

            GameObject.Find("Text3").GetComponent<Text>().text = "游戏资源已全部下载完成......";
            downCompleted = true;

            yield return new WaitForSeconds(0.1f);

            // 解压前重新赋值bundle比重字典
            GetNewBundleWeightDictionary();
            Debug.Log(bundleWeightDictionary.Values.ToArray()[0]
                     + "  " + bundleWeightDictionary.Values.ToArray()[1]
                     + "  " + bundleWeightDictionary.Values.ToArray()[2]
                     + "  " + bundleWeightDictionary.Values.ToArray()[3]
                     + "  " + bundleWeightDictionary.Values.ToArray()[4]);

            //解压已下载的全部bundle
            yield return StartCoroutine(LoadAssets());


            GameObject.Find("Text5").GetComponent<Text>().text = "游戏资源已全部解压完成......";

            loadAssetCompleted = true;

            //释放资源
            for (int i = 0; i < bundleWWWDictionary.Count; i++)
            {
                WWW www = bundleWWWDictionary.Values.ToArray()[i];
                www.assetBundle.Unload(false);
                www.Dispose();
                www = null;
            }

            yield return new WaitForSeconds(5f);
            GameObject.Find("Text0").GetComponent<Text>().text = "";
            GameObject.Find("Text1").GetComponent<Text>().text = "";
            GameObject.Find("Text2").GetComponent<Text>().text = "";
            GameObject.Find("Text3").GetComponent<Text>().text = "";
            GameObject.Find("Text4").GetComponent<Text>().text = "";
            GameObject.Find("Text5").GetComponent<Text>().text = "";
            GameObject.Find("Text6").GetComponent<Text>().text = "";
            GameObject.Find("Text7").GetComponent<Text>().text = "";
        }
        #endregion





        void Awake()
        {

            PhotonNetwork.ConnectUsingSettings("1.0");
            PhotonNetwork.automaticallySyncScene = true;
        }
        void Start()
        {
            //版本字典的获取
            GetVersionDictionary();
            MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            //UI获取
            CanvasNet = GameObject.Find("CanvasNet").transform;
            WordPackage = CanvasNet.Find("WordPackage");
            ScrollFrame = WordPackage.Find("ScrollFrame");
            WordPackageButton = WordPackage.Find("WordPackageButton").GetComponent<Button>();
            BeginButton = CanvasNet.Find("ButtonRed").GetComponent<Button>();
            SelectedPackageImage = WordPackage.Find("SelectedPackageImage").GetComponent<Image>();
            ProgressSlider = CanvasNet.Find("Slider").GetComponent<Slider>();
            ModePanel = CanvasNet.Find("ModePanel").transform;
            RacePanel = CanvasNet.Find("RacePanel").transform;
            MatchPanel = CanvasNet.Find("MatchPanel").transform;
            AIModeButton = ModePanel.Find("AIMode").GetComponent<Button>();
            MatchModeButton = ModePanel.Find("MatchMode").GetComponent<Button>();
            WordSelectButton = ModePanel.Find("WordSelect").GetComponent<Button>();
            QuitGameButton = ModePanel.Find("QuitGame").GetComponent<Button>();
            AIReturnButton = RacePanel.Find("AIReturnButton").GetComponent<Button>();
            AIBeginButton = RacePanel.Find("AIBeginButton").GetComponent<Button>();
            MatchReturnButton = MatchPanel.Find("MatchReturnButton").GetComponent<Button>();
            MatchButton = MatchPanel.Find("MatchButton").GetComponent<Button>();
            ConfirmMatchButton = MatchPanel.Find("ConfirmMatchButton").GetComponent<Button>();
            CancelMatchButton = MatchPanel.Find("CancelMatchButton").GetComponent<Button>();
            ConfirmCircle = MatchPanel.Find("ConfirmCircle");
            ConfirmCircleBar = ConfirmCircle.Find("ConfirmCircleBar").GetComponent<Image>();

            //从文件服务器获取开始背景图并显示
            GetBeginUIBackgroundFromServer();

            //单词包选择WordPackage中
            //给单词包选择按钮添加点击事件
            AddWordPackageButtonsListener();
            //给下载单词包按钮添加点击事件
            AddDownLoadWordPackageButtonListener();
            //给开始游戏按钮添加点击事件
            AddBeginButtonListener();

            //模式选择ModePanel中
            //给进入AI房间按钮添加点击事件监听
            AddAIModeButtonListener();
            //给匹配模式按钮添加点击事件监听
            AddMatchModeButtonListener();
            //给返回单词选择按钮添加点击事件监听
            AddWordSelectButtonListener();
            //给退出游戏按钮添加点击事件监听
            AddQuitGameButtonListener();

            //AI模式RacePanel中
            //给返回模式选择按钮添加点击事件监听
            AddAIReturnButtonListener();
            //给AI对战开始按钮添加点击事件监听
            AddAIBeginButtonListener();

            //Match模式MatchPanel中
            AddMatchReturnButtonListener();
            // 给开始匹配按钮添加点击事件监听
            AddMatchButtonListener();
            // 给取消匹配按钮添加点击事件监听            
            AddCancelMatchButtonListener();
            // 给确认匹配按钮添加点击事件监听            
            AddConfirmMatchButtonListener();

        }



        void Update()
        {
            //如果加载资源解压缩全部完成,则再也不进入该Update
            /*
            if (loadAssetCompleted && joined)
            {
                return;
            }*/

            if (PhotonNetwork.connected && !connected && loadAssetCompleted)
            {
                CanvasNet.Find("ModePanel").gameObject.SetActive(true);
                connected = true;
            }


            //显示下载bundles的进度条
            if (!downCompleted)
            {
                int length = bundleWWWDictionary.Count;
                for (int i = 0; i < length; i++)
                {
                    WWW www = bundleWWWDictionary.Values.ToArray()[i];
                    if (www != null && !www.isDone)
                    {
                        ProgressSlider.value = completedProgress + www.progress * bundleWeightDictionary.Values.ToArray()[i];
                    }
                }
            }
            //显示解压bundles的进度条
            if (downCompleted && !loadAssetCompleted)
            { 
                int length = bundleRequestDictionary.Count;
                for (int i = 0; i < length; i++)
                {
                    AssetBundleRequest request = bundleRequestDictionary.Values.ToArray()[i];
                    if (request != null && !request.isDone)
                    {
                        ProgressSlider.value = completedProgress + request.progress * bundleWeightDictionary.Values.ToArray()[i];
                    }
                }              
            }
            
            //AI模式种族选择，模型操作
            if (RacePanel.gameObject.GetActive() && !readyBeginAI)
            {
                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Vector3 pos;
                    if (Input.GetMouseButtonDown(0))
                    {
                        pos = Input.mousePosition;
                    }
                    else
                    {
                        pos = Input.GetTouch(0).deltaPosition;
                    }

                    //从摄像头发送射线指定屏幕位置到场景位置
                    RaycastHit hit = Funcs.RayScreenPoint(MainCamera.transform, pos);
                    if (!hit.collider)
                    {
                        Debug.Log("未点击到任何物体！");
                        return;
                    }
                    Transform obj = hit.collider.transform;

                    if (obj.name == "blueModel" || obj.name == "redModel" )
                    {
                        readyBeginAI = true;
                        StopAllCoroutines();
                        /*
                        if (leftReadyCoroutine != null || rightReadyCoroutine != null)
                        {
                            StopCoroutine(leftReadyCoroutine);
                            StopCoroutine(rightReadyCoroutine);
                        }
                        */
                        ModelMoveParams modelMoveParams = new ModelMoveParams();
                        modelMoveParams.model = obj;
                        modelMoveParams.targetPos = new Vector3(0, -2f, 89.1f);
                        modelMoveParams.firstEuler = obj.position + new Vector3(0, 0, 1);
                        modelMoveParams.secendEuler = modelMoveParams.targetPos + new Vector3(0, 0, 1);
                        modelMoveParams.maxDistanceDelta = 0.02f;
                        StartCoroutine("MoveToPos", modelMoveParams);
                        /*
                        Vector3 targetPosBegin = new Vector3(0, -2f, 89.1f);
                        Vector3 firstEulerBegin = obj.position +new Vector3(0, 0, 1);//new Vector3(0f,-180f,0f);//
                        Vector3 secendEulerBegin = targetPosBegin +new Vector3(0, 0, 1);//new Vector3(0f, -180f, 0f); //
                        StartCoroutine(MoveToPos(obj, targetPosBegin, 0.01f, firstEulerBegin, secendEulerBegin));
                        */

                        ModelMoveParams modelMoveParamsBack = new ModelMoveParams();
                        /*
                        Vector3 targetPosBack;
                        Vector3 firstEulerBack;
                        Vector3 secendEulerBack;
                        */
                        //设置种族选择变量，并将未选模型返回
                        switch (obj.name)
                        {
                            case "blueModel":
                                readyBeginAITeam = PunTeams.Team.blue;
                                //点击左边蓝色模型，右边红色模型返回
                                modelMoveParamsBack.model = GameObject.Find("redModel").transform;
                                modelMoveParamsBack.targetPos = new Vector3(5, -2.8f, 89.2f);
                                modelMoveParamsBack.firstEuler = obj.position + new Vector3(100, 0, 0);
                                modelMoveParamsBack.secendEuler = modelMoveParamsBack.targetPos + new Vector3(0, 0, -1);
                                modelMoveParamsBack.maxDistanceDelta = 0.02f;
                                StartCoroutine("MoveToPos", modelMoveParamsBack);
                                /*
                                targetPosBack = new Vector3(5, -2.8f, 89.2f);
                                firstEulerBack = obj.position + new Vector3(100, 0, 0);//new Vector3(0f, 90f, 0f); //
                                secendEulerBack = targetPosBack + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f); //
                                StartCoroutine(MoveToPos(GameObject.Find("redModel").transform, targetPosBack, 0.012f, firstEulerBack, secendEulerBack));
                                */
                                break;
                            case "redModel":
                                readyBeginAITeam = PunTeams.Team.red;
                                //点击右边红色模型，左边蓝色模型返回
                                modelMoveParamsBack.model = GameObject.Find("blueModel").transform;
                                modelMoveParamsBack.targetPos = new Vector3(-5, -2.8f, 89.2f);
                                modelMoveParamsBack.firstEuler = obj.position + new Vector3(-100, 0, 0);
                                modelMoveParamsBack.secendEuler = modelMoveParamsBack.targetPos + new Vector3(0, 0, -1);
                                modelMoveParamsBack.maxDistanceDelta = 0.02f;
                                StartCoroutine("MoveToPos", modelMoveParamsBack);
                                /*
                                targetPosBack = new Vector3(-5, -2.8f, 89.2f);
                                firstEulerBack = obj.position + new Vector3(-100, 0, 0); ;//new Vector3(0f, -90f, 0f); //
                                secendEulerBack = targetPosBack + new Vector3(0, 0, -1);//new Vector3(0f, 180f, 0f); //
                                StartCoroutine(MoveToPos(GameObject.Find("blueModel").transform, targetPosBack, 0.012f, firstEulerBack, secendEulerBack));
                                */
                                break;                                
                        }                        
                    }
                }
            }
            //AI模式满足在房间并且readyBeginAI变量为真，则实际设置种族，并让AI模式开始按钮显示出来
            if (PhotonNetwork.inRoom && readyBeginAI)
            {
                PhotonNetwork.player.SetTeam(readyBeginAITeam);
                AIBeginButton.gameObject.SetActive(true);
            }

        }
    }
}
