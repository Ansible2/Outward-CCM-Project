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


namespace creativeCommonsMusicProject
{
    [BepInPlugin(ID, NAME, VERSION)]

    internal class CCM_core : BaseUnityPlugin //Photon.MonoBehaviour
    {
        /* ------------------------------------------------------------------------
        
            Create Globals

        ------------------------------------------------------------------------ */
        const string ID = "com.Ansible2.CCMProject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        internal static CCM_core CCM_Instance;

        // for accessing classes in other files
        //CCM_rpc CCM_rpc = new CCM_rpc();
        //CCM_scheduled CCM_scheduled = new CCM_scheduled();
        CCM_getPhotonView CCM_getPhotonView = new CCM_getPhotonView();

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
        
        // used for controlling the loop in CCM_fnc_startCombatMusicIntercept
        internal static bool CCM_doRunCombatMusicCheck = true;
        
        // used to keep track of each player's' current scene. dictionary is global and synced between all players
        // this is so that if a player is first in the scene, they will define what the track is to everyone else who enters the scene after
        internal static Dictionary<PhotonPlayer, string> CCM_dictionary_activePlayerScenes = new Dictionary<PhotonPlayer, string>();
        
        // used to keep track of each active scenes music track
        internal static Dictionary<Scene, string> CCM_dictionary_activeScenesCurrentMusic = new Dictionary<Scene, string>();

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
            CCM_Instance = this;

            Logger.Log(LogLevel.Message, "Hello world");
            
            // This is your entry-point for your mod.
            // BepInEx has created a GameObject and added our MyMod class as a component to it.

            
            SceneManager.sceneLoaded += CCM_event_onSceneChanged;

            //NetworkLevelLoader.Instance.onSceneLoadingDone += CCM_event_onSceneDoneLoading;

            // fill combat music list
            // only clones become active and play the music
            // can't use the standard ones with GameObject.Find
            CCM_combatMusicList.Add("BGM_StandardCombat(Clone)");
            CCM_combatMusicList.Add("BGM_CombatAbrassar(Clone)");
            CCM_combatMusicList.Add("BGM_CombatChersonese(Clone)");
            CCM_combatMusicList.Add("BGM_CombatEnmerkar(Clone)");
            CCM_combatMusicList.Add("BGM_CombatHallowedMarsh(Clone)");
            CCM_combatMusicList.Add("BGM_CombatAntiquePlateau(Clone)");
            CCM_combatMusicList.Add("BGM_CombatBossDLC1(Clone)");
            CCM_combatMusicList.Add("BGM_CombatDungeonAntique(Clone)");
            CCM_combatMusicList.Add("BGM_CombatDungeonFactory(Clone)");
            CCM_combatMusicList.Add("BGM_CombatMinibossDLC1(Clone)");
            CCM_combatMusicList.Add("BGM_DungeonAntique(Clone)");

                             

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
        
            CCM_fnc_isSceneReal

