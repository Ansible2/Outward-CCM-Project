/* ----------------------------------------------------------------------------
Function: CCM_fnc_waitForLoadingDone

Description:
	

Parameters:
	0: _scene <Scene> - The scene to be loaded

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_fnc_waitForLoadingDone());
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System;
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
    internal partial class CCM_scheduled : BaseUnityPlugin
    {
        internal static IEnumerator CCM_fnc_waitForLoadingDone(Scene _scene)
        {
            string _mySceneName = _scene.name;

            // some scenes such as the main menu and loading scenes should not be touched
            // these are "fake" scenes
            if (CCM_core.CCM_fnc_isSceneReal(_scene))
            {
                CCM_core.CCM_fnc_logWithTime("Found real scene: " + _mySceneName);

                // wait for all scene loading to finish
                CCM_core.CCM_fnc_logWithTime("Waiting for loading...");
                yield return new WaitUntil(() => NetworkLevelLoader.Instance.IsOverallLoadingDone);
            /*
                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for loading...");
                    // sleep 0.1 second
                    yield return new WaitForSeconds(0.1f);
                }
            */
                CCM_core.CCM_fnc_logWithTime("Loading done...");

                // tell every machine that is connected about what scene the player is on
                CCM_core.CCM_fnc_logWithTime("Telling server to update all on players current scene...");

            /*
                CCM_getPhotonView.CCM_photonView.RPC(
                    "CCM_fnc_changeActiveScene",
                    PhotonTargets.AllViaServer,
                    new object[] { _scene.name, PhotonNetwork.player }
                );    
            */

                // start music replace music
                CCM_core.CCM_fnc_logWithTime("Finding main music object to change in scene");
                GameObject _mainMusicObject = CCM_core.CCM_fnc_findMainMusicObject(_scene);
                string _mainMusicObjectName = _mainMusicObject.name;

                var _trackType = CCM_core.CCM_fnc_getTrackType(_mainMusicObjectName);
                List<string> _possibleTracks_list = CCM_core.CCM_fnc_getAllAvailableReplacementTracks(_trackType);
                var _trackFileName = CCM_core.CCM_fnc_grabRandomTrack(_possibleTracks_list);

                CCM_core.CCM_fnc_logWithTime("Load audio call");
                CCM_core.CCM_Instance.StartCoroutine(CCM_fnc_loadAndPlayAudioClip(_trackFileName, CCM_core.CCM_currentTrackType));

                // wait for audio to load
                CCM_core.CCM_fnc_logWithTime("waiting for audio to load...");
                yield return new WaitUntil(() => CCM_core.CCM_loadingAudio);
                CCM_core.CCM_fnc_logWithTime("Audio play & load pass");

                


                /*
                    // wait until combat music check if off
                    while (CCM_core.CCM_doRunCombatMusicCheck)
                    {
                        CCM_core.CCM_fnc_logWithTime("waiting for combat music check reset...");
                        yield return new WaitForSeconds(0.1f);
                    }


                    CCM_core.CCM_fnc_logWithTime("Reached combat music check");
                    // start combat music check loop
                    monoRef.StartCoroutine(CCM_fnc_startCombatMusicIntercept());
                */
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("Skipped fake Scene: " + _mySceneName);
            }
        }
    }
}