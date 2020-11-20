using System.Linq;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace creativeCommonsMusicProject // Rename "MyNameSpace"
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class MyMod : BaseUnityPlugin // Rename "MyMod"
    {
        const string ID = "com.Ansible2.CCMproject"; // use the reverse domain syntax for BepInEx. Change "author" and "project".
        const string NAME = "CCM Project";
        const string VERSION = "1.0";

        internal void Awake()
        {
            // This is your entry-point for your mod.
            // BepInEx has created a GameObject and added our MyMod class as a component to it.

            Logger.Log(LogLevel.Message, "Hello world"); /* Prints to "BepInEx\LogOutput.log" */
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static bool IsRealScene(Scene scene)
        {
            var name = scene.name.ToLower();
            return !(name.Contains("lowmemory") && !name.Contains("mainmenu"));
        }
        public string sceneBeingChecked = "";
        private bool isSceneChecked(Scene sceneToCheck)
        {
            bool _isChecked = (sceneToCheck.name == sceneBeingChecked);
            return _isChecked;
        }

        private IEnumerator waitForSync(Scene myScene)
        {
            /*
            Logger.Log(LogLevel.Message, "Waiting for sync");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.name);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Logger.Log(LogLevel.Message, "Sync done");
            */
            string mySceneName = myScene.name;
            if (mySceneName != sceneBeingChecked && IsRealScene(myScene))
            {
                sceneBeingChecked = mySceneName;
                Logger.Log(LogLevel.Message, "Found real scene:");
                Logger.Log(LogLevel.Message, mySceneName);


                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    Logger.Log(LogLevel.Message, "waiting for loading...");
                    yield return new WaitForSeconds(1);
                }
                Logger.Log(LogLevel.Message, "loading done");
                Logger.Log(LogLevel.Message, "Searching for music...");

                var myList = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_"));

                Logger.Log(LogLevel.Message, "Music Found:");
                foreach (var _x in myList)
                {
                    Logger.Log(LogLevel.Message, _x.name);
                    Logger.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);
                }
                sceneBeingChecked = "";
            }
            else
            {
                Logger.Log(LogLevel.Message, "Skipped fake Scene:");
                Logger.Log(LogLevel.Message, mySceneName);
            }
        }

        private void OnSceneLoaded(Scene myScene, LoadSceneMode mySceneMode)
        {
            StartCoroutine(waitForSync(myScene));
        }

        // exec each frame
        void Update()
        {

        }

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