using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Aionline.WordsCities
{
    public class HeroObj : Photon.MonoBehaviour , IPunObservable
    {

        #region Public Var
        //国籍
        public WorldObj.Nationality nationality = WorldObj.Nationality.none;//{ set; get; }
        //单词
        public string word; //{ set; get; }
        //武将技
        public WorldObj.HeroSkill heroskill ; //{ set; get; }
        //武力值
        public int force; //{ set; get; }
        //智力值
        public int intelligence; //{ set; get; }
        //统治力
        public int dominance; //{ set; get; }
        //政治
        public int politics; //{ set; get; }
        //忠诚度
        public int allegiance; //{ set; get; }
        //魅力值
        public int charm; //{ set; get; }
        //招募价格
        public int price; //{ set; get; }

        #endregion

        #region Public Methods
        /// <summary>
        /// 分配国籍
        /// </summary>
        /// <param name="_nationality">国籍</param>
        public void GiveNationality(WorldObj.Nationality _nationality)
        {
            photonView.RPC("AssignNationality", PhotonTargets.All, _nationality);
        }
        [PunRPC]
        public void AssignNationality(WorldObj.Nationality _nationality)
        {
            nationality = _nationality;
        }

        /// <summary>
        /// 分配武将技
        /// </summary>
        public void GiveHeroSkill(WorldObj.HeroSkill _heroskill)
        {
            photonView.RPC("AssignHeroSkill", PhotonTargets.All, _heroskill);
        }
        [PunRPC]
        public void AssignHeroSkill(WorldObj.HeroSkill _heroskill)
        {
            heroskill = _heroskill;
        }
        #endregion

        /// <summary>
        /// 分配武力值
        /// </summary>
        /// <param name="_heroforce">武力值</param>
        public void GiveForce(int _heroforce)
        {
            photonView.RPC("GiveForceRPC", PhotonTargets.All, _heroforce);
        }
        [PunRPC]
        public void GiveForceRPC(int _heroforce)
        {
            force = _heroforce;
        }

        /// <summary>
        /// 分配智力值
        /// </summary>
        /// <param name="_herointelligence">智力值</param>
        public void GiveIntelligence(int _herointelligence)
        {
            photonView.RPC("GiveIntelligenceRPC", PhotonTargets.All, _herointelligence);
        }
        [PunRPC]
        public void GiveIntelligenceRPC(int _herointelligence)
        {
            intelligence = _herointelligence;
        }

        /// <summary>
        /// 分配统治力
        /// </summary>
        /// <param name="_herodominance">统治力</param>
        public void GiveDominance(int _herodominance)
        {
            photonView.RPC("GiveDominanceRPC", PhotonTargets.All, _herodominance);
        }
        [PunRPC]
        public void GiveDominanceRPC(int _herodominance)
        {
            dominance = _herodominance;
        }

        /// <summary>
        /// 分配政治值
        /// </summary>
        /// <param name="_heropolitics">政治值</param>
        public void GivePolitics(int _heropolitics)
        {
            photonView.RPC("GivePoliticsRPC", PhotonTargets.All, _heropolitics);
        }
        [PunRPC]
        public void GivePoliticsRPC(int _heropolitics)
        {
            politics = _heropolitics;
        }

        /// <summary>
        /// 分配忠诚度
        /// </summary>
        /// <param name="_heroallegiance">忠诚度</param>
        public void GiveAllegiance(int _heroallegiance)
        {
            photonView.RPC("GiveAllegianceRPC", PhotonTargets.All, _heroallegiance);
        }
        [PunRPC]
        public void GiveAllegianceRPC(int _heroallegiance)
        {
            allegiance = _heroallegiance;
        }

        /// <summary>
        /// 分配魅力值
        /// </summary>
        /// <param name="_herocharm">魅力值</param>
        public void GiveCharm(int _herocharm)
        {
            photonView.RPC("GiveCharmRPC", PhotonTargets.All, _herocharm);
        }
        [PunRPC]
        public void GiveCharmRPC(int _herocharm)
        {
            charm = _herocharm;
        }

        /// <summary>
        /// 分配招募价格
        /// </summary>
        /// <param name="_heroprice">招募价格</param>
        public void GivePrice(int _heroprice)
        {
            photonView.RPC("GivePriceRPC", PhotonTargets.All, _heroprice);
        }
        [PunRPC]
        public void GivePriceRPC(int _heroprice)
        {
            price = _heroprice;
        }



        /// <summary>
        /// 同步增加英雄到静态所有英雄对象列表
        /// </summary>
        public void AddToHeros()
        {
            photonView.RPC("AddToHerosRPC", PhotonTargets.All, null);
        }
        [PunRPC]
        void AddToHerosRPC()
        {
            WorldObj.allHeros.Add(this);
        }

        private void Start()
        {
            if (!photonView.isMine)
            {
                return;
            }
            AddToHeros();
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(nationality);
                stream.SendNext(word);
                stream.SendNext(heroskill);
                stream.SendNext(force);
                stream.SendNext(intelligence);
                stream.SendNext(dominance);
                stream.SendNext(politics);
                stream.SendNext(allegiance);
                stream.SendNext(charm);
                stream.SendNext(price);
            }
            else if (stream.isReading)
            {
                nationality = (WorldObj.Nationality)stream.ReceiveNext();
                word = (string)stream.ReceiveNext();
                heroskill = (WorldObj.HeroSkill)stream.ReceiveNext();
                force = (int)stream.ReceiveNext();
                intelligence = (int)stream.ReceiveNext();
                dominance = (int)stream.ReceiveNext();
                politics = (int)stream.ReceiveNext();
                allegiance = (int)stream.ReceiveNext();
                charm = (int)stream.ReceiveNext();
                price = (int)stream.ReceiveNext();
            }
        }

    }
}
