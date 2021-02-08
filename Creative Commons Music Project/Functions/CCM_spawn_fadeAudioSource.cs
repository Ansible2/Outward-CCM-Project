/* ----------------------------------------------------------------------------
Function: CCM_spawn_fadeAudioSource

Description:
	Fades an AudioSource to a given volume.

    It can also stop music at the end of the fades when desired.

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
using System.Collections;
using BepInEx;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static void CCM_spawn_fadeAudioSource(AudioSource _audioSoucre, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false)
        {
            CCM_Instance.StartCoroutine(CCM_fnc_fadeAudioSource(_audioSoucre, _duration, _targetVolume, _stopAfter));
        }

        internal static IEnumerator CCM_fnc_fadeAudioSource(AudioSource _audioSoucre, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false)
        {
            float currentTime = 0;
            float _startingVolume = _audioSoucre.volume;

            while (currentTime < _duration)
            {
                currentTime += Time.deltaTime;
                _audioSoucre.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                yield return null;
            }

            if (_stopAfter)
            {
                _audioSoucre.Stop();
            }
            yield break;
        }
    }
}