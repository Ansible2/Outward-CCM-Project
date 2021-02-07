//using System;
//using System.Linq;
//using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames;
using HarmonyLib;
//using UnityEngine.Networking;
//using Photon;
//using Photon.Realtime;


namespace creativeCommonsMusicProject
{
    [BepInPlugin(ID, NAME, VERSION)]

    internal partial class CCM_core : BaseUnityPlugin
    {
        /*
        internal class testClass : Photon.MonoBehaviour
        {
            internal void Awake()
            {
                CCM_fnc_logWithTime("does this work");
            }
        }
        */
        /* ------------------------------------------------------------------------
        
            Prepare Globals

        ------------------------------------------------------------------------ */
        const string ID = "com.Ansible2.CCMProject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        // used for running static coroutines
        internal static CCM_core CCM_Instance;

        internal static ManualLogSource CCM_logSource = BepInEx.Logging.Logger.CreateLogSource("CCM_project");
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

        

        // true when currently loading an audio file
        internal static bool CCM_loadingAudio = false;

        internal enum CCM_trackTypes_enum
        {
            combat,
            ambientNight,
            ambientDay,
            town,
            dungeon
        };

        // for keeping track of the CCM_trackTypes_enum
        internal static int CCM_currentTrackType = -1;

        // a list for storing the combat music object names as strings
        // this is to be able to detect when one is created
        internal static List<string> CCM_combatMusicList = new List<string>();
           
        // used to keep track of each player's' current scene. dictionary is global and synced between all players
        // this is so that if a player is first in the scene, they will define what the track is to everyone else who enters the scene after
        internal static Dictionary<int, string> CCM_dictionary_activePlayerScenes = new Dictionary<int, string>();
        
        // used to keep track of each active scenes music track
        // layout is scene/track
        internal static Dictionary<string, string> CCM_dictionary_activeScenesCurrentMusic = new Dictionary<string, string>();

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

        // this bool keeps track of CCM_musicHandler_1 & CCM_musicHandler_2
        // they need to be assigned the properties of a BGM (Background Music) game object
        // this is done (ideally) on the first run of CCM_fnc_findMainMusicObject
        internal static bool CCM_gameObjectPropsAssigned = false;     


        internal static System.Random CCM_getRandom = new System.Random();



        /* ------------------------------------------------------------------------
        
            awake function

        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.name = "CCM Core GameObject";
            gameObject.AddComponent<CCM_rpc>();
            CCM_Instance = this;

            CCM_fnc_instantiateHarmony();


            SceneManager.sceneLoaded += CCM_event_onSceneChangeStarted;
            //NetworkLevelLoader.Instance.onSceneLoadingDone += CCM_event_onSceneDoneLoading;

            // collect all filenames from the folders
            CCM_combatTracks = CCM_fnc_findMusicAtPath(CCM_combatFolderPath);
            CCM_ambientNightTracks = CCM_fnc_findMusicAtPath(CCM_ambientNightFolderPath);
            CCM_ambientDayTracks = CCM_fnc_findMusicAtPath(CCM_ambientDayFolderPath);
            CCM_townTracks = CCM_fnc_findMusicAtPath(CCM_townFolderPath);
            CCM_dungeonTracks = CCM_fnc_findMusicAtPath(CCM_dungeonFolderPath);
        }


        /* ------------------------------------------------------------------------
            CCM_fnc_logWithTime
        ------------------------------------------------------------------------ */
        internal static void CCM_fnc_logWithTime(string myMessage = "")
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













/*

    When play music event comes up, get the track type
    
    Send a request to the server for a track to play
        - player will send a target id (either photonNetwork.player or Id?) to receive a global to set for the track
            - The player will be waiting for this global to not be null before playing the music.
        
        - The server will halt all others requesting a track (for the same scene) until completeing the first request
         which it will then provide to others requesting.

        - if the track is for combat it will ignore what is currently in the CCM_dictionary_activeScenesCurrentMusic
         unless it is also a combat track
        
        - The server will get what the scene is currently playing or it will get a new track an set the scene's current playing one
    
    The server will then send a message back to the requester that will set a global that the client is waiting on to determine what track to load
    
*/










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



/*
1. Player starts game:
    - Need to use harmony to get inside of the music function responsible for creating music objects or playing them



// combat music objects are created upon combat initiation
// scene changes are not triggered for combat so you need either an event or frame check for it
/*
 * 1. Get the current scene
 * 2. Find all the game objects that have music attached to them in the current scene
 * 3. Of all the game objects find: if they are playing AND/OR they are looping and stop both
 * 4. Determine the music to play based upon the name of the object/location 
 * 5. change the music clip
 * 6. start playing the music again
 * 7. Detect when the music has stopped OR when combat music has been created using an onEachFrame type method (such as update)
 *      Combat music should be able to continue to loop, just needs to be random select
 * 8a. When music simply stops select from another in the list (should similar to KISKA random music system keep a list of used tracks
 * 8b. When combat music starts just change the clip based upon a random selection from a combat music list
 */


/*

    The Server should keep track of each active scene's song
    
    Active scenes can be told to the server by players by using scene onLoad event
     to tell the server what they're active scene is

*/



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