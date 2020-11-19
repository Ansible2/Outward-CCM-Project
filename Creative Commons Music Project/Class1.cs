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

