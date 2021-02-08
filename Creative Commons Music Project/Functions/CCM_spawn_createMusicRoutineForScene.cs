/* ----------------------------------------------------------------------------
Function: CCM_spawn_createMusicRoutineForScene

Description:
	When a scened first gets music played, a music "routine" is started.
    
    It will handle when music is supposed to played at random times.

Parameters:
	0: _audioSoucre <AudioSource> - The AudioSource to fade

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_spawn_fadeAudioSource(_someAudioSource,3,0,true));
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
        internal static void CCM_spawn_createMusicRoutineForScene()
        {
            CCM_Instance.StartCoroutine(CCM_fnc_startMusicRoutine());
        }

        internal static IEnumerator CCM_fnc_createMusicRoutineForScene()
        {

        }
    }
}
