﻿/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
*/
using BepInEx.Logging;
using UnityEngine;

namespace creativeCommonsMusicProject
{   
    internal partial class CCM_rpc : Photon.MonoBehaviour
    {
        /* ------------------------------------------------------------------------
            Create Globals
        ------------------------------------------------------------------------ */
        internal static CCM_rpc CCM_rpcInstance;
        internal static PhotonView CCM_photonView;

        /* ------------------------------------------------------------------------
            awake function
        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            DontDestroyOnLoad(this);
            CCM_rpcInstance = this;
            CCM_core.CCM_fnc_logWithTime("logging this");
            CCM_core.CCM_logSource.Log(LogLevel.Message, this);
            
            //CCM_photonView = PhotonView.Get(this);
            CCM_photonView = this.gameObject.GetOrAddComponent<PhotonView>();
            CCM_photonView.viewID = 1067;

            CCM_core.CCM_fnc_logWithTime(CCM_photonView.ToString());
            CCM_core.CCM_fnc_logWithTime("hey");
            Debug.Log("Hey");
        }


        
        internal static void TestRPC()
        {
            CCM_core.CCM_fnc_logWithTime("Testing RPC");
            // this does execute, but it happens before we connect to photon
            CCM_photonView.RPC("doTest", PhotonTargets.All);
        }

        [PunRPC]
        void doTest()
        {
            CCM_core.CCM_fnc_logWithTime("Hey PUN");
        }

        internal static void testRPC_query()
        {
            TestRPC();
        }
    }
}






/*
    // simply used as a reference in other files to the photon view needed for RPCs
    internal class CCM_getPhotonView : Photon.MonoBehaviour
    {
        internal static PhotonView CCM_photonView;
        internal void Awake()
        {
            CCM_photonView = GetComponent<PhotonView>();
            CCM_core.CCM_logSource.LogMessage(CCM_photonView);
            CCM_core.CCM_fnc_logWithTime("CCM photon view ^");
        }
    }
    */

































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