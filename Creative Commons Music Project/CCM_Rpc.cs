using BepInEx.Logging;
using UnityEngine;

namespace creativeCommonsMusicProject
{   
    internal partial class CCM_rpc : Photon.MonoBehaviour
    {
        /* ------------------------------------------------------------------------
            Create Globals
        ------------------------------------------------------------------------ */
        internal static CCM_rpc CCM_rpcComponent;
        internal static PhotonView CCM_photonView;

        /* ------------------------------------------------------------------------
            awake function
        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            //DontDestroyOnLoad(this);
            CCM_rpcComponent = this;
            
            //CCM_photonView = PhotonView.Get(this);
            CCM_photonView = this.gameObject.GetOrAddComponent<PhotonView>();
            CCM_photonView.viewID = 61526;
        }
    }
}