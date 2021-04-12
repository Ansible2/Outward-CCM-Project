/* ----------------------------------------------------------------------------
Function: CCM_fnc_requestTrackToPlay

Description:
	Asks the masterClient for what track should be played for a given scene and type.

Parameters:
	0: _trackType <CCM_core.CCM_trackTypes_enum> - The type of track requested.
    1: _playerId <INT> - The player's PhotonNetwork ID.
    2: _playerScene <STRING> - The scene the Player's requesting music for.

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_rpc.CCM_fnc_requestTrackToPlay_RPC(_trackType, PhotonNetwork.player.ID, CCM_currentScene.name);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */


namespace creativeCommonsMusicProject
{

    partial class CCM_rpc
    {
        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay_RPC
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_requestTrackToPlay_RPC(int _playerId)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay_RPC: Was called...");

            if (CCM_core.CCM_syncOnline)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay_RPC: CCM_syncOnline is ON. RPCing CCM_fnc_requestTrackToPlay to Master Client");

                CCM_photonView.RPC(
                    "CCM_fnc_requestTrackToPlay",
                    PhotonTargets.MasterClient,
                    new object[] {_playerId}
                );
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay_RPC: CCM_syncOnline is OFF. Directly execing CCM_fnc_requestTrackToPlay.");
                CCM_rpcComponent.CCM_fnc_requestTrackToPlay(_playerId);
            }  
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay(int _playerId)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Was called...");

            CCM_photonView.RPC(
                "CCM_event_playMusic_RPC",
                PhotonPlayer.Find(_playerId),
                new object[] { CCM_core.CCM_currentTrack.Filename, CCM_core.CCM_currentTrack.FolderType}
            );

        }
    }
}