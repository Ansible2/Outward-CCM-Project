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
        internal static IEnumerator CCM_fnc_waitForLoadingDone(Scene _myScene)
        {
            string _mySceneName = _myScene.name;

            // some scenes such as the main menu and loading scenes should not be touched
            // these are "fake" scenes
            if (CCM_core.CCM_fnc_isSceneReal(_myScene))
            {
                CCM_core.CCM_fnc_logWithTime("Found real scene: " + _mySceneName);

                // wait for all scene loading to finish
                CCM_core.CCM_fnc_logWithTime("Waiting for loading...");
                yield return new WaitUntil(() => NetworkLevelLoader.Instance.IsOverallLoadingDone);
            /*
                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for loading...");
                    // sleep 0.1 second
                    yield return new WaitForSeconds(0.1f);
                }
            */
                CCM_core.CCM_fnc_logWithTime("Loading done...");

                // tell every machine that is connected about what scene the player is on
                CCM_core.CCM_fnc_logWithTime("Telling server to update all on players current scene...");
                
            /*
                CCM_getPhotonView.CCM_photonView.RPC(
                    "CCM_fnc_changeActiveScene",
                    PhotonTargets.AllViaServer,
                    new object[] { _myScene.name, PhotonNetwork.player }
                );    
            */

                // start music replace music
                CCM_core.CCM_fnc_logWithTime("Finding main music object to change in scene");
                GameObject _mainMusicObject = CCM_core.CCM_fnc_findMainMusicObject(_myScene);
                string _mainMusicObjectName = _mainMusicObject.name;
                List<string> _possibleTracks_list = CCM_core.CCM_fnc_getAllAvailableReplacementTracks(_mainMusicObjectName);
                var _trackFileName = CCM_core.CCM_fnc_grabRandomTrack(_possibleTracks_list);

                CCM_core.CCM_fnc_logWithTime("Load audio call");
                CCM_core.CCM_Instance.StartCoroutine(CCM_fnc_loadAndPlayAudioClip(_trackFileName,CCM_core.CCM_currentTrackType));
                
                // wait for audio to load
                CCM_core.CCM_fnc_logWithTime("waiting for audio to load...");
                yield return new WaitUntil(() => CCM_core.CCM_loadingAudio);
                CCM_core.CCM_fnc_logWithTime("Audio play & load pass");




            /*
                // wait until combat music check if off
                while (CCM_core.CCM_doRunCombatMusicCheck)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for combat music check reset...");
                    yield return new WaitForSeconds(0.1f);
                }
                

                CCM_core.CCM_fnc_logWithTime("Reached combat music check");
                // start combat music check loop
                monoRef.StartCoroutine(CCM_fnc_startCombatMusicIntercept());
            */
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("Skipped fake Scene: " + _mySceneName);
            }
        }


        /* ------------------------------------------------------------------------
        
            CCM_fnc_loadAndPlayAudioClip

        ------------------------------------------------------------------------ */
        [PunRPC]
        internal static IEnumerator CCM_fnc_loadAndPlayAudioClip(string _fileName,int _trackType = 0)
        {
            CCM_core.CCM_fnc_logWithTime("Loading audio...");
            string _folderPath = CCM_core.CCM_fnc_getTrackTypeFolderPath(_trackType);

            string _formattedPath = CCM_core.CCM_fnc_buildFilePath(_folderPath, _fileName, true);

            CCM_core.CCM_loadingAudio = true;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_formattedPath, AudioType.OGGVORBIS))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                CCM_core.CCM_fnc_logWithTime("Web request is done");

                if (www.error != null)
                {
                    CCM_core.CCM_fnc_logWithTime("Web request encountered error");
                    yield break;
                }

                var clip = DownloadHandlerAudioClip.GetContent(www);

                GameObject _musicObjectToPlayOn = CCM_core.CCM_fnc_getMusicHandler();
                CCM_core.CCM_fnc_logWithTime("Using music object " + _musicObjectToPlayOn.name);
                AudioSource _objectAudioSource = _musicObjectToPlayOn.GetComponent<AudioSource>();
                _objectAudioSource.clip = clip;
                _objectAudioSource.clip.name = _fileName; // file name has extension
                //_objectAudioSource.volume

                CCM_core.CCM_loadingAudio = false;

                CCM_core.CCM_fnc_logWithTime("Do play");
                _objectAudioSource.Play();
                CCM_core.CCM_Instance.StartCoroutine(CCM_fnc_fadeAudioSource(_objectAudioSource, 3, CCM_core.CCM_musicVolume));
                CCM_core.CCM_nowPlayingMusicHandler = _musicObjectToPlayOn;
                CCM_core.CCM_nowPlayingAudioSource = _objectAudioSource;
            }
        }

        /* ------------------------------------------------------------------------
        
            CCM_fnc_fadeAudioSource

        ------------------------------------------------------------------------ */
        internal static IEnumerator CCM_fnc_fadeAudioSource(AudioSource _audioSoucre,float _duration = 3,float _targetVolume = 0.5f, bool _stopAfter = false)
        {
            float currentTime = 0;
            float _startingVolume = _audioSoucre.volume;

            while (currentTime < _duration)
            {
                currentTime += Time.deltaTime;
                _audioSoucre.volume = Mathf.Lerp(_startingVolume, _targetVolume, currentTime / _duration);
                yield return null;
            }

            if (_stopAfter)
            {
                _audioSoucre.Stop();
            }
            yield break;
        }
    }
}