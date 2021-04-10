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
        internal static void CCM_spawn_fadeAudioSource(AudioSource _audioSoucre, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false, bool _stopNowPlaying = false)
        {
            CCM_Instance.StartCoroutine(CCM_fnc_fadeAudioSource(_audioSoucre, _duration, _targetVolume, _stopAfter, _stopNowPlaying));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_fadeAudioSource
        ---------------------------------------------------------------------------- */
        internal static IEnumerator CCM_fnc_fadeAudioSource(AudioSource _audioSource, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false, bool _stopNowPlaying = false)
        {
            CCM_fnc_logWithTime("CCM_fnc_fadeAudioSource: Fading " + _audioSource + " to " + _targetVolume);
            float currentTime = 0;
            float _startingVolume = _audioSource.volume;
            // wait until audiosource is done with fade
            while (_fn_isAudioSourceFading(_audioSource))
            {
                CCM_fnc_logWithTime("CCM_fnc_fadeAudioSource: Waiting for audioSource: " + _audioSource + " to stop fading...");
                yield return new WaitForSeconds(0.1f);
            }

            CCM_fnc_setFadeIsFading(_audioSource, true);
            CCM_fnc_setFadeStop(_audioSource, false);
            

            CCM_fnc_logWithTime("CCM_fnc_fadeAudioSource: Now fading audioSource: " + _audioSource);         


            while (currentTime < _duration || _fn_shouldFadeStop(_audioSource))
            {
                currentTime += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                yield return null;
            }

            if (_stopAfter)
            {
                _audioSource.Stop();
                _audioSource.clip.UnloadAudioData();
                Destroy(_audioSource.clip);
                CCM_fnc_logWithTime("CCM_fnc_fadeAudioSource: Stopped audioSource: " + _audioSource);
            }
          

            if (_stopNowPlaying)
            {
                CCM_MusicHandlers.nowPlayingAudioSource = null; // could get removed from memory?
                CCM_MusicHandlers.nowPlayingMusicHandler = null;
                CCM_fnc_logWithTime("CCM_fnc_fadeAudioSource: Set now playing vars to null");
            }


            CCM_fnc_setFadeIsFading(_audioSource, false);
            CCM_fnc_setFadeStop(_audioSource, false);

            yield break;
        }


        /* ----------------------------------------------------------------------------
            _fn_shouldFadeStop
        ---------------------------------------------------------------------------- */
        private static bool _fn_shouldFadeStop(AudioSource _audioSource)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudioSource_1)
            {
                return CCM_MusicHandlers.musicAudioSource_1_stopFading;
            }
            else
            {
                return CCM_MusicHandlers.musicAudioSource_2_stopFading;
            }
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_setFadeStop
        ---------------------------------------------------------------------------- */
        private static void CCM_fnc_setFadeStop(AudioSource _audioSource, bool _value)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudioSource_1)
            {
                CCM_MusicHandlers.musicAudioSource_1_stopFading = _value;
            }
            else
            {
                CCM_MusicHandlers.musicAudioSource_2_stopFading = _value;
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_isAudioSourceFading
        ---------------------------------------------------------------------------- */
        private static bool _fn_isAudioSourceFading(AudioSource _audioSource)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudioSource_1)
            {
                return CCM_MusicHandlers.musicAudioSource_1_isFading;
            }
            else
            {
                return CCM_MusicHandlers.musicAudioSource_2_isFading;
            }
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_setFadeIsFading
        ---------------------------------------------------------------------------- */
        private static void CCM_fnc_setFadeIsFading(AudioSource _audioSource, bool _value)
        {
            if (_audioSource == CCM_MusicHandlers.musicAudioSource_1)
            {
                CCM_MusicHandlers.musicAudioSource_1_isFading = _value;
            }
            else
            {
                CCM_MusicHandlers.musicAudioSource_2_isFading = _value;
            }
        }
    }
}