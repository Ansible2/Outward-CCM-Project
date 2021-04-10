/* ----------------------------------------------------------------------------
Function: CCM_fnc_getAllAvailableReplacementTracks

Description:
	Gets a list of possible track filenames for a given game object.
    Depending on the name of the game object, certain tracks will be returned.

Parameters:
	0: _objectName <STRING> - The name of the object that the music will play on

Returns:
	<LIST of STRINGS> - A list of filenames as strings (with extensions) of tracks that could be played

Examples:
    (begin example)
		var _listOfPossibleTracks = CCM_fnc_getAllAvailableReplacementTracks(_aGameObject.name);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Linq;
using System.Collections.Generic;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static List<CCM_track> CCM_fnc_getAllAvailableReplacementTracks(int _trackType = -1)
        {
            List<CCM_track> _listOfTracks;

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat) :
                    {
                        if (CCM_Lists.unused_combatTracks.Count() < 1)
                        {
                            CCM_Lists.unused_combatTracks = new List<CCM_track>(CCM_Lists.used_combatTracks);
                            CCM_Lists.used_combatTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_combatTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        if (CCM_Lists.unused_ambientNightTracks.Count() < 1)
                        {
                            CCM_Lists.unused_ambientNightTracks = new List<CCM_track>(CCM_Lists.used_ambientNightTracks);
                            CCM_Lists.used_ambientNightTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_ambientNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        if (CCM_Lists.unused_ambientDayTracks.Count() < 1)
                        {
                            CCM_Lists.unused_ambientDayTracks = new List<CCM_track>(CCM_Lists.used_ambientDayTracks);
                            CCM_Lists.used_ambientDayTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_ambientDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townDay):
                    {
                        if (CCM_Lists.unused_townDayTracks.Count() < 1)
                        {
                            CCM_Lists.unused_townDayTracks = new List<CCM_track>(CCM_Lists.used_townDayTracks);
                            CCM_Lists.used_townDayTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_townDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townNight):
                    {
                        if (CCM_Lists.unused_townNightTracks.Count() < 1)
                        {
                            CCM_Lists.unused_townNightTracks = new List<CCM_track>(CCM_Lists.used_townNightTracks);
                            CCM_Lists.used_townNightTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_townNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        if (CCM_Lists.unused_dungeonTracks.Count() < 1)
                        {
                            CCM_Lists.unused_dungeonTracks = new List<CCM_track>(CCM_Lists.used_dungeonTracks);
                            CCM_Lists.used_dungeonTracks.Clear();
                        }
                        _listOfTracks = CCM_Lists.unused_dungeonTracks;

                        break;
                    }
                default:
                    {
                        _listOfTracks = new List<CCM_track>();
                        break;
                    }
            }

            return _listOfTracks;
        }

    }
}