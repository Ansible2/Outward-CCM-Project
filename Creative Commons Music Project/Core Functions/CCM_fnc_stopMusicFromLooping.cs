/* ----------------------------------------------------------------------------
Function: CCM_fnc_stopMusicFromLooping

Description:
	Stops a game object's audio source from looping. Returns a bool depending on 
     changes.

Parameters:
	0: _objectToStop <GameObject OR AudioSource> - The object or audio source to stop the audio looping on

Returns:
	<BOOL> - True if looping was stopped, false if no changes made

Examples:
    (begin example)
		var _didChange = CCM_fnc_stopMusicFromLooping(_aGameObject);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using BepInEx;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_core : BaseUnityPlugin
    {
        internal static bool CCM_fnc_stopMusicFromLooping(AudioSource _audioSourceToStop) 
        {
            bool _isLooping = _audioSourceToStop.loop;
            if (_isLooping)
            {
                _audioSourceToStop.loop = false;

                return true;
            }
            else
            {
                return false;
            }
        }


        internal static bool CCM_fnc_stopMusicFromLooping(GameObject _objectToStop)
        {
            bool _isLooping = _objectToStop.GetComponent<AudioSource>().loop;
            if (_isLooping)
            {
                _objectToStop.GetComponent<AudioSource>().loop = false;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}