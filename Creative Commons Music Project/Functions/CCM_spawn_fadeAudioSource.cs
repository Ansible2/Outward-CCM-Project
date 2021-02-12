﻿/* ----------------------------------------------------------------------------
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
		StartCoroutine(CCM_fnc_fadeAudioSource(_someAudioSource,3,0,true));
        
        OR
        
        CCM_spawn_fadeAudioSource(_someAudioSource,3,0,true)
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Collections;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /* ----------------------------------------------------------------------------
            CCM_spawn_fadeAudioSource
        ---------------------------------------------------------------------------- */
        internal static void CCM_spawn_fadeAudioSource(AudioSource _audioSoucre, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false)
        {
            CCM_Instance.StartCoroutine(CCM_fnc_fadeAudioSource(_audioSoucre, _duration, _targetVolume, _stopAfter));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_fadeAudioSource
        ---------------------------------------------------------------------------- */
        internal static IEnumerator CCM_fnc_fadeAudioSource(AudioSource _audioSource, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false)
        {
            float currentTime = 0;
            float _startingVolume = _audioSource.volume;
            // wait until audiosource is done with fade
            while (_fn_isAudioSourceFading(_audioSource))
            {
                yield return new WaitForSeconds(0.1f);
            }

            CCM_fnc_setFadeStop(_audioSource, false);
            CCM_fnc_setFadeIsFading(_audioSource, true);

            while (currentTime < _duration || _fn_shouldFadeStop(_audioSource))
            {
                currentTime += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                yield return null;
            }

            if (_stopAfter)
            {
                _audioSource.Stop();
                //_audioSource.clip.UnloadAudioData();
            }

            CCM_fnc_setFadeStop(_audioSource, false);
            CCM_fnc_setFadeIsFading(_audioSource, false);

            yield break;
        }


        /* ----------------------------------------------------------------------------
            _fn_shouldFadeStop
        ---------------------------------------------------------------------------- */
        private static bool _fn_shouldFadeStop(AudioSource _audioSource)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudiSource_1)
            {
                return CCM_MusicHandlers.musicAudiSource_1_stopFading;
            }
            else
            {
                return CCM_MusicHandlers.musicAudiSource_2_stopFading;
            }
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_setFadeStop
        ---------------------------------------------------------------------------- */
        private static void CCM_fnc_setFadeStop(AudioSource _audioSource, bool _value)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudiSource_1)
            {
                CCM_MusicHandlers.musicAudiSource_1_stopFading = _value;
            }
            else
            {
                CCM_MusicHandlers.musicAudiSource_2_stopFading = _value;
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_isAudioSourceFading
        ---------------------------------------------------------------------------- */
        private static bool _fn_isAudioSourceFading(AudioSource _audioSource)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudiSource_1)
            {
                return CCM_MusicHandlers.musicAudiSource_1_isFading;
            }
            else
            {
                return CCM_MusicHandlers.musicAudiSource_2_isFading;
            }
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_setFadeIsFading
        ---------------------------------------------------------------------------- */
        private static void CCM_fnc_setFadeIsFading(AudioSource _audioSource, bool _value)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudiSource_1)
            {
                CCM_MusicHandlers.musicAudiSource_1_isFading = _value;
            }
            else
            {
                CCM_MusicHandlers.musicAudiSource_2_isFading = _value;
            }
        }
    }
}