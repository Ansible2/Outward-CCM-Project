/* ----------------------------------------------------------------------------
Function: CCM_fnc_getMusicHandler

Description:
	Returns a one of the two CCM music handle GameObjects depending on which was
     the last one wo be used.

Parameters:
	NONE 

Returns:
	<CCM_MusicHandler> - Either music handler 1 or 2

Examples:
    (begin example)
		leastRecentHandler = CCM_fnc_getMusicHandler();
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /// <summary>
        /// Returns a one of the two CCM music handle GameObjects depending on which was the last one wo be used.
        /// </summary>
        /// <returns>CCM_MusicHandler</returns>
        internal static CCM_MusicHandler CCM_fnc_getMusicHandler()
        {
            
            if (CCM_lastUsedHandler == CCM_MusicHandler_1)
            {
                CCM_lastUsedHandler = CCM_MusicHandler_2;
                return CCM_MusicHandler_2;
            }
            else
            {
                CCM_lastUsedHandler = CCM_MusicHandler_1;
                return CCM_MusicHandler_1;
            }
        }
    }
}