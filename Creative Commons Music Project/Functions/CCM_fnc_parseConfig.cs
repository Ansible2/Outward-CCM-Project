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

namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal const string CCM_configFileName = "CCM Config.xml";

        internal static void CCM_fnc_parseConfig()
        {
            string _pathToConfig = Path.Combine(CCM_mainFolderPath, CCM_configFileName);
            
            if (!File.Exists(_pathToConfig))
            {
                CCM_logSource.Log(LogLevel.Fatal, "Config file could not be found at: " + _pathToConfig);
            }
            else
            {
                var _xmlConfigFile = XDocument.Load(_pathToConfig);

                
            }
            
        }

        private static void _fn_buildAudioClipLibrary(XDocument _xmlConfigFile)
        {
            var _list = _xmlConfigFile.Root.Descendants("tracks");

            foreach (var _x in _list)
            {
                var _fileName = _x.Element("filename").Value;
                var _trackTypes = _x.Element("track_types").Descendants("track_type");
                
                foreach (var _y in _trackTypes)
                {
                    _fn_pushBackToTrackList(_y.Value, _fileName);
                }


                //CCM_fnc_logWithTime();
            }
        }

        private static void _fn_grabTrackSettings(XDocument _xmlConfigFile)
        {
            var _list = _xmlConfigFile.Root.Descendants("track_spacing");
        }

        private static void _fn_loadAudioClip(string _filename)
        {

        }

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
                        break;
                    }
                case "ambientday":
                    {
                        break;
                    }
                case "ambientnight":
                    {
                        break;
                    }
                case "dungeon":
                    {
                        break;
                    }
                case "combat":
                    {
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