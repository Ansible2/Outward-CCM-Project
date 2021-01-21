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

    internal class CCM_scheduled : BaseUnityPlugin  
    {
        CCM_getPhotonView CCM_getPhotonView = new CCM_getPhotonView();
        CCM_core CCM_core = new CCM_core(); // how to get another class in a different file
        /* ------------------------------------------------------------------------
        
            CCM_fnc_startCombatMusicIntercept

        ------------------------------------------------------------------------ */
        internal static IEnumerator CCM_fnc_startCombatMusicIntercept()
        {
            CCM_core CCM_core = new CCM_core();
            ManualLogSource Logg = new ManualLogSource("myLog");


            CCM_core.CCM_fnc_logWithTime("Started combat music check");

            // create 2 lists for finding changes and comapring
            var _musicList = CCM_core.CCM_fnc_findMusicObjectsInScene();
            var _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();

            CCM_core.CCM_doRunCombatMusicCheck = true;
            while (CCM_core.CCM_doRunCombatMusicCheck)
            {
                CCM_core.CCM_fnc_logWithTime("Looping for combat music check");

                // update comparison list
                _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();
                Logg.Log(LogLevel.Message, _musicListCompare.Count);
                Logg.Log(LogLevel.Message, _musicList.Count);

                // check if any new (combat) music objects were created
                if (!CCM_core.CCM_fnc_areListsTheSame(_musicList, _musicListCompare))
                {
                    CCM_core.CCM_fnc_logWithTime("Found more music");
                    // replace _musicList with the new values in _musicListCompare
                    // copying list instead of referencing
                    _musicList = new List<GameObject>(_musicListCompare);
                    CCM_core.CCM_fnc_logWithTime("Adjusted music list");
                }

                // sleep 0.5
                yield return new WaitForSeconds(0.5f);
            }
            CCM_core.CCM_fnc_logWithTime("Ended combat music check");
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_waitForLoadingDone

        ------------------------------------------------------------------------ */
        internal IEnumerator CCM_fnc_waitForLoadingDone(Scene _myScene)
        {
            string _mySceneName = _myScene.name;

            // some scenes such as the main menu and loading scenes should not be touched
            // these are "fake" scenes
            if (CCM_core.CCM_fnc_isSceneReal(_myScene))
            {
                CCM_core.CCM_fnc_logWithTime("Found real scene: " + _mySceneName);

                // wait for all loading to finish
                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for loading...");
                    // sleep 1 second
                    yield return new WaitForSeconds(0.1f);
                }
                CCM_core.CCM_fnc_logWithTime("Loading done...");

                // tell every machine that is connected about what scene the player is on
                CCM_core.CCM_fnc_logWithTime("Telling server to update all on players current scene...");
                
                CCM_getPhotonView.CCM_photonView.RPC(
                    "CCM_fnc_changeActiveScene",
                    PhotonTargets.AllViaServer,
                    new object[] { _myScene.name, PhotonNetwork.player }
                );


                // start music replace music
                CCM_core.CCM_fnc_logWithTime("Finding main music object to change in scene");
                GameObject _mainMusicObject = CCM_core.CCM_fnc_findMainMusicObject(_myScene);
                string _mainMusicObjectName = _mainMusicObject.name;
                List<string> _possibleTracks_list = CCM_core.CCM_fnc_getAllAVailableTrackForScene(_mainMusicObjectName);
                var _trackFilePath = CCM_core.CCM_fnc_selectTrackToPlay(_possibleTracks_list);

                CCM_fnc_loadAndPlayAudioClip(_trackFilePath);
                
                




                // wait until combat music check if off
                while (CCM_core.CCM_doRunCombatMusicCheck)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for combat music check reset...");
                    yield return new WaitForSeconds(0.1f);
                }

                CCM_core.CCM_fnc_logWithTime("Reached combat music check");
                // start combat music check loop
                StartCoroutine(CCM_fnc_startCombatMusicIntercept());
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("Skipped fake Scene: " + _mySceneName);
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_loadAndPlayAudioClip

        ------------------------------------------------------------------------ */
        internal IEnumerator CCM_fnc_loadAndPlayAudioClip(string _filePath)
        {
            //string _filePath = "";
            var _formattedPath = CCM_core.CCM_filePathStart + _filePath;

            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_formattedPath, AudioType.OGGVORBIS))
            {
                //CCM_core CCM_core = new CCM_core();

                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.error != null)
                {
                    Debug.Log(www.error);
                    yield break;
                }

                var name = Path.GetFileNameWithoutExtension(_filePath);
                var clip = DownloadHandlerAudioClip.GetContent(www);
                GameObject.DontDestroyOnLoad(clip);

                GameObject _musicObjectToPlayOn = CCM_core.CCM_fnc_getMusicHandler();
                AudioSource _objectAudioSource = _musicObjectToPlayOn.GetComponent<AudioSource>();
                _objectAudioSource.clip = clip;

                _objectAudioSource.Play();
            }
            
        }
    }
}