/* ----------------------------------------------------------------------------
Function: CCM_fnc_getMusicHandler

Description:
	Returns a one of the two CCM music handle GameObjects depending on if #1 is
     playing or not.

Parameters:
	NONE 

Returns:
	<GameObject> - Either music handler 1 or 2

Examples:
    (begin example)
		_aNotPlayingMusicHandler = CCM_fnc_getMusicHandler();
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
        internal static GameObject CCM_fnc_getMusicHandler()
        {
            AudioSource _objectAudioSource = CCM_musicHandler_1.GetComponent<AudioSource>();
            bool _isPlaying = _objectAudioSource.isPlaying;
            if (_isPlaying)
            {
                return CCM_musicHandler_2;
            }
            else
            {
                return CCM_musicHandler_1;
            }
        }
    }
}