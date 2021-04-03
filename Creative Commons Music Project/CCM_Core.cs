using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames;
using HarmonyLib;
using UnityEngine.Networking;
using Photon;
using Photon.Realtime;


namespace creativeCommonsMusicProject
{
    [BepInPlugin(ID, NAME, VERSION)]

    internal partial class CCM_core : BaseUnityPlugin
    {
        /* ------------------------------------------------------------------------
        
            Prepare Globals

        ------------------------------------------------------------------------ */
        const string ID = "com.Ansible2.CCMProject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        // used for running static coroutines
        internal static CCM_core CCM_Instance;

        /* ------------------------------------------------------------------------
            Lists
        ------------------------------------------------------------------------ */
        internal static class CCM_Lists
        {
            // lists for keeping track of already played music to avoid playing it again frequently if possible
            // these will contain file names for the music
            internal static List<string> used_combatTracks = new List<string>();
            internal static List<string> used_ambientNightTracks = new List<string>();
            internal static List<string> used_ambientDayTracks = new List<string>();
            internal static List<string> used_townDayTracks = new List<string>();
            internal static List<string> used_townNightTracks = new List<string>();
            internal static List<string> used_dungeonTracks = new List<string>();

            // these will contain unused file names of music
            internal static List<string> unused_combatTracks = new List<string>();
            internal static List<string> unused_ambientNightTracks = new List<string>();
            internal static List<string> unused_ambientDayTracks = new List<string>();
            internal static List<string> unused_townDayTracks = new List<string>();
            internal static List<string> unused_townNightTracks = new List<string>();
            internal static List<string> unused_dungeonTracks = new List<string>();

            // keeps track of the music routines running for each scene to schedule music 
            internal static List<string> scenesWithMusicRoutines = new List<string>();

            internal static List<string> storedTracks = new List<string>();

            internal static List<string> scenesChoosingMusicFor = new List<string>();
        }


        /* ------------------------------------------------------------------------
            Dictionaries
        ------------------------------------------------------------------------ */
        internal static class CCM_Dictionaries
        {
            // used to keep track of each player's' current scene. dictionary is global and synced between all players
            // this is so that if a player is first in the scene, they will define what the track is to everyone else who enters the scene after
            internal static Dictionary<int, string> activePlayerScenes = new Dictionary<int, string>();

            // used to keep track of each active scenes music track
            // layout is scene/track
            internal static Dictionary<string, string> activeScenesCurrentMusic = new Dictionary<string, string>();

            // Music Routine objects
            internal static Dictionary<string, Coroutine> sceneRoutines = new Dictionary<string, Coroutine>();

            // keeps track of the currently playing music type for each scene that is active
            internal static Dictionary<string, int> activeScenesTrackType = new Dictionary<string, int>();

            // This is used locally for each machine to take a given filename and get back the audioClip of the file
            internal static Dictionary<string, AudioClip> audioClipFromString = new Dictionary<string, AudioClip>();

            internal static Dictionary<int, List<int>> trackSpacingFromType = new Dictionary<int, List<int>>();
        }


        /* ------------------------------------------------------------------------
            String Constants
        ------------------------------------------------------------------------ */
        internal static class CCM_Paths
        {
            internal static readonly string mainFolderPath = Path.GetFullPath(@"Mods\CCM Project");
            internal static readonly string tracks_folderPath = Path.Combine(mainFolderPath + @"\Tracks");
            
            internal static readonly string ambientNight_folderPath = Path.Combine(mainFolderPath + @"\Ambient Night Tracks");
            internal static readonly string townNight_folderPath = Path.Combine(mainFolderPath + @"\Town Night Tracks");

            internal static readonly string ambientDay_folderPath = Path.Combine(mainFolderPath + @"\Ambient Day Tracks");
            internal static readonly string townDay_folderPath = Path.Combine(mainFolderPath + @"\Town Day Tracks");

            internal static readonly string combat_folderPath = Path.Combine(mainFolderPath + @"\Combat Tracks");
            internal static readonly string dungeon_folderPath = Path.Combine(mainFolderPath + @"\Dungeon Tracks");

            // used for merely sorting in CCM_fnc_parseConfig into multiple subs categories above
            internal static readonly string night_folderPath = Path.Combine(mainFolderPath + @"\Night Tracks");
            internal static readonly string day_folderPath = Path.Combine(mainFolderPath + @"\Day Tracks");
            internal static readonly string ambient_folderPath = Path.Combine(mainFolderPath + @"\Ambient Tracks");
            internal static readonly string town_folderPath = Path.Combine(mainFolderPath + @"\Town Tracks");

            internal const string FILE_PREFIX = "file://";
        }

        // used for naming scheme on music-routine objects
        internal static readonly string CCM_musicRoutinePostfixString = "-MusicRoutineObject";


        /* ------------------------------------------------------------------------
            Misc
        ------------------------------------------------------------------------ */
        // self explanitory
        internal static Scene CCM_currentScene;

        internal enum CCM_trackTypes_enum
        {
            combat,
            ambientNight,
            ambientDay,
            townNight,
            townDay,
            dungeon,
            town,
            ambient,
            day,
            night
        };
        // for keeping track of the CCM_trackTypes_enum
        internal static int CCM_currentTrackType = -1;

