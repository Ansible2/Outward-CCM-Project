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
        /// <summary>
        /// Creates a coroutine for fading an audiosource to a given volume
        /// </summary>
        /// <param name="_audioSoucre"></param>
        /// <param name="_duration"></param>
        /// <param name="_targetVolume"></param>
        /// <param name="_stopAfter"></param>
        /// <param name="_stopNowPlaying"></param>
        internal static void CCM_spawn_fadeAudioSource(AudioSource _audioSoucre, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false, bool _stopNowPlaying = false)
        {
            CCM_Instance.StartCoroutine(CCM_fnc_fadeAudioSource(_audioSoucre, _duration, _targetVolume, _stopAfter, _stopNowPlaying));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_fadeAudioSource
        ---------------------------------------------------------------------------- */
        /// <summary>
        /// Fades a given audioSource to a given volume
        /// </summary>
        /// <param name="_audioSource"></param>
        /// <param name="_duration"></param>
        /// <param name="_targetVolume"></param>
        /// <param name="_stopAfter"></param>
        /// <param name="_stopNowPlaying"></param>
        /// <returns>IEnumerator</returns>
        internal static IEnumerator CCM_fnc_fadeAudioSource(AudioSource _audioSource, float _duration = 3, float _targetVolume = 0.5f, bool _stopAfter = false, bool _stopNowPlaying = false)
        {
            CCM_fnc_log.withTime.message("CCM_fnc_fadeAudioSource: Was called to fade " + _audioSource + " to " + _targetVolume);
            
            
            float currentTime = 0;
            float _startingVolume = _audioSource.volume;
            // wait until audiosource is done with fade
            if (_fn_isAudioSourceFading(_audioSource))
            {
                CCM_fnc_setFadeStop(_audioSource, true);

                int _waitCycle = 0;
                while (_fn_isAudioSourceFading(_audioSource))
                {

                    _waitCycle = _waitCycle + 1;
                    if (_waitCycle % 100 == 0)
                    {
                        CCM_fnc_log.message("CCM_fnc_fadeAudioSource: Waiting for audioSource: " + _audioSource + " to stop fading...");
                    }
                    yield return new WaitForSecondsRealtime(0.01f);
                }
            }
            else
            {
                CCM_fnc_setFadeIsFading(_audioSource, true);
            }

            CCM_fnc_setFadeStop(_audioSource, false);


            CCM_fnc_log.withTime.message("CCM_fnc_fadeAudioSource: Now fading audioSource: " + _audioSource);         


            while (currentTime < _duration && (_audioSource.volume != _targetVolume) && (!_fn_shouldFadeStop(_audioSource)))
            {
                //currentTime += Time.deltaTime;
                currentTime += Time.unscaledDeltaTime;
                _audioSource.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                yield return null;
            }

            if (_stopAfter)
            {
                _audioSource.Stop();
                _audioSource.clip.UnloadAudioData();
                Destroy(_audioSource.clip);
                CCM_fnc_log.withTime.message("CCM_fnc_fadeAudioSource: Stopped audioSource: " + _audioSource);
            }
          

            if (_stopNowPlaying)
            {
                CCM_MusicHandlers.nowPlayingAudioSource = null; // could get removed from memory?
                CCM_MusicHandlers.nowPlayingMusicHandler = null;
                CCM_fnc_log.message("CCM_fnc_fadeAudioSource: Set now playing vars to null");
            }


            CCM_fnc_setFadeIsFading(_audioSource, false);
            //CCM_fnc_setFadeStop(_audioSource, false);

            CCM_fnc_log.message("CCM_fnc_fadeAudioSource: AudioSource: " + _audioSource.name + " has finished its fade to " + _targetVolume);

            yield break;
        }


        /* ----------------------------------------------------------------------------
            _fn_shouldFadeStop
        ---------------------------------------------------------------------------- */
        /// <summary>
        /// Used for determining if a given AudioSource should stop its current fade
        /// </summary>
        /// <param name="_audioSource"></param>
        /// <returns>bool</returns>
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
        /// <summary>
        /// Sets a corresponding bool for an AudioSource to stop fading
        /// </summary>
        /// <param name="_audioSource"></param>
        /// <param name="_value"></param>
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
        /// <summary>
        /// Checks a corresponding AudioSource's bool to see if it is currently fading
        /// </summary>
        /// <param name="_audioSource"></param>
        /// <returns>bool</returns>
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
        /// <summary>
        /// Sets the corresponding bool of whether an AudioSource is fading
        /// </summary>
        /// <param name="_audioSource"></param>
        /// <param name="_value"></param>
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