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
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static GameObject CCM_fnc_getMusicHandler()
        {
            bool _isPlaying = CCM_MusicHandlers.musicAudioSource_1.isPlaying;
            if (_isPlaying)
            {
                return CCM_MusicHandlers.musicHandler_2;
            }
            else
            {
                return CCM_MusicHandlers.musicHandler_1;
            }
        }
    }
}