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


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        [HarmonyPatch(typeof(GlobalAudioManager), "PlayMusic")]
        class CCM_playMusicPatch
        {
            /* ----------------------------------------------------------------------------
                CCM_event_onVanillaMusicPlayed
            ---------------------------------------------------------------------------- */
            [HarmonyPostfix]
            static void CCM_event_onVanillaMusicPlayed(ref AudioSource __result)
            {
                var _clipName = __result.clip.name.ToLower();
                CCM_fnc_log.WithTime.message("CCM_event_onVanillaMusicPlayed: was called for vanilla clip: " + _clipName);
                
                // Events are unqiue
                if (!_clipName.Contains("event"))
                {
                    CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Clip " + _clipName + " is not event music.");
                    CCM_trackTypes_enum _trackType = CCM_fnc_getTrackType(_clipName);

                    if (_trackType != CCM_trackTypes_enum.EMPTY)
                    {
                        __result.mute = true;

                        if (_fn_changeCurrentTrackType(_trackType))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }
                        
                    }
                    else
                    {
                        CCM_fnc_log.WithTime.error("CCM_event_onVanillaMusicPlayed: Unhandled piece of music has empty track type: " + _clipName);
                    }

                    if (!CCM_createdMusicHandlers)
                    {

                        CCM_fnc_assignMusicHandlerProperties(__result.gameObject);
                    }
                }
                else
                {
                    CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Clip " + _clipName + " IS event music. Handling...");

                    __result.mute = true;

                    if (_clipName.Contains("danger"))
                    {
                        CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Danger event music requested.");
                        if (_fn_changeCurrentTrackType(CCM_trackTypes_enum.combat))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }
                    }
                    else if (_clipName.Contains("dramatic"))
                    {
                        CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Dramatic event music requested.");
                        if (_fn_changeCurrentTrackType(CCM_trackTypes_enum.combat))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }
                    }
                    else if (_clipName.Contains("friendly"))
                    {
                        CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Friendly event music requested.");
                        if (_fn_changeCurrentTrackType(CCM_trackTypes_enum.ambientDay))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }

                    }
                    else if (_clipName.Contains("quest"))
                    {
                        CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Quest event music requested.");
                        if (_fn_changeCurrentTrackType(CCM_trackTypes_enum.ambientDay))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }
                    }
                    else if (_clipName.Contains("mystery"))
                    {
                        CCM_fnc_log.message("CCM_event_onVanillaMusicPlayed: Mystery event music requested.");
                        if (_fn_changeCurrentTrackType(CCM_trackTypes_enum.dungeon))
                        {
                            _fn_requestTrack(CCM_currentTrackType);
                        }
                    }
                    else
                    {
                        CCM_fnc_log.error("CCM_event_onVanillaMusicPlayed: Event music is unknown! Won't handle...");
                    }

                }
                
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_changeCurrentTrackType
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Adjusts CCM_currentTrackType and returns a bool depending on whether it was changed or not
        ///</summary>
        private static bool _fn_changeCurrentTrackType(CCM_trackTypes_enum _trackType)
        {
            if (_trackType != CCM_currentTrackType)
            {
                CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_changeCurrentTrackType: CCM_currentTrackType set to: " + _trackType);
                CCM_currentTrackType = _trackType;
                return true;
            }
            else
            {
                CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_changeCurrentTrackType: CCM_currentTrackType is already set to: " + _trackType);
                return false;
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_requestTrack
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Requests a new music routine for a given CCM_trackTypes_enum (intiates a new music routine) 
        ///</summary>
        private static void _fn_requestTrack(CCM_trackTypes_enum _trackType)
        {
            CCM_fnc_log.WithTime.message("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: was called...");

            if (CCM_syncOnline)
            {
                CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: CCM_syncOnline is ON");

                if (PhotonNetwork.isMasterClient)
                {
                    CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: Local Machine IS Master Client. Starting routine for track type " + _trackType);
                    // start new routine
                    CCM_spawn_startMusicRoutine(_trackType);
                }
                else
                {
                    CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: Local Machine is NOT Master Client. Nothing will be done...");
                }
            }
            else
            {
                CCM_fnc_log.info("CCM_event_onVanillaMusicPlayed: _fn_requestTrack: CCM_syncOnline is OFF. Starting routine for track type " + _trackType);
                // start new routine
                CCM_spawn_startMusicRoutine(_trackType);
            }
        }
    }
}