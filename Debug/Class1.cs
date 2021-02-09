using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Photon;
using Photon.Realtime;

namespace MyDebug
{
    // Don't forget this attribute. The ID, NAME and VERSION are your const values below.
    [BepInPlugin(ID, NAME, VERSION)]
    public class MyMod : BaseUnityPlugin // Rename "MyMod"
    {
        const string ID = "com.author.project"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "My Mod Name";
        const string VERSION = "1.0";

        // folder paths for user defined music
        internal const string CCM_filePathStart = "file://";

        // lists for keeping track of already played music to avoid playing it again frequently if possible
        // these will contain file names for the music
        internal static List<string> CCM_usedCombatTracks = new List<string>();
        internal static List<string> CCM_usedAmbientNightTracks = new List<string>();
        internal static List<string> CCM_usedAmbientDayTracks = new List<string>();
        internal static List<string> CCM_usedTownTracks = new List<string>();
        internal static List<string> CCM_usedDungeonTracks = new List<string>();

        // these will contain unused file names of music
        internal static List<string> CCM_combatTracks = new List<string>();
        internal static List<string> CCM_ambientNightTracks = new List<string>();
        internal static List<string> CCM_ambientDayTracks = new List<string>();
        internal static List<string> CCM_townTracks = new List<string>();
        internal static List<string> CCM_dungeonTracks = new List<string>();

        // folder path constants
        internal static readonly string CCM_mainFolderPath = Path.GetFullPath(@"Mods\CCM Project");
        internal static readonly string CCM_combatFolderPath = Path.GetFullPath(CCM_mainFolderPath + @"\Combat Tracks");
        internal static readonly string CCM_ambientNightFolderPath = Path.GetFullPath(CCM_mainFolderPath + @"\Ambient Night Tracks");
        internal static readonly string CCM_ambientDayFolderPath = Path.GetFullPath(CCM_mainFolderPath + @"\Ambient Day Tracks");
        internal static readonly string CCM_townFolderPath = Path.GetFullPath(CCM_mainFolderPath + @"\Town Tracks");
        internal static readonly string CCM_dungeonFolderPath = Path.GetFullPath(CCM_mainFolderPath + @"\Dungeon Tracks");

        internal enum CCM_trackTypes_enum
        {
            combat,
            ambientNight,
            ambientDay,
            town,
            dungeon
        };

        internal int CCM_currentTrackType = -1;

        // a list for storing the combat music object names as strings
        // this is to be able to detect when one is created
        internal static List<string> CCM_combatMusicList = new List<string>();

        // used for controlling the loop in CCM_fnc_startCombatMusicIntercept
        internal static bool CCM_doRunCombatMusicCheck = true;

        // used to keep track of each player's' current scene. dictionary is global and synced between all players
        // this is so that if a player is first in the scene, they will define what the track is to everyone else who enters the scene after
        public static Dictionary<PhotonPlayer, string> CCM_dictionary_activePlayerScenes = new Dictionary<PhotonPlayer, string>();

        // used to keep track of each active scenes music track
        public static Dictionary<Scene, string> CCM_dictionary_activeScenesCurrentMusic = new Dictionary<Scene, string>();

        // music game objects we will use to actually play music
        public GameObject CCM_musicHandler_1 = new GameObject("CCM_musicHandler_1");
        public GameObject CCM_musicHandler_2 = new GameObject("CCM_musicHandler_2");

        // this bool keeps track of CCM_musicHandler_1 & CCM_musicHandler_2
        // they need to be assigned the properties of a BGM (Background Music) game object
        // this is done (ideally) on the first run of CCM_fnc_findMainMusicObject
        public bool CCM_gameObjectPropsAssigned = false;


        public static System.Random CCM_getRandom = new System.Random();

        internal void Awake()
        {
            Logger.Log(LogLevel.Message, "...............");
            Logger.Log(LogLevel.Message, CCM_mainFolderPath);
            var _doesExist = Directory.Exists(CCM_townFolderPath);

            Logger.Log(LogLevel.Message, _doesExist);
        }
    }
}
