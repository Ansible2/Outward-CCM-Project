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
        internal const string CCM_configFileName = @"CCM Config.xml";

        // true when currently loading an audio file
        internal static bool CCM_loadingAudio = false;

        internal static void CCM_fnc_parseConfig()
        {
            string _pathToConfig = Path.Combine(CCM_mainFolderPath, CCM_configFileName);
            
            if (File.Exists(_pathToConfig))
            {
                var _xmlConfigFile = XDocument.Load(_pathToConfig);
                _fn_buildAudioClipLibrary(_xmlConfigFile);
                _fn_grabTrackSettings(_xmlConfigFile);
            }
            else
            {
                CCM_logSource.Log(LogLevel.Fatal, "Config file could not be found at: " + _pathToConfig);
            }
        }

        /* ----------------------------------------------------------------------------
            _fn_buildAudioClipLibrary
        ---------------------------------------------------------------------------- */
        private static void _fn_buildAudioClipLibrary(XDocument _xmlConfigFile)
        {
            var _list = _xmlConfigFile.Root.Descendants("tracks");

            foreach (var _x in _list)
            {
                var _fileName = _x.Element("filename").Value;
                if (_fn_doesFileExist(_fileName))
                {
                    // collects all configed track types for the track
                    var _trackTypes = _x.Element("track_types").Descendants("track_type").ToList();

                    if (_trackTypes.Count() != 0)
                    {
                        foreach (var _y in _trackTypes)
                        {
                            _fn_pushBackToTrackList(_y.Value, _fileName);
                        }

                        CCM_Instance.StartCoroutine(_fn_loadAndStoreAudioClip(_fileName));
                    }
                    else
                    {
                        CCM_logSource.LogError("Did not find any track types for file: " + _fileName + " within " + CCM_configFileName);
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
        private static void _fn_grabTrackSettings(XDocument _xmlConfigFile)
        {
            var _trackSpacings = _xmlConfigFile.Root.Descendants("track_spacing");
            foreach (var _x in _trackSpacings)
            {
                //int _val = (int)_x;
                //int _value = int.Parse(_x.Value);
            }
        }

        /* ----------------------------------------------------------------------------
           _fn_loadAndStoreAudioClip
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_loadAndStoreAudioClip(string _filename)
        {
            var _pathToFile = Path.Combine(CCM_filePathStart, CCM_tracksFolderPath, _filename);
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

                CCM_dictionary_audioClipFromString.Add(_filename, _clip);
            }
            
            yield break;
        }

        /* ----------------------------------------------------------------------------
           _fn_doesFileExist
        ---------------------------------------------------------------------------- */
        private static bool _fn_doesFileExist(string _filename)
        {
            var _pathToFile = Path.Combine(CCM_tracksFolderPath, _filename);

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
                        CCM_townDayTracks.Add(_filename);
                        break;
                    }
                case "townnight":
                    {
                        CCM_townNightTracks.Add(_filename);
                        break;
                    }
                case "ambientday":
                    {
                        CCM_ambientDayTracks.Add(_filename);
                        break;
                    }
                case "ambientnight":
                    {
                        CCM_ambientNightTracks.Add(_filename);
                        break;
                    }
                case "dungeon":
                    {
                        CCM_dungeonTracks.Add(_filename);
                        break;
                    }
                case "combat":
                    {
                        CCM_combatTracks.Add(_filename);
                        break;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("_fn_pushBackToList: Encountered unknown track type: " + _trackType);
                        break;
                    }
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