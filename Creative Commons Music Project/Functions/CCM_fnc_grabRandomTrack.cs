﻿/* ----------------------------------------------------------------------------
Function: CCM_fnc_grabRandomTrack

Description:
	Randomly selects an entry from a list of the availale tack names based upon the track type
    
    This also removes the entry from the unused list and adds it to used tracks list for the
     corresponding track type list.

Parameters:
	0: _trackType <CCM_trackTypes_enum> - The CCM_trackTypes_enum for the tupe of track

Returns:
	<CCM_track> - The CCM_track randomly chosen from lists

Examples:
    (begin example)
		CCM_track _aRandomCombatTrack = CCM_fnc_grabRandomTrack(CCM_trackTypes_enum.combat);
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
        /// <summary>
        /// Randomly selects an entry from a list of the availale tack names based upon the track type
        /// </summary>
        /// <param name="_trackType"></param>
        /// <returns>CCM_track</returns>
        internal static CCM_track CCM_fnc_grabRandomTrack(CCM_trackTypes_enum _trackType)
        {
            List<CCM_track> _listOfUsedTracks;
            List<CCM_track> _listOfUnusedTracks;

            switch (_trackType)
            {
                case (CCM_trackTypes_enum.combat):
                    {
                        _listOfUsedTracks = CCM_Lists.used_combatTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_combatTracks;

                        break;
                    }
                case (CCM_trackTypes_enum.ambientNight):
                    {
                        _listOfUsedTracks = CCM_Lists.used_ambientNightTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_ambientNightTracks;

                        break;
                    }
                case (CCM_trackTypes_enum.ambientDay):
                    {
                        _listOfUsedTracks = CCM_Lists.used_ambientDayTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_ambientDayTracks;

                        break;
                    }
                case (CCM_trackTypes_enum.townDay):
                    {
                        _listOfUsedTracks = CCM_Lists.used_townDayTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_townDayTracks;

                        break;
                    }
                case (CCM_trackTypes_enum.townNight):
                    {
                        _listOfUsedTracks = CCM_Lists.used_townNightTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_townNightTracks;

                        break;
                    }
                case (CCM_trackTypes_enum.dungeon):
                    {
                        _listOfUsedTracks = CCM_Lists.used_dungeonTracks;
                        _listOfUnusedTracks = CCM_Lists.unused_dungeonTracks;

                        break;
                    }
                default:
                    {
                        _listOfUnusedTracks = new List<CCM_track>();
                        _listOfUsedTracks = new List<CCM_track>();
                        break;
                    }
            }


            // reallocate used list if unused list is empty now
            if (_listOfUnusedTracks.Count() < 1)
            {
                //_listOfUnusedTracks = new List<CCM_track>(_listOfUsedTracks);
                _listOfUnusedTracks.AddRange(_listOfUsedTracks);
                _listOfUsedTracks.Clear();
                CCM_fnc_log.warning("CCM_fnc_grabRandomTrack: Lists reset for track type: " + _trackType.ToString());
            }

            CCM_track _track = new CCM_track();
            int _randomIndex = -1;
            if (_listOfUnusedTracks.Count() < 1)
            {
                CCM_fnc_log.error("CCM_fnc_grabRandomTrack: _listOfUnusedTracks is empty!");
            }
            else
            {
                CCM_fnc_log.info("CCM_fnc_grabRandomTrack: Count of used tracks for type " + _trackType.ToString() + " is " + _listOfUsedTracks.Count() + " and unused is " + _listOfUnusedTracks.Count());
                _randomIndex = CCM_getRandom.Next(_listOfUnusedTracks.Count());
                _track = _listOfUnusedTracks.ElementAt(_randomIndex);
                _listOfUnusedTracks.RemoveAt(_randomIndex);
                _listOfUsedTracks.Add(_track);
            }

            
            CCM_fnc_log.info("CCM_fnc_grabRandomTrack: Selected track file: " + _track.Filename + " at index: " + _randomIndex);

            return _track;
        }
    }
}