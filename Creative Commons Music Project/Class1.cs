using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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
        
        public static List<string> combatMusicList = new List<string>();

        internal void Awake()
        {
            // This is your entry-point for your mod.
            // BepInEx has created a GameObject and added our MyMod class as a component to it.

            //Logger.Log(LogLevel.Message, "Hello world");

            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // fill combat music list
            // only clones become active and play the music
            // can't use the standard ones with GameObject.Find
            combatMusicList.Add("BGM_StandardCombat(Clone)");
            combatMusicList.Add("BGM_CombatAbrassar(Clone)");
            combatMusicList.Add("BGM_CombatChersonese(Clone)");
            combatMusicList.Add("BGM_CombatEnmerkar(Clone)");
            combatMusicList.Add("BGM_CombatHallowedMarsh(Clone)");
            combatMusicList.Add("BGM_CombatAntiquePlateau(Clone)");
            combatMusicList.Add("BGM_CombatBossDLC1(Clone)");
            combatMusicList.Add("BGM_CombatDungeonAntique(Clone)");
            combatMusicList.Add("BGM_CombatDungeonFactory(Clone)");
            combatMusicList.Add("BGM_CombatMinibossDLC1(Clone)");
            combatMusicList.Add("BGM_DungeonAntique(Clone)");
        }

        // main menu is returning true?
        private bool IsRealScene(Scene scene)
        {
            var name = scene.name.ToLower();
            Logger.Log(LogLevel.Message, "Checking scene: " + name);
            return (!name.Contains("lowmemory") && !name.Contains("mainmenu"));
        }

        private List<GameObject> FindMusicObjects(bool _findAll = false)
        {
            List<GameObject> myList = new List<GameObject>();
            if (_findAll)
            {
                myList = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_")).ToList();
            }
            else
            {
                foreach (var _x in combatMusicList)
                {
                    var theObject = GameObject.Find(_x);
                    if (theObject != null)
                    {
                        myList.Add(theObject);
                        logWithTime("Added an object");
                        //Logger.Log(LogLevel.Message,theObject);
                    }
                }
            }

            return myList;
        }

        private static bool areListsTheSame(List<GameObject> list1, List<GameObject> list2)
        {
            var firstNotSecond = list1.Except(list2);
            var secondNotFirst = list2.Except(list1);
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }


        internal static bool doRunCombatMusicCheck = true;
        // run the loop that will detect if any combat music shows up

        private IEnumerator StartChecksForCombatMusic()
        {
            logWithTime("Started combat music check");
            var musicList = FindMusicObjects();
            var musicListCompare = FindMusicObjects();
            
            doRunCombatMusicCheck = true;
            while (doRunCombatMusicCheck)
            {
                logWithTime("Looping for combat music check");
                musicListCompare = FindMusicObjects();
                Logger.Log(LogLevel.Message, musicListCompare.Count);
                Logger.Log(LogLevel.Message, musicList.Count);

                if (!areListsTheSame(musicList, musicListCompare))
                {
                    logWithTime("Found more music");
                    musicList = musicListCompare;
                    logWithTime("Adjustsed music list");
                }


                yield return new WaitForSeconds(0.5f);
            }
            logWithTime("Ended combat music check");
        }

        /*       
        private IEnumerator StartChecksForCombatMusic()
        {
            logWithTime("Started combat music check");
            var musicList = FindMusicObjects();
            var musicListCompare = FindMusicObjects();

            doRunCombatMusicCheck = true;
            while (doRunCombatMusicCheck)
            {
                logWithTime("Looping for combat music check");
                musicListCompare = FindMusicObjects();

                if (!areListsTheSame(musicList,musicListCompare))
                {
                    logWithTime("Found more music");
                    musicList = musicListCompare;
                    logWithTime("Adjustsed music list");
                }

                yield return new WaitForSeconds((float)0.5);
            }
            logWithTime("Ended combat music check");
        }
        */

        private IEnumerator waitForSync(Scene myScene)
        {
            string mySceneName = myScene.name;
            if (IsRealScene(myScene))
            {
                logWithTime("Found real scene: " + mySceneName);

                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    logWithTime("waiting for loading...");
                    yield return new WaitForSeconds(1);
                }
                logWithTime("Loading done. Searching for music objects...");

                var myList = FindMusicObjects(true);

                logWithTime("Music Objects Found:");
                //Logger.Log(LogLevel.Message, "Music Objects Found:");
                foreach (var _x in myList)
                {
                    Logger.Log(LogLevel.Message, _x.name);
                    Logger.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);
                }

                while (doRunCombatMusicCheck)
                {
                    logWithTime("waiting for combat music check reset...");
                    yield return new WaitForSeconds(0.1f);
                }
                //yield return new WaitUntil(() => !doRunCombatMusicCheck);
                logWithTime("Reached combat music check");
                StartCoroutine(StartChecksForCombatMusic());
            }
            else
            {
                logWithTime("Skipped fake Scene: " + mySceneName);
            }
        }

        // on a scene change
        private void OnSceneLoaded(Scene myScene, LoadSceneMode mySceneMode)
        {
            doRunCombatMusicCheck = false;
            StartCoroutine(waitForSync(myScene));
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
            if (areListsTheSame(musicList, musicListCompare)) {
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