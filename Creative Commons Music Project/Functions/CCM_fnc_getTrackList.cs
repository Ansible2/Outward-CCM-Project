/* ----------------------------------------------------------------------------
Function: CCM_fnc_getTrackList

Description:
	Returns a reference to the corresponging CCM_Lists list for the
     requested type.

Parameters:
	0: _type <int> - The numerical type based upon the CCM_trackTypes_enum
    1: _used <BOOL> - Get the parnter used track list

Returns:
	<LIST of STRINGs> - Returns a reference to the list of the given type

Examples:
    (begin example)
		var _unusedCombatTrackList = CCM_fnc_getTrackList((int)CCM_trackTypes_enum.combat);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Collections.Generic;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static List<string> CCM_fnc_getTrackList(int _trackType = -1, bool _used = false)
        {
            List<string> _listOfTracks;

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_combatTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_combatTracks;
                        }
                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_ambientNightTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_ambientNightTracks;
                        }
                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_ambientDayTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_ambientDayTracks;
                        }
                        break;
                    }
                case ((int)CCM_trackTypes_enum.townDay):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_townDayTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_townDayTracks;
                        }
                        break;
                    }
                case ((int)CCM_trackTypes_enum.townNight):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_townNightTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_townNightTracks;
                        }
                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        if (_used)
                        {
                            _listOfTracks = CCM_Lists.used_dungeonTracks;
                        }
                        else
                        {
                            _listOfTracks = CCM_Lists.unused_dungeonTracks;
                        }
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