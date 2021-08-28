/* ----------------------------------------------------------------------------
Function: CCM_spawn_fadeMusichandler

Description:
	Fades an AudioSource to a given volume.

    It can also stop music at the end of the fades when desired.

Parameters:
	0: _musicHandler <CCM_MusicHandler> - The Music handler which has an audiosource to fade
    1: _duration <FLOAT> - Over how long should the fade take place
    2: _targetVolume <FLOAT> - The volume you want the audio to be at at the end of the fade
    3: _musicIsStarting <BOOL> - Is music starting (true) or stopping (false)?

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_fnc_fadeMusichandler(_someAudioSource,3,0,true));
        
        OR
        
        CCM_spawn_fadeMusichandler(_someAudioSource,3,0,true)
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
            CCM_spawn_fadeMusichandler
        ---------------------------------------------------------------------------- */
        /// <summary>
        /// Creates a coroutine for fading an audiosource to a given volume
        /// </summary>
        /// <param name="_musicHandler"></param>
        /// <param name="_duration"></param>
        /// <param name="_targetVolume"></param>
        /// <param name="_musicIsStarting"></param>
        internal static void CCM_spawn_fadeMusichandler(CCM_MusicHandler _musicHandler, float _duration = 3, float _targetVolume = 0.5f, bool _musicIsStarting = false)
        {
            CCM_Instance.StartCoroutine(CCM_fnc_fadeMusichandler(_musicHandler, _duration, _targetVolume, _musicIsStarting));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_fadeMusichandler
        ---------------------------------------------------------------------------- */
        /// <summary>
        /// Fades a given audioSource to a given volume
        /// </summary>
        /// <param name="_musicHandler"></param>
        /// <param name="_duration"></param>
        /// <param name="_targetVolume"></param>
        /// <param name="_musicIsStarting"></param>
        /// <returns>IEnumerator</returns>
        internal static IEnumerator CCM_fnc_fadeMusichandler(CCM_MusicHandler _musicHandler, float _duration = 3, float _targetVolume = 0.5f, bool _musicIsStarting = false)
        {
            var _fadeId = _musicHandler.fadeCount += 1;
            AudioSource _audioSource = _musicHandler.audioSource;
            CCM_fnc_log.withTime.message("CCM_fnc_fadeMusichandler: Was called to fade " + _audioSource + " to " + _targetVolume + " with a fade ID of " + _fadeId);

            
            // wait until audiosource is done with a previous fade
            if (_musicHandler.isFading)
            {
                //_musicHandler.stopFading = true;

                int _waitCycle = 0;
                while (_musicHandler.isFading)
                {

                    _waitCycle = _waitCycle + 1;
                    if (_waitCycle % 100 == 0)
                    {
                        CCM_fnc_log.message("CCM_fnc_fadeMusichandler: Waiting for audioSource: " + _audioSource + " to stop fading...");
                    }
                    yield return new WaitForSecondsRealtime(0.01f);
                }
            }
            else
            {
                _musicHandler.isFading = true;
            }


            // make sure another fade request did not come in and superceed
            if (_fadeId == _musicHandler.fadeCount)
            {
                // reset
                _musicHandler.stopFading = false;

                if (_musicIsStarting)
                {
                    _audioSource.Play();
                    CCM_nowPlayingMusicHandler = _musicHandler;
                    CCM_fnc_log.withTime.message("CCM_fnc_fadeMusichandler: Played audioSource: " + _audioSource + " for Fade ID: " + _fadeId);
                }



                float currentTime = 0;
                float _startingVolume = _audioSource.volume;

                CCM_fnc_log.withTime.message("CCM_fnc_fadeMusichandler: Now fading audioSource: " + _audioSource + " to volume " + _targetVolume + " from starting volume " + _startingVolume + " with a fade ID of " + _fadeId);


                float _finishTime = Time.unscaledDeltaTime + _duration;
                while (currentTime < _finishTime && (_audioSource.volume != _targetVolume) && (!_musicHandler.stopFading) && (_musicHandler.audioSource.isPlaying))
                {
                    currentTime += Time.unscaledDeltaTime;
                    _audioSource.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                    yield return null;
                }

                
                if (!_musicIsStarting)
                {
                    _audioSource.Stop();
                    _audioSource.clip.UnloadAudioData();
                    Destroy(_audioSource.clip);
                    CCM_fnc_log.withTime.message("CCM_fnc_fadeMusichandler: Stopped audioSource: " + _audioSource + " for Fade ID: " + _fadeId);
                }


                CCM_fnc_log.message("CCM_fnc_fadeMusichandler: AudioSource: " + _musicHandler.name + " has finished its fade at " + _audioSource.volume + " for Fade ID: " + _fadeId);

                //_musicHandler.stopFading = false;
                _musicHandler.isFading = false;
            }
            else
            {
                CCM_fnc_log.withTime.message("CCM_fnc_fadeMusichandler: Found fade ID: " + _fadeId + " has been overwritten...");
            }


            yield break;
        }

    }
}