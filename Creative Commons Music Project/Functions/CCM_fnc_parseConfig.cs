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


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        const string CCM_configFileName = @"CCM Config.xml";
        // true when currently loading an audio file
        internal static bool CCM_loadingAudio = false;

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
                _fn_storeTrackSettings(_xmlConfigFile);
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
            List<string> _alreadyStoredTracks = new List<string>();

            var _list = _xmlConfigFile.Root.Descendants("tracks");

            foreach (var _x in _list)
            {
                var _fileName = _x.Element("filename").Value.ToLower();
                // make sure provided filename is actually in the tracks folder
                if (_fn_doesFileExist(_fileName))
                {
                    // check for duplicates
                    if (_alreadyStoredTracks.Contains(_fileName))
                    {
                        CCM_logSource.LogError("Configed track: " + _fileName + " within " + CCM_configFileName + " is a duplicate!");
                    }
                    else
                    {
                        // collects all configed track types for the track
                        var _trackTypes = _x.Element("track_types").Descendants("track_type").ToList();

                        if (_trackTypes.Count() != 0)
                        {
                            foreach (var _y in _trackTypes)
                            {
                                _fn_pushBackToTrackList(_y.Value, _fileName);
                            }

                            _alreadyStoredTracks.Add(_fileName);
                            CCM_Instance.StartCoroutine(_fn_loadAndStoreAudioClip(_fileName));
                        }
                        else
                        {
                            CCM_logSource.LogError("Did not find any track types for file: " + _fileName + " within " + CCM_configFileName);
                        }
                    }
                }
                else
                {
                    CCM_logSource.LogError("Did not find track file: " + _fileName + " within tracks folder!");
                }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_grabTrackSettings
        ---------------------------------------------------------------------------- */
        private static void _fn_storeTrackSettings(XDocument _xmlConfigFile)
        {
            var _trackSpacings = _xmlConfigFile.Root.Descendants("track_spacing");

            foreach (var _x in _trackSpacings)
            {
                bool _doAdd = false;
                string _name = _x.Value.ToLower();

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
                            CCM_logSource.LogError("Encountered unkown config for track spacing: " + _name);
                            break;
                        }
                }

                if (_doAdd)
                {
                    List<int> _list = new List<int>() { _min, _max };
                    CCM_Dictionaries.tracKSpacingFromType.Add(_trackType, _list);
                }
            }
        }


        /* ----------------------------------------------------------------------------
           _fn_loadAndStoreAudioClip    (AudioClips are loaded at the start and stored due to a need to know their duration in order to queue songs for the future. 
                                        I could've used another library for this, but it is the smoothest for development and playback though costly for memory)
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_loadAndStoreAudioClip(string _filename)
        {
            var _pathToFile = Path.Combine(CCM_Paths.FILE_PREFIX, CCM_Paths.tracks_folderPath, _filename);
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
                    yield return new WaitForSeconds(0.05f);
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
        private static bool _fn_doesFileExist(string _filename)
        {
            var _pathToFile = Path.Combine(CCM_Paths.tracks_folderPath, _filename);

            bool _doesFileExist = true;
            if (!File.Exists(_pathToFile))
            {
                _doesFileExist = false;
            }
                        
            return _doesFileExist;
        }


        /* ----------------------------------------------------------------------------
           _fn_pushBackToTrackList
        ---------------------------------------------------------------------------- */
        private static void _fn_pushBackToTrackList(string _trackType, string _filename)
        {
            _trackType = _trackType.ToLower();

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
                default:
                    {
                        CCM_fnc_logWithTime("_fn_pushBackToList: Encountered unknown track type: " + _trackType);
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