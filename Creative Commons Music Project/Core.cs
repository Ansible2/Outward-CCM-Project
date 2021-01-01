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
    public class CCM_core : BaseUnityPlugin //Photon.MonoBehaviour
    {
        const string ID = "com.Ansible2.CCMproject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        public const string CCM_mainFolderPath = @"Mods\CCM Project";
        public const string CCM_combatFolderPath = CCM_mainFolderPath + @"\Combat Tracks";
        public const string CCM_ambientNightFolderPath = CCM_mainFolderPath + @"\Ambient Night Tracks";
        public const string CCM_ambientDayFolderPath = CCM_mainFolderPath + @"\Ambient Day Tracks";
        public const string CCM_cityFolderPath = CCM_mainFolderPath + @"\City Tracks";
        public const string CCM_dungeonFolderPath = CCM_mainFolderPath + @"\Dungeon Tracks";

        CCM_rpc CCM_rpc = new CCM_rpc();
        CCM_scheduled CCM_scheduled = new CCM_scheduled();

        // a list for storing the combat music object names as strings
        internal static List<string> CCM_combatMusicList = new List<string>();
        // used for controlling the loop in CCM_fnc_startCombatMusicIntercept
        internal static bool CCM_doRunCombatMusicCheck = true;

        /* ------------------------------------------------------------------------
        
            CCM_fnc_logWithTime

        ------------------------------------------------------------------------ */
        public void CCM_fnc_logWithTime(string myMessage = "")
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

            return (!_name.Contains("lowmemory") && !_name.Contains("mainmenu"));
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
        
            CCM_fnc_replaceAudio

        ------------------------------------------------------------------------ */
        internal bool CCM_fnc_replaceAudio(GameObject _objectToChange)
        {
            if (_objectToChange != null)
            {
                var _objectName = _objectToChange.name;
                var _objectAudioClip = _objectToChange.GetComponent<AudioSource>().clip;


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
        private void CCM_onSceneLoaded(Scene _myScene, LoadSceneMode _mySceneMode)
        {
            // combat music will always be reset on scene changes
            CCM_doRunCombatMusicCheck = false;

            // wait for loading to be done in a scheduled environment
            // also runs combat music check
            StartCoroutine(CCM_scheduled.CCM_fnc_waitForLoadingDone(_myScene));


            /*    
            if (PhotonNetwork.isMasterClient)
            {
                GetComponent<PhotonView>().RPC("doAThing",PhotonTargets.MasterClient);
            }
            */
        }










/*
        //[PunRPC]
        public void doAThing()
        {
            
        }
*/

        // start a suspeneded while once a scene is loaded because it will only need to account for change upon scene transisition
        // need to find a way to end it, however.
        // could also just exec it at start and then just keep it running at intervals

        // exec each frame
        /*
        void Update()
        {
            musicListCompare = (List<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_"));
            if (CCM_fnc_areListsTheSame(musicList, musicListCompare)) {
                musicList = musicListCompare;
                Logger.Log(LogLevel.Message,"Found new music object");
            }
        }
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


/*
Logger.Log(LogLevel.Message, "Waiting for sync");
AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.name);
while (!asyncLoad.isDone)
{
    yield return null;
}
Logger.Log(LogLevel.Message, "Sync done");
*/