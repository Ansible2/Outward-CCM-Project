
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;


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
        const string VERSION = "0.9.2.1";

        // used for running static coroutines
        internal static CCM_core CCM_Instance;

        /* ------------------------------------------------------------------------
            Lists
        ------------------------------------------------------------------------ */
        internal static class CCM_Lists
        {
            // lists for keeping track of already played music to avoid playing it again frequently if possible
            // these will contain file names for the music
            internal static List<CCM_track> used_combatTracks = new List<CCM_track>();
            internal static List<CCM_track> used_ambientNightTracks = new List<CCM_track>();
            internal static List<CCM_track> used_ambientDayTracks = new List<CCM_track>();
            internal static List<CCM_track> used_townDayTracks = new List<CCM_track>();
            internal static List<CCM_track> used_townNightTracks = new List<CCM_track>();
            internal static List<CCM_track> used_dungeonTracks = new List<CCM_track>();

            // these will contain unused file names of music
            internal static List<CCM_track> unused_combatTracks = new List<CCM_track>();
            internal static List<CCM_track> unused_ambientNightTracks = new List<CCM_track>();
            internal static List<CCM_track> unused_ambientDayTracks = new List<CCM_track>();
            internal static List<CCM_track> unused_townDayTracks = new List<CCM_track>();
            internal static List<CCM_track> unused_townNightTracks = new List<CCM_track>();
            internal static List<CCM_track> unused_dungeonTracks = new List<CCM_track>();
        }


        /* ------------------------------------------------------------------------
            Dictionaries
        ------------------------------------------------------------------------ */
        internal static class CCM_Dictionaries
        {
            internal static Dictionary<CCM_trackTypes_enum, List<int>> trackSpacingFromType = new Dictionary<CCM_trackTypes_enum, List<int>>();

            internal static Dictionary<string, int> trackLengthFromString = new Dictionary<string, int>();
        }

        /// <summary>
        /// An abstraction for storing information about the individual tracks a player has loaded
        /// </summary>
        internal class CCM_track
        {
            internal string Filename { get; }
            internal int Length { get; set; }
            internal CCM_trackTypes_enum FolderType { get; }

            internal CCM_track(string filename, CCM_trackTypes_enum folderType)
            {
                Filename = filename;
                FolderType = folderType;
                Length = 0;
            }
            internal CCM_track(string filename, CCM_trackTypes_enum folderType, int length)
            {
                Filename = filename;
                FolderType = folderType;
                Length = length;
            }
            internal CCM_track() 
            {
                Filename = "";
                FolderType = CCM_trackTypes_enum.EMPTY;
                Length = -1;
            }
        }


        /* ------------------------------------------------------------------------
            String Constants
        ------------------------------------------------------------------------ */
        /// <summary>
        /// Keeps track of the many constant directory paths CCM can use
        /// </summary>
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


        /* ------------------------------------------------------------------------
            Misc
        ------------------------------------------------------------------------ */
        /// <summary>
        /// The current scene name occupied by the player
        /// </summary>
        internal static string CCM_currentSceneName;

        /// <summary>
        /// The current scenes track type (e.g. is combat music playing or night town music or day town music, etc.)
        /// </summary>
        internal static CCM_trackTypes_enum CCM_currentTrackType = CCM_trackTypes_enum.EMPTY;

        /// <summary>
        /// Keeps track of the currently running music routine loop
        /// </summary>
        internal static Coroutine CCM_currentRoutine;

        /// <summary>
        /// Keeps track of what CCM_track is currently playing
        /// </summary>
        internal static CCM_track CCM_currentTrack;

        internal enum CCM_trackTypes_enum
        {
            ambientDay,
            ambientNight,
            ambient,
            combat,
            configed,
            day,
            dungeon,
            night,
            townDay,
            townNight,
            town,
            EMPTY
        };

        internal static string CCM_currentTrackFilename = "";

        internal static System.Random CCM_getRandom = new System.Random();

        internal static bool CCM_syncOnline;

        internal static float CCM_musicVolume;

        internal static bool CCM_loadingAudio = false;

        internal static bool CCM_choosingTrackForScene;

        internal static bool CCM_trackLengthLoadComplete = false;

        internal static bool CCM_createdMusicHandlers = false;



        /* ------------------------------------------------------------------------
            Music Handlers
        ------------------------------------------------------------------------ */
        /// <summary>
        /// Used for keeping track of the various states and properties for the two music GameObjects used by CCM
        /// </summary>
/*  
        internal static class CCM_MusicHandlers
        {
            // music game objects we will use to actually play music
            internal static GameObject musicHandler_1;
            internal static GameObject musicHandler_2;
            internal static AudioSource musicAudioSource_1;
            internal static AudioSource musicAudioSource_2;

            internal static uint CCM_fadeId_1 = 0;
            internal static uint CCM_fadeId_2 = 0;

            internal static bool musicAudioSource_1_isFading = false;
            internal static bool musicAudioSource_2_isFading = false;

            internal static bool musicAudioSource_1_stopFading = false;
            internal static bool musicAudioSource_2_stopFading = false;

            /// <summary>
            /// Checks if the two music handlers are fully Instantiated and ready to play music
            /// </summary>
            internal static bool handlersInstantiated = false;

            // this keeps track of which music handler is actually currently intended to be played on
            // for instance, when transitioning to a new track, this one
            internal static GameObject nowPlayingMusicHandler;
            internal static AudioSource nowPlayingAudioSource;
        }
*/
        internal class CCM_MusicHandler
        {
            internal GameObject musicHandler;
            internal AudioSource audioSource;
            internal string name;
            internal uint fadeCount;
            internal bool isFading { get; set; }
            internal bool stopFading { get; set; }
            //internal bool nowPlaying { get; set; }

            internal CCM_MusicHandler(string nameOf, GameObject objectToCopy)
            {
                musicHandler = Instantiate(objectToCopy);
                DontDestroyOnLoad(musicHandler);
                musicHandler.name = nameOf + "_GameObject";

                audioSource = musicHandler.GetComponent<AudioSource>();
                audioSource.loop = false;
                audioSource.volume = 0;

                name = nameOf;

                isFading = false;
                stopFading = false;
                //nowPlaying = false;
                fadeCount = 0;
            }
        }

        internal static CCM_MusicHandler CCM_MusicHandler_1;
        internal static CCM_MusicHandler CCM_MusicHandler_2;
        internal static CCM_MusicHandler CCM_nowPlayingMusicHandler;
        internal static CCM_MusicHandler CCM_lastUsedHandler;

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
            CCM_fnc_instantiateHarmony
        ------------------------------------------------------------------------ */
        /// <summary>
        /// Adds in the patch to the vanilla function that allows CCM_event_onVanillaMusicPlayed to execute
        /// </summary>
        private static void CCM_fnc_instantiateHarmony()
        {
            var harmony = new Harmony("com.Ansible2.CCM");
            harmony.PatchAll();
        }

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
*/