/* ----------------------------------------------------------------------------
Function: CCM_fnc_parseConfig

Description:
	Loads all audio files into memory from the parsed ini document. 
	
	Also populates the relevant lists of tracks with strings for selection.

Parameters:
	NONE

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_fnc_parseConfig();
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using BepInEx.Logging;
using System.Collections;
using UnityEngine.Networking;
using System;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        const string CCM_configFileName = @"CCM Config.xml";

        /* ----------------------------------------------------------------------------
            CCM_fnc_parseConfig
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_parseConfig()
        {
            string _pathToConfig = Path.Combine(CCM_Paths.mainFolderPath, CCM_configFileName);
            
            if (File.Exists(_pathToConfig))
            {
                var _xmlConfigFile = XDocument.Load(_pathToConfig);
                _fn_buildAudioClipLibrary(_xmlConfigFile);
                _fn_grabTrackSpacingSettings(_xmlConfigFile);
                _fn_getOnlineMode(_xmlConfigFile);
            }
            else
            {
                CCM_logSource.Log(LogLevel.Fatal, "Config file could not be found at: " + _pathToConfig);
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_buildAudioClipLibrary
        ---------------------------------------------------------------------------- */
        ///<summary>Does stuff</summary>
        private static void _fn_buildAudioClipLibrary(XDocument _xmlConfigFile)
        {
            // so we don't get duplicates, store already gotten files
            //List<string> _alreadyStoredTracks = new List<string>();

            var _list = _xmlConfigFile.Root.Descendants("tracks").Elements();
            //_xmlConfigFile.Root.Nodes

            if (_list.Count() == 0)
            {
                CCM_logSource.LogMessage("CCM_fnc_parseConfig: _fn_buildAudioClipLibrary: No tracks were located in config file: " + CCM_configFileName);
            }

            // debug
            /*
            foreach (var _x in _list)
            {
                CCM_logSource.LogMessage(_x.ToString());
                var _filename = _x.Element("filename").Value.ToLower();
                CCM_fnc_logWithTime(_filename.Trim());
            }
            */
        
            foreach (var _x in _list)
            {
                var _filename = _x.Element("filename").Value.ToLower().Trim();
                //CCM_fnc_logWithTime(_filename);
                // make sure provided filename is actually in the tracks folder
                if (_fn_doesFileExist(_filename, CCM_Paths.tracks_folderPath))
                {
                    // check for duplicates
                    if (CCM_Lists.storedTracks.Contains(_filename))
                    {
                        CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_buildAudioClipLibrary: Configed track: " + _filename + " within " + CCM_configFileName + " is a duplicate!");
                    }
                    else
                    {
                        // collects all configed track types for the track
                        var _trackTypes = _x.Element("track_types").Descendants("track_type").ToList();
                        
                        if (_trackTypes.Count() > 0)
                        {
                            foreach (var _y in _trackTypes)
                            {
                                _fn_pushBackToTrackList(_y.Value, _filename);
                            }

                            CCM_Lists.storedTracks.Add(_filename);
                            CCM_Instance.StartCoroutine(_fn_loadAndStoreAudioClip(_filename, CCM_Paths.tracks_folderPath));
                        }
                        else
                        {
                            CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_buildAudioClipLibrary: Did not find any track types for file: " + _filename + " within " + CCM_configFileName);
                        }
                    }
                }
                else
                {
                    CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_buildAudioClipLibrary: Did not find track file: " + _filename + " within tracks folder!");
                }
            }
        
        }


        /* ----------------------------------------------------------------------------
           _fn_getAudioClipsFromFolders
        ---------------------------------------------------------------------------- */
        private static void _fn_getAudioClipsFromFolders()
        {
            CCM_trackTypes_enum[] _trackTypes = (CCM_trackTypes_enum[])Enum.GetValues(typeof(CCM_trackTypes_enum));

            List<string> _fileNames;
            foreach (var _trackType in _trackTypes)
            {
                string _folderPath = CCM_fnc_getTrackTypeFolderPath(_trackType);
                _fileNames = _fn_getFileNamesAtPath(_folderPath);

                if (_fileNames.Count() == 0)
                {
                    CCM_logSource.LogMessage("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Folder path " + _folderPath + " returned no files!");
                }
                else
                {
                    // get only the file names for returns
                    foreach (string _filename in _fileNames)
                    {                          
                        if (!CCM_Lists.storedTracks.Contains(_filename)) 
                        {
                            _fn_pushBackToTrackList(_trackType, _filename);
                            CCM_Instance.StartCoroutine(_fn_loadAndStoreAudioClip(_filename, _folderPath));
                            CCM_Lists.storedTracks.Add(_filename);
                        }
                        else
                        {
                            CCM_logSource.LogMessage("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Found that track: " + _filename + " was already loaded in another location, throwing away duplicate...");
                        }
                    }
                }
            }



            // Do any tracks that were not configed but are in the "tracks" folder
            string _folderPathToSearch = CCM_Paths.tracks_folderPath;            
            _fileNames = _fn_getFileNamesAtPath(_folderPathToSearch);


            if (_fileNames.Count() == 0)
            {
                CCM_fnc_logWithTime("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Folder path " + _folderPathToSearch + " returned no files!");
            }
            else
            {
                // get only the file names for returns
                foreach (string _filename in _fileNames)
                {
                    if (!CCM_Lists.storedTracks.Contains(_filename))
                    {
                        _fn_pushBackToTrackList("all", _filename);
                        CCM_Instance.StartCoroutine(_fn_loadAndStoreAudioClip(_filename, _folderPathToSearch));
                        CCM_Lists.storedTracks.Add(_filename);
                    }
                    else
                    {
                        CCM_logSource.LogMessage("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Found that track: " + _filename + " was already loaded in another location, throwing away duplicate...");
                    }
                }
            }

            _fileNames = null;
        }


        /* ----------------------------------------------------------------------------
           _fn_grabTrackSpacingSettings
        ---------------------------------------------------------------------------- */
        private static void _fn_grabTrackSpacingSettings(XDocument _xmlConfigFile)
        {
            var _trackSpacings = _xmlConfigFile.Root.Descendants("track_spacing").Elements();

            CCM_fnc_logWithTime(_trackSpacings);

            foreach (var _x in _trackSpacings)
            {
                bool _doAdd = false;

                string _name = _x.Name.ToString().ToLower();
                CCM_fnc_logWithTime(_name);

                int _trackType = -1;
                
                int _min = (int)_x.Element("min");
                int _max = (int)_x.Element("max");

                switch (_name)
                {
                    case ("trackspacing_townday"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.townDay;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_townnight"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.townNight;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_ambientday"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.ambientDay;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_ambientnight"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.ambientNight;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_dungeon"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.dungeon;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_combat"):
                        {
                            _trackType = (int)CCM_trackTypes_enum.combat;
                            _doAdd = true;
                            break;
                        }
                    default:
                        {
                            CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_grabTrackSpacingSettings: Encountered unkown config for track spacing: " + _name);
                            break;
                        }
                }

                if (_doAdd)
                {
                    CCM_fnc_logWithTime("Added min " + _min + " and max " + _max + " to " + _name);
                    List<int> _list = new List<int>() { _min, _max };
                    CCM_Dictionaries.trackSpacingFromType.Add(_trackType, _list);
                }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_loadAndStoreAudioClip    (AudioClips are loaded at the start and stored due to a need to know their duration in order to queue songs for the future. 
                                        I could've used another library for this, but it is the smoothest for development and playback though costly for memory)
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_loadAndStoreAudioClip(string _filename, string _folderPath)
        {
            var _pathToFile = Path.Combine(CCM_Paths.FILE_PREFIX, _folderPath, _filename);
            AudioType _audioType = CCM_fnc_getAudioTypeFromString(_filename);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_pathToFile, _audioType))
            {
                while (CCM_loadingAudio)
                {
                    yield return new WaitForSeconds(0.05f);
                }

                CCM_loadingAudio = true;
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                CCM_loadingAudio = false;

                CCM_fnc_logWithTime("_fn_loadAndStoreAudioClip: Web request is done for " + _filename);

                if (www.error != null)
                {
                    CCM_fnc_logWithTime("_fn_loadAndStoreAudioClip: Web request encountered the following error: " + www.error);
                    yield break;
                }

                var _clip = DownloadHandlerAudioClip.GetContent(www);
                DontDestroyOnLoad(_clip);
                _clip.name = _filename;

                CCM_Dictionaries.audioClipFromString.Add(_filename, _clip);
            }
            
            yield break;
        }


        /* ----------------------------------------------------------------------------
           _fn_doesFileExist
        ---------------------------------------------------------------------------- */
        private static bool _fn_doesFileExist(string _filename, string _folderPath)
        {
            var _pathToFile = Path.Combine(_folderPath, _filename);

            bool _doesFileExist = false;
            if (File.Exists(_pathToFile))
            {
                _doesFileExist = true;
            }

            CCM_fnc_logWithTime("_fn_doesFileExist: Checked file path " + _pathToFile + " : Does exist? -> " + _doesFileExist);

            return _doesFileExist;
        }


        /* ----------------------------------------------------------------------------
           _fn_pushBackToTrackList
        ---------------------------------------------------------------------------- */
        private static void _fn_pushBackToTrackList(string _trackType, string _filename)
        {
            _trackType = _trackType.ToLower().Trim();

            switch (_trackType)
            {
                case "townday":
                    {
                        CCM_Lists.unused_townDayTracks.Add(_filename);
                        break;
                    }
                case "townnight":
                    {
                        CCM_Lists.unused_townNightTracks.Add(_filename);
                        break;
                    }
                case "ambientday":
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_filename);
                        break;
                    }
                case "ambientnight":
                    {
                        CCM_Lists.unused_ambientNightTracks.Add(_filename);
                        break;
                    }
                case "dungeon":
                    {
                        CCM_Lists.unused_dungeonTracks.Add(_filename);
                        break;
                    }
                case "combat":
                    {
                        CCM_Lists.unused_combatTracks.Add(_filename);
                        break;
                    }
                case "all":
                    {
                        CCM_Lists.unused_townDayTracks.Add(_filename);
                        CCM_Lists.unused_townNightTracks.Add(_filename);
                        CCM_Lists.unused_ambientDayTracks.Add(_filename);
                        CCM_Lists.unused_ambientNightTracks.Add(_filename);
                        CCM_Lists.unused_dungeonTracks.Add(_filename);
                        CCM_Lists.unused_combatTracks.Add(_filename);
                        break;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("_fn_pushBackToList: Encountered unknown track type: " + _trackType);
                        break;
                    }
            }
        }
        // enum overload
        private static void _fn_pushBackToTrackList(CCM_trackTypes_enum _trackType, string _filename)
        {
            switch (_trackType)
            {
                case CCM_trackTypes_enum.townDay:
                    {
                        CCM_Lists.unused_townDayTracks.Add(_filename);
                        break;
                    }
                case CCM_trackTypes_enum.townNight:
                    {
                        CCM_Lists.unused_townNightTracks.Add(_filename);
                        break;
                    }
                case CCM_trackTypes_enum.ambientDay:
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_filename);
                        break;
                    }
                case CCM_trackTypes_enum.ambientNight:
                    {
                        CCM_Lists.unused_ambientNightTracks.Add(_filename);
                        break;
                    }
                case CCM_trackTypes_enum.dungeon:
                    {
                        CCM_Lists.unused_dungeonTracks.Add(_filename);
                        break;
                    }
                case CCM_trackTypes_enum.combat:
                    {
                        CCM_Lists.unused_combatTracks.Add(_filename);
                        break;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("_fn_pushBackToList: Encountered unknown track type enum: " + _trackType);
                        break;
                    }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_getOnlineMode
        ---------------------------------------------------------------------------- */
        private static void _fn_getOnlineMode(XDocument _xmlConfigFile)
        {
            bool _isOnline = _xmlConfigFile.Root.Element("syncOnline").Value.ToUpper() == "ON";

            if (_isOnline)
            {
                CCM_syncOnline = true;
            }
            else
            {
                CCM_syncOnline = false;
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_getFilenamesAtPath
        ---------------------------------------------------------------------------- */
        private static List<string> _fn_getFileNamesAtPath(string _folderPathToSearch)
        {
            List<string> _returnList = new List<string>();

            if (!Directory.Exists(_folderPathToSearch))
            {
                CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_getFileNamesAtPath: Folder path " + _folderPathToSearch + " does not exist.");
            }
            else
            {
                // this will get all files of .ogg, .mp3, and .wav. However, this includes their paths
                string[] _files = Directory.GetFiles(_folderPathToSearch, "*.ogg");
                List<string> _tempList = _files.ToList();

                _files = Directory.GetFiles(_folderPathToSearch, "*.mp3");
                _tempList.AddRange(_files);
                _files = Directory.GetFiles(_folderPathToSearch, "*.wav");
                _tempList.AddRange(_files);

                if (_tempList.Count() < 1)
                {
                    CCM_logSource.LogError("CCM_fnc_parseConfig: _fn_getFileNamesAtPath: File path " + _folderPathToSearch + " returned no files.");
                }
                else
                {
                    // get only the file names for returns
                    string _tempFileName;
                    foreach (string _filePath in _tempList)
                    {
                        _tempFileName = Path.GetFileName(_filePath).ToLower();
                        _returnList.Add(_tempFileName);
                    }
                }
            }

            return _returnList;
        }














        /* This is how you build your own "types"
        class CCM_track
        {
            string name { get; set; }
            AudioClip Clip { get; set; }
            CCM_trackTypes_enum _type { get; set; }
        }
        */
    }
}













// random between two ints
/*
    Random r = new Random();
	int rInt = r.Next(0, 100); //for ints
	Console.WriteLine(rInt);
*/















/*
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Change this to match your program's normal namespace
namespace MyProg
{
    class IniFile   // revision 11
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
*/