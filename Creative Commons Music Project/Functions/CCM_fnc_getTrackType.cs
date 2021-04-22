/* ----------------------------------------------------------------------------
Function: CCM_fnc_getTrackType

Description:
	Takes a string (of a GameObject) and decideds on what the type of music that
     game object plays.

    The return is a CCM_trackTypes_enum.

Parameters:
	0: _objectName <STRING> - The name of the GameObject you wish to find a track type for

Returns:
	<CCM_trackTypes_enum> - An enum for the type of track suited to replace the music

Examples:
    (begin example)
		CCM_trackTypes_enum _trackType = CCM_fnc_getTrackType(_myObjectName);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /// <summary>
        /// Takes a string (of a GameObject) and decideds on what the type of music that game object plays.
        /// </summary>
        /// <param name="_objectName"></param>
        /// <returns>CCM_trackTypes_enum</returns>
        internal static CCM_trackTypes_enum CCM_fnc_getTrackType(string _objectName = "")
        {
            // .Contains() is case sensetive
            _objectName = _objectName.ToLower();
            var _type = CCM_trackTypes_enum.EMPTY;
            string _logString = "CCM_fnc_getTrackType: Found that object " + _objectName + " matched tracks for ";

            if (_objectName.Contains("dungeon"))
            {
                CCM_fnc_logWithTime(_logString + "dungeons");
                _type = CCM_trackTypes_enum.dungeon;
            }
            else if (_objectName.Contains("combat"))
            {
                CCM_fnc_logWithTime(_logString + "combat");
                _type = CCM_trackTypes_enum.combat;
            }
            else if (_objectName.Contains("region") || _objectName.Contains("chersonese"))
            {

                if (_objectName.Contains("night"))
                {
                    CCM_fnc_logWithTime(_logString + "ambient night");
                    _type = CCM_trackTypes_enum.ambientNight;
                }
                else
                {
                    CCM_fnc_logWithTime(_logString + "ambient day");
                    _type = CCM_trackTypes_enum.ambientDay;
                }

            }
            else if (_objectName.Contains("town") || _objectName.Contains("cierzo"))
            {
                if (_objectName.Contains("night"))
                {
                    CCM_fnc_logWithTime(_logString + "Towns night");
                    _type = CCM_trackTypes_enum.townNight;
                }
                else
                {
                    CCM_fnc_logWithTime(_logString + "Towns day");
                    _type = CCM_trackTypes_enum.townDay;
                }
            }
            else
            {
                CCM_fnc_logWithTime("CCM_fnc_getTrackType: Did not find any track type to match " + _objectName);
            }

            return _type;
        }
    }
}