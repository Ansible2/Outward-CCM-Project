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
using BepInEx.Logging;
using UnityEngine.SceneManagement;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal void CCM_event_onSceneChangeStarted(Scene _goingToScene, LoadSceneMode _mode)
        {
            CCM_rpc.CCM_fnc_changeActiveScene_RPC(_goingToScene.name, PhotonNetwork.player.ID);
            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: called for Scene - " + _goingToScene.name);

            //StartCoroutine(CCM_scheduled.CCM_fnc_waitForLoadingDone(_myScene));
        }
    }
}