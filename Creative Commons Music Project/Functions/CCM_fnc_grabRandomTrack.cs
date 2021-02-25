/* ----------------------------------------------------------------------------
Function: CCM_fnc_grabRandomTrack

Description:
	Randomly selects an entry from a list of the availale tack names based upon the track type
    
    This also removes the entry from the unused list and adds it to used tracks list for the
     corresponding track type list.

Parameters:
	0: _trackType <int> - The int cas of the track type as correspons with the CCM_trackTypes_enum

Returns:
	<STRING> - The name of the track file (with extension)

Examples:
    (begin example)
		string _aRandomCombatTrack = CCM_fnc_grabRandomTrack((int)CCM_trackTypes_enum.combat);
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
        internal static string CCM_fnc_grabRandomTrack(int _trackType)
        {
            List<string> _listOfUsedTracks;
            List<string> _listOfUnusedTracks;

            switch (_trackType)
            {
                case ((int)CCM_trackTypes_enum.combat):
                    {
                        _listOfUsedTracks = CCM_Lists.used_combatTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_combatTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientNight):
                    {
                        _listOfUsedTracks = CCM_Lists.used_ambientNightTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_ambientNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.ambientDay):
                    {
                        _listOfUsedTracks = CCM_Lists.used_ambientDayTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_ambientDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townDay):
                    {
                        _listOfUsedTracks = CCM_Lists.used_townDayTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_townDayTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.townNight):
                    {
                        _listOfUsedTracks = CCM_Lists.used_townNightTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_townNightTracks;

                        break;
                    }
                case ((int)CCM_trackTypes_enum.dungeon):
                    {
                        _listOfUsedTracks = CCM_Lists.used_dungeonTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_dungeonTracks;

                        break;
                    }
                default:
                    {
                        _listOfUnusedTracks = new List<string>();
                        _listOfUnusedTracks.Add("");
                        _listOfUsedTracks = new List<string>();
                        break;
                    }
            }


            // reallocate used list if unused list is empty now
            if (_listOfUnusedTracks.Count() < 1)
            {
                _listOfUnusedTracks = new List<string>(_listOfUsedTracks);
                _listOfUsedTracks.Clear();
            }

            // randomly select track or give error
            string _trackFilename = "";
            if (_listOfUnusedTracks.Count() < 1)
            {
                CCM_logSource.LogError("CCM_fnc_grabRandomTrack: _listOfUnusedTracks is empty!");
            }
            else
            {
                int _randomIndex = CCM_getRandom.Next(_listOfUnusedTracks.Count());
                _trackFilename = _listOfUnusedTracks.ElementAt(_randomIndex);
                _listOfUnusedTracks.RemoveAt(_randomIndex);
                _listOfUsedTracks.Add(_trackFilename);
            }

            CCM_fnc_logWithTime("CCM_fnc_grabRandomTrack: Selected track file: " + _trackFilename);

            return _trackFilename;
        }
    }
}