/* ----------------------------------------------------------------------------
Function: CCM_fnc_grabRandomTrack

Description:
	Randomly selects an entry from a list of strings.
    
    This also removes the entry from the list.

Parameters:
	0: _trackList <LIST of STRINGs> - A list of track fileNames to select from

Returns:
	<STRING> - The name of the track file (with extension)

Examples:
    (begin example)
		string _trackFileName = CCM_fnc_grabRandomTrack(_listOfTracks);
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
        internal static string CCM_fnc_grabRandomTrack(List<string> _trackList)
        {
            string _trackFilename = "";

            if (_trackList.Count() < 1)
            {
                CCM_fnc_logWithTime("_trackList is empty!");
            }
            else
            {
                int _randomIndex = CCM_getRandom.Next(_trackList.Count());
                _trackFilename = _trackList.ElementAt(_randomIndex);
                _trackList.RemoveAt(_randomIndex);
            }

            CCM_fnc_logWithTime("Selected track file:");
            CCM_fnc_logWithTime(_trackFilename);

            return _trackFilename;
        }
    }
}