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
        internal static void CCM_fnc_requestTrackToPlay_RPC(CCM_core.CCM_trackTypes_enum _trackType, int _playerId, string _playersScene)
        {
            if (CCM_core.CCM_syncOnline)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay_RPC: Sync Online is true. RPCing CCM_fnc_requestTrackToPlay.");
                CCM_photonView.RPC(
                    "CCM_fnc_requestTrackToPlay",
                    PhotonTargets.MasterClient,
                    new object[] { _trackType, _playerId, _playersScene }
                );
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay_RPC: Sync Online is false. Directly execing CCM_fnc_requestTrackToPlay.");
                CCM_rpcComponent.CCM_fnc_requestTrackToPlay(_trackType, _playerId, _playersScene);
            }  
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay(CCM_core.CCM_trackTypes_enum _trackType, int _playerId, string _playersScene)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: was called...");
            bool _startNewRoutine = false;
            bool _rpcDirectToPlayer = false;

            // Check if track type changed
            if (CCM_core.CCM_Dictionaries.activeScenesTrackType.ContainsKey(_playersScene))
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Scene: " + _playersScene + " was found in active scenes dictionary");
                if (CCM_core.CCM_Dictionaries.activeScenesTrackType[_playersScene] != _trackType)
                {
                    // start a new routine for track type changes
                    CCM_core.CCM_Dictionaries.activeScenesTrackType[_playersScene] = _trackType;
                    _startNewRoutine = true;
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: track type was changed for " + _playersScene + "... starting new routine.");
                }
                else
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Scene: " + _playersScene + " does not need a new routine started.");
                }
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Scene: " + _playersScene + " was not found in active scenes dictionary... Starting new Routine.");
                // if scene was not active we need to start a new routine
                CCM_core.CCM_Dictionaries.activeScenesTrackType.Add(_playersScene, _trackType);
                _startNewRoutine = true;
            }

            // this still needs to be able to return tracks that are already in active scenes for other players
            bool _sceneHasCurrentMusic = CCM_core.CCM_Dictionaries.activeScenesCurrentTrack.ContainsKey(_playersScene);
            bool _sceneMusicIsBeingChosen = CCM_core.CCM_Lists.scenesChoosingMusicFor.Contains(_playersScene);
            if (_startNewRoutine)
            {
                CCM_core.CCM_spawn_startMusicRoutineForScene(_playersScene, _trackType);
            }
            else if (_sceneHasCurrentMusic && !_startNewRoutine && !_sceneMusicIsBeingChosen)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Player ID " + _playerId + " met standards for RPC direct");
                _rpcDirectToPlayer = true;
            }



            if (_rpcDirectToPlayer)
            {
                CCM_core.CCM_track _track = CCM_core.CCM_Dictionaries.activeScenesCurrentTrack[_playersScene];
                string _sceneTrackFileName = _track.Filename;
                CCM_photonView.RPC(
                    "CCM_event_playMusic_RPC",
                    PhotonPlayer.Find(_playerId),
                    new object[] { _sceneTrackFileName, _track.FolderType, _playersScene, true }
                );
            } 
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_requestTrackToPlay: Player ID " + _playerId + " did not meet standards for RPC direct");
            }
                
        }
    }
}