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
        internal static List<string> CCM_fnc_getAllAvailableReplacementTracks(int _trackType = -1)
        {
            List<string> _listOfTracks = new List<string>();

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat) :
                    {
                        if (CCM_list_combatTracks.Count() < 1)
                        {
                            CCM_list_combatTracks = new List<string>(CCM_list_usedCombatTracks);
                            CCM_list_usedCombatTracks.Clear();
                        }

                        _listOfTracks = CCM_list_combatTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        if (CCM_list_ambientNightTracks.Count() < 1)
                        {
                            CCM_list_ambientNightTracks = new List<string>(CCM_list_usedAmbientNightTracks);
                            CCM_list_usedAmbientNightTracks.Clear();
                        }

                        _listOfTracks = CCM_list_ambientNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        if (CCM_list_ambientDayTracks.Count() < 1)
                        {
                            CCM_list_ambientDayTracks = new List<string>(CCM_list_usedAmbientDayTracks);
                            CCM_list_usedAmbientDayTracks.Clear();
                        }

                        _listOfTracks = CCM_list_ambientDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townDay):
                    {
                        if (CCM_list_townDayTracks.Count() < 1)
                        {
                            CCM_list_townDayTracks = new List<string>(CCM_list_usedTownDayTracks);
                            CCM_list_usedTownDayTracks.Clear();
                        }
                        _listOfTracks = CCM_list_townDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townNight):
                    {
                        if (CCM_list_townNightTracks.Count() < 1)
                        {
                            CCM_list_townNightTracks = new List<string>(CCM_list_usedTownNightTracks);
                            CCM_list_usedTownNightTracks.Clear();
                        }
                        _listOfTracks = CCM_list_townNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        if (CCM_list_dungeonTracks.Count() < 1)
                        {
                            CCM_list_dungeonTracks = new List<string>(CCM_list_usedDungeonTracks);
                            CCM_list_usedDungeonTracks.Clear();
                        }

                        _listOfTracks = CCM_list_dungeonTracks;

                        break;
                    }
                default:
                    {
                        _listOfTracks.Add("");
                        break;
                    }
            }

            return _listOfTracks;
        }

    }
}