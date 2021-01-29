/* ----------------------------------------------------------------------------
Function: CCM_fnc_getTrackList

Description:
	Returns a reference to the corresponging global (unused tracks) list for the
     requested type.

Parameters:
	0: _type <int> - The numerical type based upon the CCM_trackTypes_enum

Returns:
	<LIST of STRINGs> - Returns a reference to the list of the given type

Examples:
    (begin example)
		var _unusedCombatTrackList = CCM_fnc_getTrackList(0);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Collections.Generic;
using BepInEx;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_core : BaseUnityPlugin
    {
        internal static List<string> CCM_fnc_getTrackList(int _trackType = -1)
        {
            List<string> _listOfTracks;

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat):
                    {
                        _listOfTracks = CCM_combatTracks;
                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        _listOfTracks = CCM_ambientNightTracks;
                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        _listOfTracks = CCM_ambientDayTracks;
                        break;
                    }
                case ((int)CCM_trackTypes_enum.town):
                    {
                        _listOfTracks = CCM_townTracks;
                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        _listOfTracks = CCM_dungeonTracks;
                        break;
                    }
                default:
                    {
                        _listOfTracks = new List<string>();
                        break;
                    }
            }

            return _listOfTracks;
        }
    }
}