        internal static System.Random CCM_getRandom = new System.Random();

        internal static ManualLogSource CCM_logSource = BepInEx.Logging.Logger.CreateLogSource("CCM_project");

        internal static bool CCM_syncOnline;

        internal static float CCM_musicVolume;

        internal static bool CCM_loadingAudio = false;


        /* ------------------------------------------------------------------------
            Music Handlers
        ------------------------------------------------------------------------ */
        internal static class CCM_MusicHandlers
        {
            // music game objects we will use to actually play music
            internal static GameObject musicHandler_1;
            internal static GameObject musicHandler_2;
            internal static AudioSource musicAudioSource_1;
            internal static AudioSource musicAudioSource_2;

            internal static bool musicAudioSource_1_isFading = false;
            internal static bool musicAudioSource_2_isFading = false;

            internal static bool musicAudioSource_1_stopFading = false;
            internal static bool musicAudioSource_2_stopFading = false;

            //
            internal static bool handlersInstantiated = false;

            // this keeps track of which music handler is actually currently intended to be played on
            // for instance, when transitioning to a new track, this one
            internal static GameObject nowPlayingMusicHandler;
            internal static AudioSource nowPlayingAudioSource;
        }
        
        
        /* ------------------------------------------------------------------------
        
            awake function

        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            CCM_fnc_instantiateHarmony();

            DontDestroyOnLoad(this.gameObject);
            this.gameObject.name = "CCM Core GameObject";
            gameObject.AddComponent<CCM_rpc>();
            CCM_Instance = this;

            CCM_fnc_parseConfig();

            SceneManager.sceneLoaded += CCM_event_onSceneChangeStarted;
        }


        /* ------------------------------------------------------------------------
            CCM_fnc_logWithTime
        ------------------------------------------------------------------------ */
        internal static void CCM_fnc_logWithTime(object myMessage)
        {
            CCM_core.CCM_logSource.Log(LogLevel.Message, Time.time + "--: " + myMessage);
        }


        /* ------------------------------------------------------------------------
            CCM_fnc_instantiateHarmony
        ------------------------------------------------------------------------ */
        private static void CCM_fnc_instantiateHarmony()
        {
            var harmony = new Harmony("com.Ansible2.CCM");
            harmony.PatchAll();
        }





        // You don't need to hook into any stop music function (atleast for combat tracks) as it appears there will be another playmusic called to restart the ambient one
        // THIS NEEDS TO BE CONFIRMED IN THE GAMES CODE HOWEVER
        // You have to make sure that you don't need to hook into the QueueMusic function instead to detect this for instance as it might just unmute the other game object instead of calling the playMusic function again










    }


}



// music in game by object name
/*
BGM_OutwardTheme,
BGM_StandardCombat,
BGM_CierzoDay,
BGM_ChersoneseDay,
BGM_Dungeon2,
BGM_StandardCombat2,
BGM_CierzoNight,
BGM_ChersoneseNight,
BGM_DungeonAmbiant,
BGM_DungeonRuins,
BGM_DungeonWild,
BGM_CombatAbrassar,
BGM_CombatChersonese,
BGM_CombatEnmerkar,
BGM_CombatHallowedMarsh,
BGM_EventDanger,
BGM_EventDramatic,
BGM_EventFriendly,
BGM_RegionAbrassar,
BGM_RegionAntiqueFields,
BGM_RegionChersonese,
BGM_RegionChersoneseNIGHT,
BGM_RegionEnmerkarForest,
BGM_RegionHallowedMarsh,
BGM_RegionHallowedMarshNIGHT,
BGM_RegionKarburan,
BGM_TownBerg,
BGM_TownBergNIGHT,
BGM_TownCierzo,
BGM_TownCierzoNIGHT,
BGM_TownHarmattanNIGHT,
BGM_TownKaraburanCity,
BGM_TownMarket,
BGM_TownMonsoon,
BGM_GeneralTitleScreen,
BGM_TownMonsoonNIGHT,
BGM_TownLevant,
BGM_TownLevantNIGHT,
BGM_CombatBoss,
BGM_CombatMiniboss,
BGM_RegionEnmerkarForestNIGHT,
BGM_RegionAbrassarNIGHT,
BGM_DungeonManmade,
BGM_EventQuest,
BGM_Empty,
BGM_CombatAntiquePlateau,
BGM_CombatBossDLC1,
BGM_CombatDungeonAntique,
BGM_CombatDungeonFactory,
BGM_CombatMinibossDLC1,
BGM_DungeonAntique,
BGM_DungeonFactory,
BGM_EventMystery,
BGM_RegionAntiquePlateau,
BGM_RegionAntiquePlateauNIGHT,
BGM_TownHarmattan

//// music types:

// region
// region night
// town 
// town night
// combat
// dungeon
// Cierzo
// Cierzo night
// Chersonese
// Chersonese night
*/


/*
Logger.Log(LogLevel.Message, "Waiting for sync");
AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.name);
while (!asyncLoad.isDone)
{
    yield return null;
}
Logger.Log(LogLevel.Message, "Sync done");
*/