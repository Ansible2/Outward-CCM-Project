﻿/* ----------------------------------------------------------------------------
Function: CCM_event_onVanillaMusicPlayed

Description:
	A PostFix function to the private method GlobalAudioManager.PlayMusic inside the
     Assembly-CSharp.dll.
    
    It is used to intercept when Outward wants to play music.

Parameters:
	0: __result <AudioSource> - The audio ssource returned from the GlobalAudioManager.PlayMusic function

Returns:
	NOTHING 

Examples:
    (begin example)
		
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;
using HarmonyLib;
using System.Collections;



namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        // This is used to keep track of when the server sends back an RPC of what track to play
        internal static string CCM_trackToPlay = "";

        [HarmonyPatch(typeof(GlobalAudioManager), "PlayMusic")]
        class CCM_playMusicPatch
        {
            [HarmonyPostfix]
            static void CCM_event_onVanillaMusicPlayed(ref AudioSource __result)
            {
                int _trackType = CCM_fnc_getTrackType(__result.clip.name);

                if (_trackType != -1)
                {
                    __result.mute = true;

                    CCM_currentTrackType = _trackType;
                    CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: CCM_currentTrackType was set to " + _trackType);

                    _fn_requestTrackFromMaster(_trackType);
                }

                if (!CCM_MusicHandlers.handlersInstantiated)
                {
                   
                    CCM_fnc_assignMusicHandlerProperties(__result.gameObject);
                }
            }
        }


        private static void _fn_requestTrackFromMaster(int _trackType)
        {
            CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrackFromMaster: was called...");

            CCM_rpc.CCM_fnc_requestTrackToPlay_RPC(_trackType, PhotonNetwork.player.ID, CCM_currentScene.name);
            /*
            bool _trackIsCombat = _trackType == (int)CCM_trackTypes_enum.combat;
            if (_trackIsCombat)
            {
                CCM_fnc_logWithTime("CCM_spawn_playMusic: _trackType is combat track");
            }

            // wait to get request back from server
            while (CCM_trackToPlay == "")
            {
                yield return new WaitForSeconds(0.1f);
            }
            //string _trackFilename = CCM_trackToPlay;
            //CCM_trackToPlay = "";

            //CCM_rpc.CCM_rpcComponent.CCM_spawn_loadAndPlayAudioClip(_trackFilename, _trackType, _trackIsCombat);
            */
        }
    }
}