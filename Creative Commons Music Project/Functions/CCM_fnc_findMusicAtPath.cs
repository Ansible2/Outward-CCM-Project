/* ----------------------------------------------------------------------------
Function: CCM_fnc_findMusicAtPath

Description:
	Finds all .ogg, .mp3, and .wav audio files at a given path.

Parameters:
	0: _folderPathToSearch <STRING> - The path to search inside

Returns:
	<LIST of STRINGs> - A list of all the filenames at the path (with extensions)

Examples:
    (begin example)
        string _folderPath = "B:\GOG Games\Outward\Mods\CCM Project\Combat Tracks";
		var _listOfFilenames = CCM_fnc_findMusicAtPath(_folderPath);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Linq;
using System.Collections.Generic;
using System.IO;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal List<string> CCM_fnc_findMusicAtPath(string _folderPathToSearch)
        {
            List<string> _returnList = new List<string>();

            if (!Directory.Exists(_folderPathToSearch))
            {
                CCM_logSource.LogMessage("CCM_fnc_findMusicAtPath: Folder path " + _folderPathToSearch + " does not exist...");
            }
            else
            {
                // this will get all files of .ogg, .mp3, and .wav. However, this includes their FULL paths
                string[] _files = Directory.GetFiles(_folderPathToSearch, "*.ogg");
                List<string> _tempList = _files.ToList();
                _files = Directory.GetFiles(_folderPathToSearch, "*.mp3");
                _tempList.AddRange(_files);
                _files = Directory.GetFiles(_folderPathToSearch, "*.wav");
                _tempList.AddRange(_files);


                if (_tempList.Count() < 1)
                {
                    CCM_fnc_logWithTime("CCM_fnc_findMusicAtPath: File path " + _folderPathToSearch + " returned no files...");
                }
                else
                {
                    // get only the file names for returns
                    string _tempFileName;
                    foreach (string _filePath in _tempList)
                    {
                        _tempFileName = Path.GetFileName(_filePath);
                        _returnList.Add(_tempFileName);
                    }
                }
            }

            return _returnList;
        }
    }
}