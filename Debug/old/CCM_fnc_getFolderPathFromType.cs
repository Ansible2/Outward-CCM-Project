/* ----------------------------------------------------------------------------
Function: CCM_fnc_getFolderPathFromType

Description:
	Takes a CCM_trackTypes_enum and converts it to the corresponding folder path.

Parameters:
	0: _audioSoucre <CCM_trackTypes_enum or INT> - The type to get a folder path for

Returns:
	STRING - The folder path

Examples:
    (begin example)
		// get day tracks folder path
		var _path = CCM_fnc_getFolderPathFromType(CCM_trackTypes_enum.day);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static string CCM_fnc_getFolderPathFromType(int _trackType)
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
                case (int)CCM_trackTypes_enum.night:
                    {
                        return CCM_Paths.night_folderPath;
                    }
                case (int)CCM_trackTypes_enum.day:
                    {
                        return CCM_Paths.day_folderPath;
                    }
                case (int)CCM_trackTypes_enum.ambient:
                    {
                        return CCM_Paths.ambient_folderPath;
                    }
                case (int)CCM_trackTypes_enum.town:
                    {
                        return CCM_Paths.town_folderPath;
                    }
                case (int)CCM_trackTypes_enum.configed:
                    {
                        return CCM_Paths.tracks_folderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("CCM_fnc_getFolderPathFromType: Returned empty string for folder path for int " + _trackType);
                        return "";
                    }
            }
        }

        internal static string CCM_fnc_getFolderPathFromType(CCM_trackTypes_enum _trackType)
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
                case CCM_trackTypes_enum.night:
                    {
                        return CCM_Paths.night_folderPath;
                    }
                case CCM_trackTypes_enum.day:
                    {
                        return CCM_Paths.day_folderPath;
                    }
                case CCM_trackTypes_enum.ambient:
                    {
                        return CCM_Paths.ambient_folderPath;
                    }
                case CCM_trackTypes_enum.town:
                    {
                        return CCM_Paths.town_folderPath;
                    }
                case CCM_trackTypes_enum.configed:
                    {
                        return CCM_Paths.tracks_folderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("CCM_fnc_getFolderPathFromType: Returned empty string for folder path for Enum " + _trackType);
                        return "";
                    }
            }
        }
    }
}