        ------------------------------------------------------------------------ */
        internal static bool CCM_fnc_isSceneReal(Scene _scene)
        {
            var _name = _scene.name.ToLower();
            CCM_fnc_logWithTime("Checking scene: " + _name);

            bool _isSceneReal = (!_name.Contains("lowmemory") && !_name.Contains("mainmenu"));
            CCM_fnc_logWithTime("Scene real? " + _isSceneReal);

            return _isSceneReal;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_findMusicObjectsInScene

        ------------------------------------------------------------------------ */
        internal static List<GameObject> CCM_fnc_findMusicObjectsInScene(bool _findAll = false)
        {
            List<GameObject> _myList = new List<GameObject>();
            if (_findAll)
            {
                _myList = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_")).ToList();
            }
            else
            {
                // collect names of already found objects for comparison
                // lists are by default pass by ref in classes
                List<string> _myListNames = new List<string>();
                foreach (var _y in _myList)
                {
                    _myListNames.Add(_y.name);
                }

                foreach (var _x in CCM_combatMusicList)
                {
                    if (!_myListNames.Contains(_x))
                    {
                        var _theObject = GameObject.Find(_x);
                        if (_theObject != null)
                        {
                            _myList.Add(_theObject);
                            CCM_fnc_logWithTime("Added an object");
                        }
                    }
                }
            }

            return _myList;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_areListsTheSame

        ------------------------------------------------------------------------ */
        internal static bool CCM_fnc_areListsTheSame(List<GameObject> _list1, List<GameObject> _list2)
        {
            var _firstNotSecond = _list1.Except(_list2);
            var _secondNotFirst = _list2.Except(_list1);
            return !_firstNotSecond.Any() && !_secondNotFirst.Any();
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_stopMusicFromLooping

        ------------------------------------------------------------------------ */
        internal static bool CCM_fnc_stopMusicFromLooping(GameObject _objectToStop)
        {
            bool _isLooping = _objectToStop.GetComponent<AudioSource>().loop;
            if (_isLooping)
            {
                _objectToStop.GetComponent<AudioSource>().loop = false;
                
                return true;
            }
            else
            {
                return false;
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_stopMusicFromPlaying

        ------------------------------------------------------------------------ */
        internal static bool CCM_fnc_stopMusicFromPlaying(GameObject _objectToStop)
        {
            AudioSource _objectAudioSource = _objectToStop.GetComponent<AudioSource>();
            bool _isPlaying = _objectAudioSource.isPlaying;
            if (_isPlaying)
            {
                _objectAudioSource.Stop();

                return true;
            }
            else
            {
                return false;
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_event_onSceneChanged

        ------------------------------------------------------------------------ */
        internal void CCM_event_onSceneChanged(Scene _myScene, LoadSceneMode mode)
        {
            CCM_fnc_logWithTime("CCM_event_onSceneChanged called");
            // combat music will always be reset on scene changes
            CCM_doRunCombatMusicCheck = false;

            // wait for loading to be done in a scheduled environment
            // also runs combat music check
            StartCoroutine(CCM_scheduled.CCM_fnc_waitForLoadingDone(_myScene));
        }


        /* ------------------------------------------------------------------------
        
            CCM_event_onSceneDoneLoading

        ------------------------------------------------------------------------ */
        internal void CCM_event_onSceneDoneLoading()
        {
            CCM_fnc_logWithTime("CCM_event_onSceneDoneLoading called");

            //CCM_fnc_findMainMusicObject(SceneManager.GetActiveScene());
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_findMainMusicObject

        ------------------------------------------------------------------------ */
        internal static GameObject CCM_fnc_findMainMusicObject(Scene _sceneToCheck)
        {
            GameObject _mainMusicObject = null;
            if (CCM_fnc_isSceneReal(_sceneToCheck))
            {
                // get music objects currently active in the scene
                var _myList = CCM_fnc_findMusicObjectsInScene(true);

                // Print list of music objects & their clips
                // find music object to use for playing
                CCM_fnc_logWithTime("Music Objects Found:");
                bool _wasLooping, _wasPlaying;
                foreach (var _x in _myList)
                {
                    // stop playing music
                    _wasPlaying = CCM_fnc_stopMusicFromPlaying(_x);
                    // stop looping
                    _wasLooping = CCM_fnc_stopMusicFromLooping(_x);

                    CCM_logSource.Log(LogLevel.Message, _x.name);
                    CCM_logSource.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);

                    // if object was both playing and looping it would be the object that we can use to play music, so save it
                    if (_wasLooping && _wasPlaying)
                    {
                        _mainMusicObject = _x;
                        CCM_logSource.Log(LogLevel.Message, "Found a main music object: " + _mainMusicObject);
                    }
                }

                // if main music object is not found select an object
                if (_mainMusicObject == null)
                {
                    CCM_fnc_logWithTime("_mainMusicObject is null, finding replacement...");
                }
                else
                {
                    // assign properties (if not already) to custom music objects
                    if (!CCM_gameObjectPropsAssigned)
                    {
                        CCM_musicVolume = _mainMusicObject.GetComponent<AudioSource>().volume;

                        // copy settings to our objects for playing music
                        CCM_musicHandler_1 = Instantiate(_mainMusicObject);
                        CCM_musicHandler_1.name = "CCM_musicHandler_1";
                        DontDestroyOnLoad(CCM_musicHandler_1);
                        CCM_musicAudiSource_1 = CCM_musicHandler_1.GetComponent<AudioSource>();
                        CCM_musicAudiSource_1.volume = 0;


                        CCM_musicHandler_2 = Instantiate(_mainMusicObject);
                        CCM_musicHandler_2.name = "CCM_musicHandler_2";
                        DontDestroyOnLoad(CCM_musicHandler_2);
                        CCM_musicAudiSource_2 = CCM_musicHandler_2.GetComponent<AudioSource>();
                        CCM_musicAudiSource_2.volume = 0;


                        CCM_gameObjectPropsAssigned = true;

                        CCM_logSource.Log(LogLevel.Message, "Assigned CCM music handler objects the properties of " + _mainMusicObject);
                    }
                }
            }

            return _mainMusicObject;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_getAllAVailableTrackForScene

        ------------------------------------------------------------------------ */
        internal static List<string> CCM_fnc_getAllAVailableTracksForScene(string _objectName = "")
        {
            _objectName = _objectName.ToLower();
            List<string> _listOfTracks = new List<string>();

            // need to build up lists of all track types in awake function
            // then check the game object against these lists to find a track

            var _logString = "Found object " + _objectName + " matched tracks for ";
            // if a name exits
            if (_objectName != "")
            {   
                // dungeon
                if (_objectName.Contains("dungeon"))
                {
                    CCM_fnc_logWithTime(_logString + "dungeons");
                    // if all tracks are used up
                    if (CCM_dungeonTracks.Count() < 1)
                    {
                        CCM_dungeonTracks = new List<string>(CCM_usedDungeonTracks);
                        CCM_usedDungeonTracks.Clear();
                    }

                    CCM_currentTrackType = (int) CCM_trackTypes_enum.dungeon;
                    _listOfTracks = CCM_dungeonTracks;
                }
                // combat
                else if (_objectName.Contains("combat"))
                {
                    CCM_fnc_logWithTime(_logString + "combat");
                    if (CCM_combatTracks.Count() < 1)
                    {
                        CCM_combatTracks = new List<string>(CCM_usedCombatTracks);
                        CCM_usedCombatTracks.Clear();
                    }

                    CCM_currentTrackType = (int)CCM_trackTypes_enum.combat;
                    _listOfTracks = CCM_combatTracks;
                }
                // ambient
                else if (_objectName.Contains("region") || _objectName.Contains("chersonese"))
                {
                    // ambient night
                    if (_objectName.Contains("night"))
                    {
                        CCM_fnc_logWithTime(_logString + "ambient night");
                        if (CCM_ambientNightTracks.Count() < 1)
                        {
                            CCM_ambientNightTracks = new List<string>(CCM_usedAmbientNightTracks);
                            CCM_usedAmbientNightTracks.Clear();
                        }

                        CCM_currentTrackType = (int)CCM_trackTypes_enum.ambientNight;
                        _listOfTracks = CCM_ambientNightTracks;
                    }
                    else
                    {
                        CCM_fnc_logWithTime(_logString + "ambient day");
                        if (CCM_ambientDayTracks.Count() < 1)
                        {
                            CCM_ambientDayTracks = new List<string>(CCM_usedAmbientDayTracks);
                            CCM_usedAmbientDayTracks.Clear();
                        }

                        CCM_currentTrackType = (int)CCM_trackTypes_enum.ambientDay;
                        _listOfTracks = CCM_ambientDayTracks;
                    }
                    
                }
                // towns
                else if (_objectName.Contains("town") || _objectName.Contains("cierzo"))
                {
                    // town night
                    if (_objectName.Contains("night"))
                    {
                        CCM_fnc_logWithTime(_logString + "towns night");
                    }
                    else
                    {
                        CCM_fnc_logWithTime(_logString + "towns day");
                    }

                    if (CCM_townTracks.Count() < 1)
                    {
                        CCM_townTracks = new List<string>(CCM_usedTownTracks);
                        CCM_usedTownTracks.Clear();
                    }

                    CCM_currentTrackType = (int)CCM_trackTypes_enum.town;
                    _listOfTracks = CCM_townTracks;

                }
                // default
                else
                {
                    CCM_fnc_logWithTime(_logString + "NOTHING DEEPER");
                    // add empty string to not break follow on functions

                    CCM_currentTrackType = -1;
                    _listOfTracks.Add("");
                }
            }
            else
            {
                CCM_fnc_logWithTime(_logString + "NOTHING");

                CCM_currentTrackType = -1;
                // add empty string to not break follow on functions
                _listOfTracks.Add("");
            }

            return _listOfTracks;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_findMusicAtPath

        ------------------------------------------------------------------------ */
        internal List<string> CCM_fnc_findMusicAtPath(string _folderPathToSearch)
        {
            List<string> _returnList = new List<string>();

            if (!Directory.Exists(_folderPathToSearch))
            {
                CCM_fnc_logWithTime("Folder path " + _folderPathToSearch + " does not exist!");
            }
            else
            {
                // this will get all files of .ogg, however, this includes their paths
                string[] _files = Directory.GetFiles(_folderPathToSearch, "*.ogg");
                List<string> _tempList = _files.ToList();

                if (_tempList.Count() < 1)
                {
                    CCM_fnc_logWithTime("File path " + _folderPathToSearch + " returned no files!");
                }
                else
                {
                    // get only the file names for returns
                    string _tempFileName;
                    foreach (string _filePath in _tempList)
                    {
                        _tempFileName = Path.GetFileName(_filePath);
                        _returnList.Add(_tempFileName);
                    }
                }
            }

            return _returnList;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_selectTrackToPlay

        ------------------------------------------------------------------------ */
        internal static string CCM_fnc_selectTrackToPlay(List<string> _trackList)
        {
            string _trackFilePath = "";

            if (_trackList.Count() < 1)
            {
                CCM_fnc_logWithTime("_trackList is empty!");
            }
            else
            {
                int _randomIndex = CCM_getRandom.Next(_trackList.Count());
                _trackFilePath = _trackList.ElementAt(_randomIndex);
                _trackList.RemoveAt(_randomIndex);
            }

            CCM_fnc_logWithTime("Selected track file:");
            CCM_fnc_logWithTime(_trackFilePath);

            return _trackFilePath;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_getMusicHandler

        ------------------------------------------------------------------------ */
        internal static GameObject CCM_fnc_getMusicHandler()
        {
            AudioSource _objectAudioSource = CCM_musicHandler_1.GetComponent<AudioSource>();
            bool _isPlaying = _objectAudioSource.isPlaying;
            if (_isPlaying)
            {
                return CCM_musicHandler_2;
            }
            else
            {
                return CCM_musicHandler_1;
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_getTrackTypeFolderPath

        ------------------------------------------------------------------------ */
        internal static string CCM_fnc_getTrackTypeFolderPath(int _trackType)
        {
            switch (_trackType)
            {
                case (int)CCM_trackTypes_enum.ambientDay:
                    {
                        return CCM_ambientDayFolderPath;
                    }
                case (int)CCM_trackTypes_enum.ambientNight:
                    {
                        return CCM_ambientNightFolderPath;
                    }
                case (int)CCM_trackTypes_enum.combat:
                    {
                        return CCM_combatFolderPath;
                    }
                case (int)CCM_trackTypes_enum.dungeon:
                    {
                        return CCM_dungeonFolderPath;
                    }
                case (int) CCM_trackTypes_enum.town:
                    {
                        return CCM_townFolderPath;
                    }
                default:
                    {
                        CCM_fnc_logWithTime("Returned empty string for folder path");
                        return "";
                    }
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_buildFilePath

        ------------------------------------------------------------------------ */
        internal static string CCM_fnc_buildFilePath(string _folderPath, string _fileName, bool _addFileExtension = false)
        {
            string _filePath = _folderPath + @"\" + _fileName;
            
            if (_addFileExtension)
            {
                _filePath = CCM_filePathStart + _filePath;
            }

            CCM_fnc_logWithTime("Returned File Path: " + _filePath);
            
            return _filePath;
        }


        

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