/* ----------------------------------------------------------------------------
Function: CCM_fnc_stopMusicFromLooping

Description:
	Stops a game object's audio source from playing. Returns a bool depending on 
     changes.
    
    Can either fade out the audio or perform an immediate stop.

Parameters:
	0: _objectToStop <GameObject OR AudioSource> - The object or audio source to stop the audio playing on
    1: _fadeOut <BOOL> - Should the audio fade out or immediately stop

Returns:
	<BOOL> - True if playing was stopped, false if no changes made

Examples:
    (begin example)
		var _didChange = CCM_fnc_stopMusicFromPlaying(_aGameObject);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static bool CCM_fnc_stopMusicFromPlaying(AudioSource _audioSourceToStop, bool _fadeOut = true)
        {
            bool _isPlaying = _audioSourceToStop.isPlaying;
            if (_isPlaying)
            {
                if (_fadeOut)
                {
                    CCM_Instance.StartCoroutine(CCM_scheduled.CCM_fnc_fadeAudioSource(_audioSourceToStop, 3, 0, true));
                }
                else
                {
                    _audioSourceToStop.Stop();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool CCM_fnc_stopMusicFromPlaying(GameObject _objectToStop, bool _fadeOut = true)
        {
            AudioSource _objectAudioSource = _objectToStop.GetComponent<AudioSource>();
            bool _isPlaying = _objectAudioSource.isPlaying;
            if (_isPlaying)
            {
                if (_fadeOut)
                {
                    CCM_Instance.StartCoroutine(CCM_scheduled.CCM_fnc_fadeAudioSource(_objectAudioSource, 3, 0, true));
                }
                else
                {
                    _objectAudioSource.Stop();
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}