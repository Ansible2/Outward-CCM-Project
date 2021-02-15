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
using UnityEngine;
using BepInEx.Logging;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal void CCM_event_onSceneChangeStarted(Scene _goingToScene, LoadSceneMode _mode)
        {
            if (CCM_fnc_isSceneReal(_goingToScene))
            {
                CCM_currentScene = _goingToScene;
                CCM_rpc.CCM_fnc_changeActiveScene_RPC(_goingToScene.name, PhotonNetwork.player.ID);
            }
            
            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: called for Scene - " + _goingToScene.name);
        }
    }
}