/* ----------------------------------------------------------------------------
Function: CCM_fnc_assignMusicHandlerProperties

Description:
	Takes a template GameObject and (deep) copies its properties to both CCM music
     handler objects. It also assigns a few key global values.

Parameters:
	0:  <GameObject> - The object to use as a tempalte from which to copy the properties

Returns:
	NOTHING 

Examples:
    (begin example)
		CCM_fnc_assignMusicHandlerProperties(_objectToCopyFrom);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using BepInEx.Logging;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        ///<summary>
        /// Takes a template GameObject and (deep) copies its properties to both CCM music handler objects. It also assigns a few key global values.
        ///</summary>
        internal static void CCM_fnc_assignMusicHandlerProperties(GameObject _objectToCopy)
        {
            if (!CCM_createdMusicHandlers)
            {
                // if main music object is not found select an object
                if (_objectToCopy == null)
                {
                    CCM_fnc_log.error("CCM_fnc_assignMusicHandlerProperties: _objectToCopy is null");
                }
                else
                {
                    CCM_musicVolume = _objectToCopy.GetComponent<AudioSource>().volume;
                    CCM_MusicHandler_1 = new CCM_MusicHandler("CCM_musicHandler_1", _objectToCopy);
                    CCM_MusicHandler_2 = new CCM_MusicHandler("CCM_musicHandler_2", _objectToCopy);

                    CCM_createdMusicHandlers = true;
                    CCM_fnc_log.info("CCM_fnc_assignMusicHandlerProperties: Assigned CCM music handler objects the properties of " + _objectToCopy);
                }
            }
        }
    }
}