﻿using System.Linq;
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
//using ExitGames.Client.Photon;

namespace creativeCommonsMusicProject // Rename "MyNameSpace"
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class MyMod : BaseUnityPlugin // Rename "MyMod"
    {
        const string ID = "com.Ansible2.CCMproject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";
        
        
        private void logWithTime(string myMessage = "")
        {
            Logger.Log(LogLevel.Message,Time.time + "--: " + myMessage);
        }
        
        internal static List<string> CCM_combatMusicList = new List<string>();

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

        // main menu is returning true?
        private bool _fn_isRealScene(Scene _scene)
        {
            var _name = _scene.name.ToLower();
            Logger.Log(LogLevel.Message, "Checking scene: " + _name);
            return (!_name.Contains("lowmemory") && !_name.Contains("mainmenu"));
        }

        private List<GameObject> _fn_findMusicObjects(bool _findAll = false)
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
                            logWithTime("Added an object");
                        }
                    }
                }
            }

            return _myList;
        }

        private static bool _fn_areListsTheSame(List<GameObject> _list1, List<GameObject> _list2)
        {
            var _firstNotSecond = _list1.Except(_list2);
            var _secondNotFirst = _list2.Except(_list1);
            return !_firstNotSecond.Any() && !_secondNotFirst.Any();
        }


        internal static bool CCM_doRunCombatMusicCheck = true;
        // run the loop that will detect if any combat music shows up

        private IEnumerator _startChecksForCombatMusic()
        {
            logWithTime("Started combat music check");
            var _musicList = _fn_findMusicObjects();
            var _musicListCompare = _fn_findMusicObjects();
            
            CCM_doRunCombatMusicCheck = true;
            while (CCM_doRunCombatMusicCheck)
            {
                logWithTime("Looping for combat music check");
                _musicListCompare = _fn_findMusicObjects();
                Logger.Log(LogLevel.Message, _musicListCompare.Count);
                Logger.Log(LogLevel.Message, _musicList.Count);

                if (!_fn_areListsTheSame(_musicList, _musicListCompare))
                {
                    logWithTime("Found more music");
                    _musicList = _musicListCompare;
                    logWithTime("Adjusted music list");
                }


                yield return new WaitForSeconds(0.5f);
            }
            logWithTime("Ended combat music check");
        }

        private IEnumerator _fn_waitForSync(Scene _myScene)
        {
            string _mySceneName = _myScene.name;
            if (_fn_isRealScene(_myScene))
            {
                logWithTime("Found real scene: " + _mySceneName);

                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    logWithTime("waiting for loading...");
                    yield return new WaitForSeconds(1);
                }
                logWithTime("Loading done. Searching for music objects...");

                var _myList = _fn_findMusicObjects(true);

                logWithTime("Music Objects Found:");
                foreach (var _x in _myList)
                {
                    Logger.Log(LogLevel.Message, _x.name);
                    Logger.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);
                }

                while (CCM_doRunCombatMusicCheck)
                {
                    logWithTime("waiting for combat music check reset...");
                    yield return new WaitForSeconds(0.1f);
                }
                //yield return new WaitUntil(() => !CCM_doRunCombatMusicCheck);
                logWithTime("Reached combat music check");
                StartCoroutine(_startChecksForCombatMusic());
            }
            else
            {
                logWithTime("Skipped fake Scene: " + _mySceneName);
            }
        }

        private void _fn_replaceAudio(GameObject _objectToChange)
        {
            if (_objectToChange != null)
            {
                var _objectName = _objectToChange.name;
                var _objectAudioClip = _objectToChange.GetComponent<AudioSource>().clip;
            }
        }
        
        // on a scene change
        private void CCM_onSceneLoaded(Scene _myScene, LoadSceneMode _mySceneMode)
        {
            CCM_doRunCombatMusicCheck = false;
            StartCoroutine(_fn_waitForSync(_myScene));
            if (PhotonNetwork.isMasterClient)
            {
                PhotonView.RPC("doAThing",PhotonTargets.Others);
            }
        }

        [PunRPC]
        void doAThing()
        {

        }













        // start a suspeneded while once a scene is loaded because it will only need to account for change upon scene transisition
        // need to find a way to end it, however.
        // could also just exec it at start and then just keep it running at intervals

        // exec each frame
        /*
        void Update()
        {
            musicListCompare = (List<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_"));
            if (_fn_areListsTheSame(musicList, musicListCompare)) {
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

    public class MyCustomRPC : Photon.MonoBehaviour
    {
        public void CallRemoteMethod()
        {

            if (PhotonNetwork.offlineMode == true)
            { //use this you need to support offline mode.
                //MyRemoteMethod(PhotonTargets.Others, new object[] { 42, true });
                return;
            }
            GetComponent<PhotonView>().RPC("MyRemoteMethod", PhotonTargets.Others, new object[] { 42, true });
   
               //Target Types
               //PhotonTargets.Others
               //PhotonTargets.All //triggered instantly
               //PhotonTargets.AllViaServer //local client gets even through server just like everyone else
               //PhotonTargets.MasterClient
               //PhotonNetwork.playerList[0]
               //PhotonTargets.AllBuffered
               //PhotonTargets.AllBufferedViaServer //used in item pickups where could be contested which client got it first
               //An important use is also when a new player connects later, they will recieve this 
               //buffered event that the item has been picked up and should be removed from scene
        }

        [PunRPC]
        void MyRemoteMethod(int someNumber, bool someBool)
        {
            Debug.Log(someNumber);
            Debug.Log(someBool);
        }
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