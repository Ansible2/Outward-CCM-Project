/* ----------------------------------------------------------------------------
Function: CCM_fnc_getTrackTypeFolderPath

Description:
	Returns the full folder path of a given track type.

Parameters:
	0: _trackType <INT> - The track type in relation to the enum CCM_trackTypes_enum

Returns:
	<STRING> - The absolute folder path

Examples:
    (begin example)
		var _combatFolderPath = CCM_fnc_getTrackTypeFolderPath(2);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static string CCM_fnc_getTrackTypeFolderPath(int _trackType)
        {
            switch (_trackType)
            {
                case (int)CCM_trackTypes_enum.ambientDay:
                    {
                        return CCM_ambientDayFolderPath;
                    }
                case (int)CCM_trackTypes_enum.ambientNight:
                    {
                        return CCM_ambientNightFolderPath;
                    }
                case (int)CCM_trackTypes_enum.combat:
                    {
                        return CCM_combatFolderPath;
                    }
                case (int)CCM_trackTypes_enum.dungeon:
                    {
                        return CCM_dungeonFolderPath;
                    }
                case (int)CCM_trackTypes_enum.town:
                    {
                        return CCM_townFolderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("Returned empty string for folder path");
                        return "";
                    }
            }
        }
    }
}