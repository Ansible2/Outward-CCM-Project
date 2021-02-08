/* ----------------------------------------------------------------------------
Function: CCM_spawn_fadeAudioSource

Description:
	When a scened first gets music played

Parameters:
	0: _audioSoucre <AudioSource> - The AudioSource to fade
    1: _duration <FLOAT> - Over how long should the fade take place
    2: _targetVolume <FLOAT> - The volume you want the audio to be at at the end of the fade
    3: _stopAfter <BOOL> - Should the AudioSource be commanded to stop after the fade

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

namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static void CCM_spawn_startMusicRoutine()
        {
            CCM_Instance.StartCoroutine(CCM_fnc_startMusicRoutine());
        }

        internal static IEnumerator CCM_fnc_startMusicRoutine()
        {

        }
    }
}
