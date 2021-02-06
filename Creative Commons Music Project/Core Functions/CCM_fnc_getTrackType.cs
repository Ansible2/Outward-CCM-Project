/* ----------------------------------------------------------------------------
Function: CCM_fnc_getTrackType

Description:
	Takes a string (of a GameObject) and decideds on what the type of music that
     game object plays.

    The return is an int based upon the CCM_trackTypes_enum enum.

Parameters:
	0: _objectName <STRING> - The name of the GameObject you wish to find a track type for

Returns:
	<INT> - A number that corresponds to the enum CCM_trackTypes_enum

Examples:
    (begin example)
		int _trackType = CCM_fnc_getTrackType(_myObjectName);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static int CCM_fnc_getTrackType(string _objectName = "")
        {
            // .Contains() is case sensetive
            _objectName = _objectName.ToLower();
            int _type = -1;
            string _logString = "CCM_fnc_getTrackType: Found that object " + _objectName + " matched tracks for ";

            if (_objectName.Contains("dungeon"))
            {
                CCM_fnc_logWithTime(_logString + "dungeons");
                _type = (int)CCM_trackTypes_enum.dungeon;
            }
            else if (_objectName.Contains("combat"))
            {
                CCM_fnc_logWithTime(_logString + "combat");
                _type = (int)CCM_trackTypes_enum.combat;
            }
            else if (_objectName.Contains("region") || _objectName.Contains("chersonese"))
            {

                if (_objectName.Contains("night"))
                {
                    CCM_fnc_logWithTime(_logString + "ambient night");
                    _type = (int)CCM_trackTypes_enum.ambientNight;
                }
                else
                {
                    CCM_fnc_logWithTime(_logString + "ambient day");
                    _type = (int)CCM_trackTypes_enum.ambientDay;
                }

            }
            else if (_objectName.Contains("town") || _objectName.Contains("cierzo"))
            {
                CCM_fnc_logWithTime(_logString + "towns");
                _type = (int)CCM_trackTypes_enum.town;
            }
            else
            {
                CCM_fnc_logWithTime("CCM_fnc_getTrackType: Did not find any track type to match " + _objectName);
            }

            return _type;
        }
    }
}