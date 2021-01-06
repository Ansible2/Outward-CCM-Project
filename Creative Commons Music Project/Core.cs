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
        const string ID = "com.Ansible2.CCMproject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        // folder paths for user defined music
        internal const string CCM_mainFolderPath = @"Mods\CCM Project";
        internal const string CCM_combatFolderPath = CCM_mainFolderPath + @"\Combat Tracks";
        internal const string CCM_ambientNightFolderPath = CCM_mainFolderPath + @"\Ambient Night Tracks";
        internal const string CCM_ambientDayFolderPath = CCM_mainFolderPath + @"\Ambient Day Tracks";
        internal const string CCM_cityFolderPath = CCM_mainFolderPath + @"\City Tracks";
        internal const string CCM_dungeonFolderPath = CCM_mainFolderPath + @"\Dungeon Tracks";

        CCM_rpc CCM_rpc = new CCM_rpc();
        CCM_scheduled CCM_scheduled = new CCM_scheduled();
        CCM_getPhotonView CCM_getPhotonView = new CCM_getPhotonView();


        // a list for storing the combat music object names as strings
        internal static List<string> CCM_combatMusicList = new List<string>();
        // used for controlling the loop in CCM_fnc_startCombatMusicIntercept
        internal static bool CCM_doRunCombatMusicCheck = true;
        // used to keep track of each player's' current scene. dictionary is global and synced between all players
        public static Dictionary<PhotonPlayer, string> CCM_dictionary_activePlayerScenes = new Dictionary<PhotonPlayer, string>();
        // used to keep track of each active scenes music track
        public static Dictionary<Scene, string> CCM_dictionary_activeScenesCurrentMusic = new Dictionary<Scene, string>();

        // music game objects we will use
        public GameObject CCM_musicHandler_1 = new GameObject("CCM_musicHandler_1");
        public GameObject CCM_musicHandler_2 = new GameObject("CCM_musicHandler_2");

        // this bool keeps track of CCM_musicHandler_1 & CCM_musicHandler_2
        // they need to be assigned the properties of a BGM game object
        // this is done (ideally) on the first run of CCM_fnc_findMainMusicObject
        public bool CCM_gameObjectPropsAssigned = false;


        /* ------------------------------------------------------------------------
        
            CCM_fnc_logWithTime

        ------------------------------------------------------------------------ */
        internal void CCM_fnc_logWithTime(string myMessage = "")
        {
            Logger.Log(LogLevel.Message,Time.time + "--: " + myMessage);
        }
        

        /* ------------------------------------------------------------------------
        
            awake function

        ------------------------------------------------------------------------ */
        internal void Awake()
        {
            // This is your entry-point for your mod.
            // BepInEx has created a GameObject and added our MyMod class as a component to it.

            //Logger.Log(LogLevel.Message, "Hello world");
            SceneManager.sceneLoaded += CCM_onSceneLoaded;
            
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
        }
        

        /* ------------------------------------------------------------------------
        
            CCM_fnc_isSceneReal

        ------------------------------------------------------------------------ */
        internal bool CCM_fnc_isSceneReal(Scene _scene)
        {
            var _name = _scene.name.ToLower();
            Logger.Log(LogLevel.Message, "Checking scene: " + _name);

            bool _isSceneReal = (!_name.Contains("lowmemory") && !_name.Contains("mainmenu"));
            Logger.Log(LogLevel.Message, "Scene real? " + _isSceneReal);

            return _isSceneReal;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_findMusicObjectsInScene

        ------------------------------------------------------------------------ */
        internal List<GameObject> CCM_fnc_findMusicObjectsInScene(bool _findAll = false)
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
        internal bool CCM_fnc_stopMusicFromLooping(GameObject _objectToStop)
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
        internal bool CCM_fnc_stopMusicFromPlaying(GameObject _objectToStop)
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
        
            CCM_onSceneLoaded

        ------------------------------------------------------------------------ */
        private void CCM_onSceneLoaded(Scene _myScene)
        {
            // combat music will always be reset on scene changes
            CCM_doRunCombatMusicCheck = false;

            // wait for loading to be done in a scheduled environment
            // also runs combat music check
            StartCoroutine(CCM_scheduled.CCM_fnc_waitForLoadingDone(_myScene));
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_findMainMusicObject

        ------------------------------------------------------------------------ */
        internal GameObject CCM_fnc_findMainMusicObject(Scene _sceneToCheck)
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
                    // stop looping
                    _wasLooping = CCM_fnc_stopMusicFromLooping(_x);

                    // stop playing music
                    _wasPlaying = CCM_fnc_stopMusicFromPlaying(_x);
                    Logger.Log(LogLevel.Message, _x.name);
                    Logger.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);

                    // if object was both playing and looping it would be the object that we can use to play music, so save it
                    if (_wasLooping && _wasPlaying)
                    {
                        _mainMusicObject = _x;
                        Logger.Log(LogLevel.Message, "Found a main music object: " + _mainMusicObject);
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
                        CCM_gameObjectPropsAssigned = true;
                        // copy settings to our objects for playing music
                        CCM_musicHandler_1 = Instantiate(_mainMusicObject);
                        CCM_musicHandler_1.name = "CCM_musicHandler_1";
                        CCM_musicHandler_2 = Instantiate(_mainMusicObject);
                        CCM_musicHandler_2.name = "CCM_musicHandler_2";

                        Logger.Log(LogLevel.Message, "Assigned CCM music handler objects the properties of " + _mainMusicObject);
                    }
                }
            }

            return _mainMusicObject;
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_getAvailableTracks

        ------------------------------------------------------------------------ */
        internal List<string> CCM_fnc_getAvailableTracks(string _objectName = "")
        {
            List<string> _listOfTracks = new List<string>();

            //_objectName.Contains("dungeon")


            // if no name exit
            if (_objectName == "")
            {
                _listOfTracks.Add("");

                
            }
            else
            {

            }

            return _listOfTracks;
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