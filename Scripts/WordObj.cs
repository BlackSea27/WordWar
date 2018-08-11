using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class WordObj : Photon.MonoBehaviour //, IPunObservable
    {
        #region Public Var
        //单词
        public string word;// { set; get; }
        //读音
        public string soundmark;// { set; get; }
        //中文释义
        public string chinese;// { set; get; }
        //例句
        public string sentence;// { set; get; }
        //图片
        public Sprite sprite;// { set; get; }
        //语音
        public AudioClip pronunciation; // { set; get; }

        #endregion


        #region Private Var

        #endregion

        #region Private Methods
        /*
        /// <summary>
        /// 同步增加单词到静态所有单词对象列表
        /// </summary>
        public void AddToWords()
        {
            photonView.RPC("AddToWordsRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        void AddToWordsRPC()
        {
            WorldObj.allWords.Add(this);
        }
        */

        private void Start()
        {
            if (!photonView.isMine)
            {
                return;
            }
            //AddToWords();
        }
        #endregion



        /*
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(word);
                stream.SendNext(pronunciation);
                stream.SendNext(chinese);
                stream.SendNext(sentence);
                stream.SendNext(sprite);
            }
            else
            {
                word = (string)stream.ReceiveNext();
                soundmark = (string)stream.ReceiveNext();
                chinese = (string)stream.ReceiveNext();
                sentence = (string)stream.ReceiveNext();
                sprite = (Sprite)stream.ReceiveNext();
            }
        }
        */
    }
}
