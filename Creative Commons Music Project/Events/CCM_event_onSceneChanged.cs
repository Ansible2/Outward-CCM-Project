/* ----------------------------------------------------------------------------
Function: CCM_event_onSceneChanged

Description:
	An alias for SceneManager.sceneLoaded.
    This fires when a new scene BEGINS loading (not when it necessarily finished).

Parameters:
	0: _goingToScene <Scene> - The scene to load
    1: _mode <LoadSceneMode> - 

Returns:
	NOTHING 

Examples:
    (begin example)
		SceneManager.sceneLoaded += CCM_event_onSceneChangeStarted;
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine.SceneManagement;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        ///<summary>
        /// An event that fires when a scene change begins and initiates the replacement of the current music routine
        ///</summary>
        internal void CCM_event_onSceneChangeStarted(Scene _goingToScene, LoadSceneMode _mode)
        {
            CCM_fnc_log.withTime.message("CCM_event_onSceneChangeStarted: Called for Scene - " + _goingToScene.name);
           
            if (CCM_fnc_isSceneReal(_goingToScene))
            {
                CCM_fnc_log.message("CCM_event_onSceneChangeStarted: Scene is real");
                CCM_currentSceneName = _goingToScene.name.ToLower();

                bool _isMasterClient = PhotonNetwork.isMasterClient;
                bool _routineIsRunning = CCM_currentRoutine != null;
                
                if (!_isMasterClient)
                {
                    CCM_fnc_log.info("CCM_event_onSceneChangeStarted: Player is NOT Master Client");
                    
                    if (CCM_syncOnline)
                    {
                        CCM_fnc_log.info("CCM_event_onSceneChangeStarted: CCM_syncOnline is ON");

                        if (_routineIsRunning)
                        {
                            CCM_fnc_log.withTime.warning("CCM_event_onSceneChangeStarted: Client has a routine running. Stopping routine and requesting track directly from Master Client...");
                            CCM_fnc_stopMusicRoutine();
                            CCM_rpc.CCM_rpcComponent.CCM_fnc_requestTrackToPlay(PhotonNetwork.player.ID);
                        }
                    }

                }

            }
            else if (_goingToScene.name.ToLower().Contains("mainmenu") && CCM_nowPlayingMusicHandler != null) // stop if going back to main menu
            {
                CCM_fnc_log.withTime.message("CCM_event_onSceneChangeStarted: Found that going to main menu scene and CCM music is playing. Will fade out...");
                CCM_spawn_fadeMusichandler(CCM_nowPlayingMusicHandler, 3, 0, false);
            }
            else
            {
                CCM_fnc_log.message("CCM_event_onSceneChangeStarted: Scene is NOT real.");
            }
        }
    }
}