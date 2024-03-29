﻿/* ----------------------------------------------------------------------------
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

        static List<CCM_track> _filesToLoad = new List<CCM_track>();
        static List<string> _checkedFiles = new List<string>();

        /* ----------------------------------------------------------------------------
            CCM_fnc_parseConfig
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Initiates the configuration of CCM Config.xml Fills all the detail lists and dictionaries about the tracks
        ///</summary>
        internal static void CCM_fnc_parseConfig()
        {
            string _pathToConfig = Path.Combine(CCM_Paths.mainFolderPath, CCM_configFileName);
            
            if (File.Exists(_pathToConfig))
            {
                var _xmlConfigFile = XDocument.Load(_pathToConfig);
                _fn_getLogMode(_xmlConfigFile, _pathToConfig);

                _fn_getFilesFromConfig(_xmlConfigFile);
                _fn_getFilesFromFolders();

                CCM_Instance.StartCoroutine(_fn_getAllFileTrackLengths());


                _fn_grabTrackSpacingSettings(_xmlConfigFile);
                _fn_getOnlineMode(_xmlConfigFile, _pathToConfig);
            }
            else
            {
                CCM_fnc_log.fatal("CCM_fnc_parseConfig: Config file could not be found at: " + _pathToConfig);
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_getAllFileTrackLengths
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Initiates the loading of all track files (temporarily) into memory in order to retrieve their track lengths
        ///</summary>
        private static IEnumerator _fn_getAllFileTrackLengths()
        {
            CCM_fnc_log.WithTime.message("CCM_fnc_parseConfig: _fn_getAllFileTrackLengths: Started audio load...");
            
            foreach (var _track in _filesToLoad)
            {
                while (CCM_loadingAudio)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }

                CCM_fnc_log.info("CCM_fnc_parseConfig: _fn_getAllFileTrackLengths: Starting a new load...");

                CCM_loadingAudio = true;

                CCM_Instance.StartCoroutine(_fn_getTrackLength(_track));
            }

            _filesToLoad = null;
            _checkedFiles = null;

            CCM_core.CCM_trackLengthLoadComplete = true;
            CCM_fnc_log.WithTime.message("CCM_fnc_parseConfig: _fn_getAllFileTrackLengths: Completed audio load");
        }


        /* ----------------------------------------------------------------------------
            _fn_getFilesFromConfig
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Reads tracks that are configured inside the CCM Config.xml and places them into their corresponding track lists. Similar to _fn_getFilesFromFolders
        ///</summary>
        private static void _fn_getFilesFromConfig(XDocument _xmlConfigFile)
        {

            var _list = _xmlConfigFile.Root.Descendants("tracks").Elements();

            if (_list.Count() == 0)
            {
                CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getFilesFromConfig: No tracks were located in config file: " + CCM_configFileName);
            }
        
            foreach (var _x in _list)
            {
                var _filename = _x.Element("filename").Value.ToLower().Trim();

                // make sure provided filename is actually in the tracks folder
                if (_fn_doesFileExist(_filename, CCM_Paths.tracks_folderPath))
                {
                    // check for duplicates
                    if (_checkedFiles.Contains(_filename))
                    {
                        CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getFilesFromConfig: Configed track: " + _filename + " within " + CCM_configFileName + " is a duplicate!");
                    }
                    else
                    {
                        // create ccm_track
                        CCM_track _track = new CCM_track(_filename, CCM_trackTypes_enum.configed);

                        // collects all configed track types for the track
                        var _trackTypes = _x.Element("track_types").Descendants("track_type").ToList();
                        
                        if (_trackTypes.Count() > 0)
                        {
                            // add to track type lists
                            foreach (var _y in _trackTypes)
                            {
                                _fn_pushBackToTrackList(_y.Value, _track);
                            }

                            _filesToLoad.Add(_track);
                            _checkedFiles.Add(_filename);
                        }
                        else
                        {
                            CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getFilesFromConfig: Did not find any track types for file: " + _filename + " within " + CCM_configFileName);
                        }
                    }
                }
                else
                {
                    CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getFilesFromConfig: Did not find track file: " + _filename + " within tracks folder!");
                }
            }

        }


        /* ----------------------------------------------------------------------------
           _fn_getFilesFromFolders
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Reads tracks that are NOT configured inside the CCM Config.xml but are simply in folders and places them into their corresponding track lists. Similar to _fn_getFilesFromConfig
        ///</summary>
        private static void _fn_getFilesFromFolders()
        {
            // create an array of CCM_trackTypes_enum so we can use foreach for loop through
            CCM_trackTypes_enum[] _trackTypes = (CCM_trackTypes_enum[])Enum.GetValues(typeof(CCM_trackTypes_enum));

            List<string> _fileNames;

            foreach (var _trackType in _trackTypes)
            {
                if (_trackType != CCM_trackTypes_enum.configed && _trackType != CCM_trackTypes_enum.EMPTY) // don't do XML configed tracks here
                {
                    string _folderPath = CCM_fnc_getTrackTypeFolderPath(_trackType);
                    _fileNames = _fn_getFileNamesAtPath(_folderPath);

                    if (_fileNames.Count() == 0)
                    {
                        CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Folder path " + _folderPath + " returned no files!");
                    }
                    else
                    {
                        
                        // get only the file names for returns
                        foreach (string _filename in _fileNames)
                        {
                            if (!_checkedFiles.Contains(_filename))
                            {
                                CCM_track _track = new CCM_track(_filename, _trackType);
                                _fn_pushBackToTrackLists(_trackType, _track);
                                
                                _filesToLoad.Add(_track);
                                _checkedFiles.Add(_filename);
                            }
                            else
                            {
                                CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Found that track: " + _filename + " was already loaded in another location, throwing away duplicate...");
                            }
                        }
                    }
                }              
            }



            // Do any remaining tracks that were not configed in the XML file but are in the "tracks" folder
            // Any tracks in this category are considered to be a track that can play anywhere
            string _folderPathToSearch = CCM_Paths.tracks_folderPath;            
            _fileNames = _fn_getFileNamesAtPath(_folderPathToSearch);
            
            if (_fileNames.Count() == 0)
            {
                CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Folder path " + _folderPathToSearch + " returned no files!");
            }
            else
            {
                // get only the file names for returns
                foreach (string _filename in _fileNames)
                {
                    if (!_checkedFiles.Contains(_filename))
                    {
                        CCM_track _track = new CCM_track(_filename, CCM_trackTypes_enum.configed);
                        _fn_pushBackToTrackList("all", _track);
                        
                        _filesToLoad.Add(_track);
                        _checkedFiles.Add(_filename);
                    }
                    else
                    {
                        CCM_fnc_log.warning("CCM_fnc_parseConfig: _fn_getAudioClipsFromFolders: Found that track: " + _filename + " was already loaded in another location, throwing away duplicate...");
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

            CCM_fnc_log.info(_trackSpacings);

            foreach (var _x in _trackSpacings)
            {
                bool _doAdd = true;

                string _name = _x.Name.ToString().ToLower();

                CCM_trackTypes_enum _trackType = CCM_trackTypes_enum.EMPTY;
                
                int _min = (int)_x.Element("min");
                int _max = (int)_x.Element("max");

                switch (_name)
                {
                    case ("trackspacing_townday"):
                        {
                            _trackType = CCM_trackTypes_enum.townDay;
                            break;
                        }
                    case ("trackspacing_townnight"):
                        {
                            _trackType = CCM_trackTypes_enum.townNight;
                            break;
                        }
                    case ("trackspacing_ambientday"):
                        {
                            _trackType = CCM_trackTypes_enum.ambientDay;
                            _doAdd = true;
                            break;
                        }
                    case ("trackspacing_ambientnight"):
                        {
                            _trackType = CCM_trackTypes_enum.ambientNight;
                            break;
                        }
                    case ("trackspacing_dungeon"):
                        {
                            _trackType = CCM_trackTypes_enum.dungeon;
                            break;
                        }
                    case ("trackspacing_combat"):
                        {
                            _trackType = CCM_trackTypes_enum.combat;
                            break;
                        }
                    default:
                        {
                            _doAdd = false;
                            CCM_fnc_log.error("CCM_fnc_parseConfig: _fn_grabTrackSpacingSettings: Encountered unkown config for track spacing: " + _name);
                            break;
                        }
                }

                if (_doAdd)
                {
                    CCM_fnc_log.info("CCM_fnc_parseConfig: _fn_grabTrackSpacingSettings: Added min " + _min + " and max " + _max + " to " + _name);
                    List<int> _list = new List<int>() { _min, _max };
                    CCM_Dictionaries.trackSpacingFromType.Add(_trackType, _list);
                }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_getTrackLength    (AudioClips are loaded at the start and stored due to a need to know their duration in order to queue songs for the future. 
                                        I could've used another library for this, but it is the smoothest for development at the moment.
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Gets the length of a given CCM_track (which is basically just the track file's details)
        ///</summary>
        private static IEnumerator _fn_getTrackLength(CCM_track _track)
        {
            var _folderPath = CCM_fnc_getTrackTypeFolderPath(_track.FolderType);
            
            string _filename = _track.Filename;
            var _pathToFile = Path.Combine(CCM_Paths.FILE_PREFIX, _folderPath, _track.Filename);
            
            AudioType _audioType = CCM_fnc_getAudioTypeFromString(_filename);
            
            using (UnityWebRequest _request = UnityWebRequestMultimedia.GetAudioClip(_pathToFile, _audioType))
            {
                ((DownloadHandlerAudioClip)_request.downloadHandler).streamAudio = true;
                _request.SendWebRequest();

                while (!_request.isDone)
                {
                    yield return new WaitForSecondsRealtime(0.01f);
                }

                CCM_fnc_log.info("CCM_fnc_parseConfig: _fn_getTrackLength: Web request is done for " + _filename);

                if (_request.error != null)
                {
                    CCM_fnc_log.error("CCM_fnc_parseConfig: _fn_getTrackLength: Web request encountered the following error: " + _request.error);
                    yield break;
                }

                var _clip = DownloadHandlerAudioClip.GetContent(_request);
                _track.Length = (int)_clip.length;
                if (_track.Length == 0)
                {
                    CCM_fnc_log.error("CCM_fnc_parseConfig: " + _filename + " has a track length of " + _track.Length + ". This track will not be added to the list rotation as a length of 0 causes issues.");
                }
                else
                {
                    CCM_fnc_log.info("CCM_fnc_parseConfig: " + _filename + " has a track length of " + _track.Length);
                    CCM_Dictionaries.trackLengthFromString.Add(_filename, _track.Length);
                }
                              

                _clip.UnloadAudioData();
                Destroy(_clip);

                CCM_loadingAudio = false;
            }
            
            yield break;
        }


        /* ----------------------------------------------------------------------------
           _fn_doesFileExist
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Checks if a given filename is present at a given path
        ///</summary>
        private static bool _fn_doesFileExist(string _filename, string _folderPath)
        {
            var _pathToFile = Path.Combine(_folderPath, _filename);

            bool _doesFileExist = false;
            if (File.Exists(_pathToFile))
            {
                _doesFileExist = true;
            }

            CCM_fnc_log.message("CCM_fnc_parseConfig: _fn_doesFileExist: Checked file path " + _pathToFile + " : Does exist? -> " + _doesFileExist);

            return _doesFileExist;
        }


        /* ----------------------------------------------------------------------------
           _fn_pushBackToTrackList
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Places a given CCM_track into the corresponding unused track lists based upon a given track type string
        ///</summary>
        // string _trackType overload is used for XML configed tracks
        private static void _fn_pushBackToTrackList(string _trackType, CCM_track _track)
        {
            _trackType = _trackType.ToLower().Trim();

            switch (_trackType)
            {
                case "townday":
                    {
                        CCM_Lists.unused_townDayTracks.Add(_track);
                        break;
                    }
                case "townnight":
                    {
                        CCM_Lists.unused_townNightTracks.Add(_track);
                        break;
                    }
                case "ambientday":
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_track);
                        break;
                    }
                case "ambientnight":
                    {
                        CCM_Lists.unused_ambientNightTracks.Add(_track);
                        break;
                    }
                case "dungeon":
                    {
                        CCM_Lists.unused_dungeonTracks.Add(_track);
                        break;
                    }
                case "combat":
                    {
                        CCM_Lists.unused_combatTracks.Add(_track);
                        break;
                    }
                case "all":
                    {
                        CCM_Lists.unused_townDayTracks.Add(_track);
                        CCM_Lists.unused_townNightTracks.Add(_track);
                        CCM_Lists.unused_ambientDayTracks.Add(_track);
                        CCM_Lists.unused_ambientNightTracks.Add(_track);
                        CCM_Lists.unused_dungeonTracks.Add(_track);
                        CCM_Lists.unused_combatTracks.Add(_track);
                        break;
                    }
                default:
                    {
                        CCM_fnc_log.error("_fn_pushBackToList: Encountered unknown track type: " + _trackType);
                        break;
                    }
            }
        }

        ///<summary>
        /// Places a given CCM_track into the corresponding unused track lists based upon a given CCM_trackTypes_enum
        ///</summary>
        // enum overload is used for sorting clips based on folders used (_fn_getAudioClipsFromFolders)
        private static void _fn_pushBackToTrackLists(CCM_trackTypes_enum _trackType, CCM_track _track)
        {
            switch (_trackType)
            {
                case CCM_trackTypes_enum.townDay:
                    {
                        CCM_Lists.unused_townDayTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.townNight:
                    {
                        CCM_Lists.unused_townNightTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.ambientDay:
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.ambientNight:
                    {
                        CCM_Lists.unused_ambientNightTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.dungeon:
                    {
                        CCM_Lists.unused_dungeonTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.combat:
                    {
                        CCM_Lists.unused_combatTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.ambient:
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_track);
                        CCM_Lists.unused_ambientNightTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.town:
                    {
                        CCM_Lists.unused_townNightTracks.Add(_track);
                        CCM_Lists.unused_townDayTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.day:
                    {
                        CCM_Lists.unused_ambientDayTracks.Add(_track);
                        CCM_Lists.unused_townDayTracks.Add(_track);
                        break;
                    }
                case CCM_trackTypes_enum.night:
                    {
                        CCM_Lists.unused_ambientNightTracks.Add(_track);
                        CCM_Lists.unused_townNightTracks.Add(_track);
                        break;
                    }
                default:
                    {
                        CCM_fnc_log.error("_fn_pushBackToList: Encountered unknown track type enum: " + _trackType);
                        break;
                    }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_getOnlineMode
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Parses the CCM_syncOnline config entry in CCM Config.xml
        ///</summary>
        private static void _fn_getOnlineMode(XDocument _xmlConfigFile, string _pathToConfig)
        {
            CCM_syncOnline = _xmlConfigFile.Root.Element("syncOnline").Value.ToUpper() == "ON";

            XElement syncElement = _xmlConfigFile.Root.Element("syncOnline");
            if (syncElement != null)
            {
                CCM_syncOnline = syncElement.Value.ToUpper() == "ON";
                if (CCM_syncOnline)
                {
                    CCM_fnc_log.info("CCM online synchronization is set to ON");
                }
                else
                {
                    CCM_fnc_log.info("CCM online synchronization is set to OFF");
                }
            }
            else
            {
                CCM_fnc_log.error("CCM syncOnline attribute in CCM Config.xml is undefined, defaulting to OFF");

                _xmlConfigFile.Root.Add(new XElement("syncOnline","OFF"));
                _xmlConfigFile.Save(_pathToConfig);
            }

        }


        /* ----------------------------------------------------------------------------
           _fn_getLogMode
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Parses the CCM_doLog config entry in CCM Config.xml
        ///</summary>
        private static void _fn_getLogMode(XDocument _xmlConfigFile, string _pathToConfig)
        {
            XElement logElement = _xmlConfigFile.Root.Element("log");
            if (logElement != null)
            {
                bool _doLog = logElement.Value.ToUpper() == "ON";
                if (_doLog)
                {
                    CCM_fnc_log.info("CCM IS set to log");
                }
                else
                {
                    CCM_fnc_log.info("CCM is set to NOT log");
                }

                // this is so the logging function can print the messages above before the logging variable is set to false
                CCM_doLog = _doLog;
            } 
            else
            {
                CCM_fnc_log.error("CCM log attribute in CCM Config.xml is undefined, logging will default to ON and XML will be saved with setting");
                
                _xmlConfigFile.Root.Add(new XElement("log", "ON"));
                _xmlConfigFile.Save(_pathToConfig);
            }
            
        }


        /* ----------------------------------------------------------------------------
           _fn_getFilenamesAtPath
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Gets all .ogg files at a given directory
        ///</summary>
        private static List<string> _fn_getFileNamesAtPath(string _folderPathToSearch)
        {
            List<string> _returnList = new List<string>();

            if (!Directory.Exists(_folderPathToSearch))
            {
                CCM_fnc_log.error("CCM_fnc_parseConfig: _fn_getFileNamesAtPath: Folder path " + _folderPathToSearch + " does not exist.");
            }
            else
            {
                // this will get all files of .ogg. However, this includes their paths
                string[] _files = Directory.GetFiles(_folderPathToSearch, "*.ogg");
                List<string> _filesList = _files.ToList();

                if (_filesList.Count() < 1)
                {
                    CCM_fnc_log.error("CCM_fnc_parseConfig: _fn_getFileNamesAtPath: File path " + _folderPathToSearch + " returned no files.");
                }
                else
                {
                    // get only the file names for returns
                    string _tempFileName;
                    foreach (string _filePath in _filesList)
                    {
                        _tempFileName = Path.GetFileName(_filePath).ToLower();
                        _returnList.Add(_tempFileName);
                    }
                }
            }

            return _returnList;
        }

    }
}