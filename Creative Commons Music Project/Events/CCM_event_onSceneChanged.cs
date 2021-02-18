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
        internal void CCM_event_onSceneChangeStarted(Scene _goingToScene, LoadSceneMode _mode)
        {
            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: called for Scene - " + _goingToScene.name);
            
            // sometimes this event can be called twice in a row, in order to avoid a double call of follow on functions, this is here
            if (CCM_currentScene != _goingToScene)
            {
                if (CCM_fnc_isSceneReal(_goingToScene))
                {
                    CCM_currentScene = _goingToScene;
                    CCM_rpc.CCM_fnc_changeActiveScene_RPC(_goingToScene.name, PhotonNetwork.player.ID);
                }
                else if (_goingToScene.name.ToLower().Contains("mainmenu") && CCM_MusicHandlers.nowPlayingAudioSource != null) // stop if going back to main menu
                {
                    CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Found that going to main menu scene and music is playing. Will fade out...");
                    CCM_spawn_fadeAudioSource(CCM_MusicHandlers.nowPlayingAudioSource, 3, 0, true, true);
                }
            }
            
        }
    }
}