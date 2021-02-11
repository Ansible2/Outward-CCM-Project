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
                        return CCM_Paths.ambientDay_folderPath;
                    }
                case (int)CCM_trackTypes_enum.ambientNight:
                    {
                        return CCM_Paths.ambientNight_folderPath;
                    }
                case (int)CCM_trackTypes_enum.combat:
                    {
                        return CCM_Paths.combat_folderPath;
                    }
                case (int)CCM_trackTypes_enum.dungeon:
                    {
                        return CCM_Paths.dungeon_folderPath;
                    }
                case (int)CCM_trackTypes_enum.townDay:
                    {
                        return CCM_Paths.townDay_folderPath;
                    }
                case (int)CCM_trackTypes_enum.townNight:
                    {
                        return CCM_Paths.townNight_folderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("CCM_fnc_getTrackTypeFolderPath: Returned empty string for folder path for int " + _trackType);
                        return "";
                    }
            }
        }

        internal static string CCM_fnc_getTrackTypeFolderPath(CCM_trackTypes_enum _trackType)
        {
            switch (_trackType)
            {
                case CCM_trackTypes_enum.ambientDay:
                    {
                        return CCM_Paths.ambientDay_folderPath;
                    }
                case CCM_trackTypes_enum.ambientNight:
                    {
                        return CCM_Paths.ambientNight_folderPath;
                    }
                case CCM_trackTypes_enum.combat:
                    {
                        return CCM_Paths.combat_folderPath;
                    }
                case CCM_trackTypes_enum.dungeon:
                    {
                        return CCM_Paths.dungeon_folderPath;
                    }
                case CCM_trackTypes_enum.townDay:
                    {
                        return CCM_Paths.townDay_folderPath;
                    }
                case CCM_trackTypes_enum.townNight:
                    {
                        return CCM_Paths.townNight_folderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("CCM_fnc_getTrackTypeFolderPath: Returned empty string for folder path for Enum " + _trackType);
                        return "";
                    }
            }
        }
    }
}