/* ----------------------------------------------------------------------------
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


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        [HarmonyPatch(typeof(GlobalAudioManager), "PlayMusic")]
        class CCM_playMusicPatch
        {
            [HarmonyPostfix]
            static void CCM_event_onVanillaMusicPlayed(ref AudioSource __result)
            {
                CCM_trackTypes_enum _trackType = CCM_fnc_getTrackType(__result.clip.name);

                if (_trackType != CCM_trackTypes_enum.EMPTY)
                {
                    __result.mute = true;

                    CCM_currentTrackType = _trackType;
                    CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: CCM_currentTrackType was set to " + _trackType);

                    _fn_requestTrack(_trackType);
                }

                if (!CCM_MusicHandlers.handlersInstantiated)
                {
                   
                    CCM_fnc_assignMusicHandlerProperties(__result.gameObject);
                }
            }
        }


        private static void _fn_requestTrack(CCM_trackTypes_enum _trackType)
        {
            CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: was called...");

            if (CCM_syncOnline)
            {
                CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: CCM_syncOnline is ON");

                if (PhotonNetwork.isMasterClient)
                {
                    CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: Local Machine IS Master Client. Starting routine for track type " + _trackType);
                    // start new routine
                    CCM_spawn_startMusicRoutine(_trackType);
                }
                else
                {
                    CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: Local Machine is NOT Master Client. Will nothing will be done...");
                }
            }
            else
            {
                CCM_fnc_logWithTime("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: CCM_syncOnline is OFF. Starting routine for track type " + _trackType);
                // start new routine
                CCM_spawn_startMusicRoutine(_trackType);
            }
        }
    }
}