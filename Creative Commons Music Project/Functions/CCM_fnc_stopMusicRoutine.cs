/* ----------------------------------------------------------------------------
Function: CCM_spawn_startMusicRoutineForScene

Description:
	When a scened first gets music played, a music "routine" is started.
    
    It will handle when music is supposed to played at random times.

Parameters:
	0: _sceneName <STRING> - The name of the scene this is to be play music for
    1: _trackType <int> - The corresponding int track type as in the CCM_trackTypes_enum
    2: _deletePrevious <BOOL> - If a previous routine exists for the scene, 
        should it be deleted before creating the new one

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_spawn_startMusicRoutineForScene("a scene name", (int)CCM_trackTypes_enum.combat, true);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;








/*
    create a gameobject to attach this routine to.
    Add these gameObjects to a list so that each is known to the server.
    Change the name of the gameobject to be the scene name + someting like "routine"
    When you need to reference a certain scenes routine, just look it up with that string.
    To stop a routine, destroy the gameObject associated to the scene.
*/





namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static void CCM_fnc_stopMusicRoutine(string _sceneName)
        {
            if (CCM_Dictionaries.sceneRoutineObjects.ContainsKey(_sceneName))
            {
                GameObject _routineObject = CCM_Dictionaries.sceneRoutineObjects[_sceneName];

                CCM_Dictionaries.sceneRoutineObjects.Remove(_sceneName);
                Destroy(_routineObject);
            }
        }
    }
}