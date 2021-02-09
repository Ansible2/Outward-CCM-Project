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
        // lists for keeping track of already played music to avoid playing it again frequently if possible
        // these will contain file names for the music
        internal static List<string> CCM_list_usedCombatTracks = new List<string>();
        internal static List<string> CCM_list_usedAmbientNightTracks = new List<string>();
        internal static List<string> CCM_list_usedAmbientDayTracks = new List<string>();
        internal static List<string> CCM_list_usedTownDayTracks = new List<string>();
        internal static List<string> CCM_list_usedTownNightTracks = new List<string>();
        internal static List<string> CCM_list_usedDungeonTracks = new List<string>();

        // these will contain unused file names of music
        internal static List<string> CCM_list_combatTracks = new List<string>();
        internal static List<string> CCM_list_ambientNightTracks = new List<string>();
        internal static List<string> CCM_list_ambientDayTracks = new List<string>();
        internal static List<string> CCM_list_townDayTracks = new List<string>();
        internal static List<string> CCM_list_townNightTracks = new List<string>();
        internal static List<string> CCM_list_dungeonTracks = new List<string>();


        // keeps track of the music routines running for each scene to schedule music 
        internal static List<string> CCM_scenesWithMusicRoutines = new List<string>();

        
        /* ------------------------------------------------------------------------
            Dictionaries
        ------------------------------------------------------------------------ */
        // used to keep track of each player's' current scene. dictionary is global and synced between all players
        // this is so that if a player is first in the scene, they will define what the track is to everyone else who enters the scene after
        internal static Dictionary<int, string> CCM_dictionary_activePlayerScenes = new Dictionary<int, string>();

        // used to keep track of each active scenes music track
        // layout is scene/track
        internal static Dictionary<string, string> CCM_dictionary_activeScenesCurrentMusic = new Dictionary<string, string>();

        // Music Routine objects
        internal static Dictionary<string, GameObject> CCM_dictionary_sceneRoutineObjects = new Dictionary<string, GameObject>();
        
        // keeps track of the currently playing music type for each scene that is active
        internal static Dictionary<string, int> CCM_dictionary_activeScenesTrackType = new Dictionary<string, int>();

        // This is used locally for each machine to take a given filename and get back the audioClip of the file
        internal static Dictionary<string, AudioClip> CCM_dictionary_audioClipFromString = new Dictionary<string, AudioClip>();


        /* ------------------------------------------------------------------------
            String Constants
        ------------------------------------------------------------------------ */
        // folder path constants
        internal static readonly string CCM_mainFolderPath = Path.GetFullPath(@"Mods\CCM Project");
        internal static readonly string CCM_tracksFolderPath = Path.Combine(CCM_mainFolderPath + @"\Tracks");

        /*
        internal static readonly string CCM_combatFolderPath = Path.Combine(CCM_mainFolderPath + @"\Combat Tracks");
        internal static readonly string CCM_ambientNightFolderPath = Path.Combine(CCM_mainFolderPath + @"\Ambient Night Tracks");
        internal static readonly string CCM_ambientDayFolderPath = Path.Combine(CCM_mainFolderPath + @"\Ambient Day Tracks");
        internal static readonly string CCM_townFolderPath = Path.Combine(CCM_mainFolderPath + @"\Town Tracks");
        internal static readonly string CCM_dungeonFolderPath = Path.Combine(CCM_mainFolderPath + @"\Dungeon Tracks");
        */
        // used for naming scheme on music-routine objects
        internal static readonly string CCM_musicRoutinePostfixString = "-MusicRoutineObject";

        // folder paths for user defined music
        internal const string CCM_filePathStart = "file://";


        /* ------------------------------------------------------------------------
            Misc
        ------------------------------------------------------------------------ */
        // this bool keeps track of CCM_musicHandler_1 & CCM_musicHandler_2
        // they need to be assigned the properties of a BGM (Background Music) game object
        // So when they can be instantiated with the same properites it will be set to true
        internal static bool CCM_gameObjectPropsAssigned = false;

        // self explanitory
        internal static Scene CCM_currentScene;

        
        internal enum CCM_trackTypes_enum
        {
            combat,
            ambientNight,
            ambientDay,
            townNight,
            townDay,
            dungeon
        };
        // for keeping track of the CCM_trackTypes_enum
        internal static int CCM_currentTrackType = -1;

        internal static System.Random CCM_getRandom = new System.Random();

        internal static ManualLogSource CCM_logSource = BepInEx.Logging.Logger.CreateLogSource("CCM_project");


        /* ------------------------------------------------------------------------
            Music Handlers
        ------------------------------------------------------------------------ */
        // music game objects we will use to actually play music
        internal static GameObject CCM_musicHandler_1;
        internal static GameObject CCM_musicHandler_2;
        internal static AudioSource CCM_musicAudiSource_1;
        internal static AudioSource CCM_musicAudiSource_2;
        internal static float CCM_musicVolume;

        // this keeps track of which music handler is actually currently intended to be played on
        // for instance, when transitioning to a new track, this one
        internal static GameObject CCM_nowPlayingMusicHandler;
        internal static AudioSource CCM_nowPlayingAudioSource;




        /* ------------------------------------------------------------------------
        
            awake function

        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            CCM_fnc_parseConfig();

            DontDestroyOnLoad(this.gameObject);
            this.gameObject.name = "CCM Core GameObject";
            gameObject.AddComponent<CCM_rpc>();
            CCM_Instance = this;

            CCM_fnc_instantiateHarmony();

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










        /*
        IEnumerator LoadMusic(string songPath)
        {
            if (System.IO.File.Exists(songPath))
            {
                using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + songPath, AudioType.AUDIOQUEUE))
                {
                    ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;

                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.LogError(uwr.error);
                        yield break;
                    }

                    DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)uwr.downloadHandler;

                    if (dlHandler.isDone)
                    {
                        AudioClip audioClip = dlHandler.audioClip;

                        if (audioClip != null)
                        {
                            audioClip = DownloadHandlerAudioClip.GetContent(uwr);

                            Debug.Log("Playing song using Audio Source!");

                        }
                        else
                        {
                            Debug.Log("Couldn't find a valid AudioClip :(");
                        }
                    }
                    else
                    {
                        Debug.Log("The download process is not completely finished.");
                    }
                }
            }
            else
            {
                Debug.Log("Unable to locate converted song file.");
            }
        }
        */
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