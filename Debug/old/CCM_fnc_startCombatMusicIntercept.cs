/* ----------------------------------------------------------------------------
Function: CCM_fnc_startCombatMusicIntercept

Description:
	Performs a loop that is designed to intercept combat tracks.

Parameters:
	NONE

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_fnc_startCombatMusicIntercept());
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
/*
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_scheduled : BaseUnityPlugin
    {
        internal static IEnumerator CCM_fnc_startCombatMusicIntercept()
        {
            CCM_core.CCM_fnc_logWithTime("Started combat music check");

            // create 2 lists for finding changes and comapring
            var _musicList = CCM_core.CCM_fnc_findMusicObjectsInScene();
            var _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();

            CCM_core.CCM_doRunCombatMusicCheck = true;
            while (CCM_core.CCM_doRunCombatMusicCheck)
            {
                CCM_core.CCM_fnc_logWithTime("Looping for combat music check");

                // update comparison list
                _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();
                CCM_core.CCM_logSource.Log(LogLevel.Message, _musicListCompare.Count);
                CCM_core.CCM_logSource.Log(LogLevel.Message, _musicList.Count);

                // check if any new (combat) music objects were created
                if (!CCM_core.CCM_fnc_areListsTheSame(_musicList, _musicListCompare))
                {
                    CCM_core.CCM_fnc_logWithTime("Found more music");
                    // replace _musicList with the new values in _musicListCompare
                    // copying list instead of referencing
                    _musicList = new List<GameObject>(_musicListCompare);
                    CCM_core.CCM_fnc_logWithTime("Adjusted music list");
                }

                // sleep 0.5
                yield return new WaitForSeconds(0.5f);
            }

            CCM_core.CCM_fnc_logWithTime("Ended combat music check");
        }
    }
}
*/