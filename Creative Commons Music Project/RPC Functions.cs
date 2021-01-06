using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Photon;
using Photon.Realtime;

namespace creativeCommonsMusicProject
{
    internal class CCM_rpc
    {
        CCM_core CCM_core = new CCM_core(); // how to get another class in a different file
        /* ------------------------------------------------------------------------
        
            CCM_fnc_changeActiveScene

        ------------------------------------------------------------------------ */
        [PunRPC]
        internal void CCM_fnc_changeActiveScene(string _sceneName,PhotonPlayer _player)
        {
            bool _playerInDictionary = CCM_core.CCM_dictionary_activePlayerScenes.ContainsKey(_player);
            
            if (_playerInDictionary)
            {
                CCM_core.CCM_dictionary_activePlayerScenes.Add(_player, _sceneName);
            }
            else
            {
                CCM_core.CCM_dictionary_activePlayerScenes[_player] = _sceneName;
            }
        }
    }


    // simply used as a reference in other files to the photon view needed for RPCs
    internal class CCM_getPhotonView : UnityEngine.MonoBehaviour
    {
        internal PhotonView CCM_photonView;
        void Start()
        {
            CCM_photonView = GetComponent<PhotonView>();
        }
    }
}

/*
    public class MyCustomRPC : Photon.MonoBehaviour
    {
        public void CallRemoteMethod()
        {

            if (PhotonNetwork.offlineMode == true)
            { //use this you need to support offline mode.
                //MyRemoteMethod(PhotonTargets.Others, new object[] { 42, true });
                return;
            }
            GetComponent<PhotonView>().RPC("MyRemoteMethod", PhotonTargets.Others, new object[] { 42, true });
            

               //Target Types
               //PhotonTargets.Others
               //PhotonTargets.All //triggered instantly
               //PhotonTargets.AllViaServer //local client gets even through server just like everyone else
               //PhotonTargets.MasterClient
               //PhotonNetwork.playerList[0]
               //PhotonTargets.AllBuffered
               //PhotonTargets.AllBufferedViaServer //used in item pickups where could be contested which client got it first
               //An important use is also when a new player connects later, they will recieve this 
               //buffered event that the item has been picked up and should be removed from scene
        }

        [PunRPC]
        void MyRemoteMethod(int someNumber, bool someBool)
        {
            Debug.Log(someNumber);
            Debug.Log(someBool);
        }
    }
*/