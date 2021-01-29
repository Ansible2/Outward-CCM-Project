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
using BepInEx;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_core : BaseUnityPlugin
    {
        internal static List<string> CCM_fnc_getAllAvailableReplacementTracks(int _trackType = -1)
        {
            List<string> _listOfTracks = new List<string>();

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat) :
                    {
                        if (CCM_combatTracks.Count() < 1)
                        {
                            CCM_combatTracks = new List<string>(CCM_usedCombatTracks);
                            CCM_usedCombatTracks.Clear();
                        }

                        _listOfTracks = CCM_combatTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        if (CCM_ambientNightTracks.Count() < 1)
                        {
                            CCM_ambientNightTracks = new List<string>(CCM_usedAmbientNightTracks);
                            CCM_usedAmbientNightTracks.Clear();
                        }

                        _listOfTracks = CCM_ambientNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        if (CCM_ambientDayTracks.Count() < 1)
                        {
                            CCM_ambientDayTracks = new List<string>(CCM_usedAmbientDayTracks);
                            CCM_usedAmbientDayTracks.Clear();
                        }

                        _listOfTracks = CCM_ambientDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.town):
                    {
                        if (CCM_townTracks.Count() < 1)
                        {
                            CCM_townTracks = new List<string>(CCM_usedTownTracks);
                            CCM_usedTownTracks.Clear();
                        }
                        _listOfTracks = CCM_townTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        if (CCM_dungeonTracks.Count() < 1)
                        {
                            CCM_dungeonTracks = new List<string>(CCM_usedDungeonTracks);
                            CCM_usedDungeonTracks.Clear();
                        }

                        _listOfTracks = CCM_dungeonTracks;

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