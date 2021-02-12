﻿/* ----------------------------------------------------------------------------
Function: CCM_spawn_startMusicRoutineForScene

Description:
	When a scened first gets music played, a music "routine" is started.
    
    It will handle when music is supposed to played at random times.

Parameters:
	0: _sceneName <STRING> - The name of the scene this is to be play music for
    1: _trackType <int> - The corresponding int track type as in the CCM_trackTypes_enum
    2: _deletePrevious <BOOL> - If a previous routine exists for the scene, 
        should it be deleted before creating the new one

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_spawn_startMusicRoutineForScene("a scene name", (int)CCM_trackTypes_enum.combat, true);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /* ----------------------------------------------------------------------------
            CCM_spawn_startMusicRoutineForScene
        ---------------------------------------------------------------------------- */
        internal static void CCM_spawn_startMusicRoutineForScene(string _sceneName, int _trackType, bool _deletePrevious = true)
        {
            //bool _routineExistsForScene = CCM_dictionary_sceneRoutineObjects.ContainsKey(_sceneName);
            if (_deletePrevious)
            {
                _fn_stopMusicRoutine(_sceneName);
            }

            // create object
            GameObject _musicRoutineObject = new GameObject(_sceneName + CCM_musicRoutinePostfixString);
            DontDestroyOnLoad(_musicRoutineObject);

            MonoBehaviour _musicRoutineInstance = _musicRoutineObject.GetOrAddComponent<MonoBehaviour>();
            CCM_Dictionaries.sceneRoutineObjects.Add(_sceneName, _musicRoutineObject);
            
            
            _musicRoutineInstance.StartCoroutine(_fn_beginRoutine(_sceneName, _musicRoutineObject, _trackType));
        }


        internal static void _fn_stopMusicRoutine(string _sceneName)
        {
            if (CCM_Dictionaries.sceneRoutineObjects.ContainsKey(_sceneName))
            {
                GameObject _routineObject = CCM_Dictionaries.sceneRoutineObjects[_sceneName];

                CCM_Dictionaries.sceneRoutineObjects.Remove(_sceneName);
                Destroy(_routineObject);
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_beginRoutine
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_beginRoutine(string _sceneName, GameObject _musicRoutineObject, int _trackType)
        {
            // create a function that will select a track for a scene
            // that will set the bool to tell RPC'd functions that it is happening
            // then if someone request a track while that bool is set to true (and it isn't a change to the music routine), 
            /// just exit because they will receive the RPC'd event for playmusic on that scene.
            /// if the bool is false, THEN RPC a play music on that player per a request
            while (CCM_Dictionaries.sceneRoutineObjects.ContainsKey(_sceneName) && CCM_Dictionaries.activePlayerScenes.ContainsValue(_sceneName))
            {
                string _sceneTrackFileName = _fn_decideNewTrackForScene(_trackType, _sceneName);
                CCM_rpc.CCM_photonView.RPC(
                    "CCM_event_playMusic_RPC",
                    PhotonTargets.All,
                    new object[] { _sceneTrackFileName, _sceneName, true }
                );

                int _trackLength = (int)CCM_Dictionaries.audioClipFromString[_sceneTrackFileName].length;
                int _sleepTime = _fn_decideTimeBetweenTracks(_trackType) + _trackLength;

                yield return new WaitForSeconds(_sleepTime);
            }

            // if music game object is still alive
            if (_musicRoutineObject != null)
            {
                _fn_stopMusicRoutine(_sceneName);
            }

            if (CCM_Dictionaries.sceneRoutineObjects.ContainsKey(_sceneName) && !CCM_Dictionaries.activePlayerScenes.ContainsValue(_sceneName))
            {
                CCM_Dictionaries.sceneRoutineObjects.Remove(_sceneName);
            }

            
        }


        /* ----------------------------------------------------------------------------
            _fn_decideTimeBetweenTracks
        ---------------------------------------------------------------------------- */
        private static int _fn_decideTimeBetweenTracks(int _trackType)
        {
            // get min/max values
            var _sleepList = CCM_Dictionaries.trackSpacingFromType[_trackType];
            var _sleepTime = CCM_getRandom.Next(_sleepList[0], _sleepList[1]);
            
            return _sleepTime;
        }

        private static string _fn_decideNewTrackForScene(int _trackType, string _sceneName)
        {
            // list the scene as being in the process of choosing a new track to prevent players from requesting a play event from the server in CCM_fnc_requestTrackToPlay
            CCM_Lists.scenesChoosingMusicFor.Add(_sceneName);
            
            // get a random track
            List<string> _possibleTracks = CCM_core.CCM_fnc_getAllAvailableReplacementTracks(_trackType);
            string _newTrackName = CCM_core.CCM_fnc_grabRandomTrack(_possibleTracks);

            if (CCM_Dictionaries.activeScenesCurrentMusic.ContainsKey(_sceneName))
            {
                CCM_Dictionaries.activeScenesCurrentMusic[_sceneName] = _newTrackName;
            }
            else
            {
                CCM_Dictionaries.activeScenesCurrentMusic.Add(_sceneName, _newTrackName);
            }
            // THIS MAY NEED TO GO AFTER THE RPC TO ALL PLAYERS

            // list the scene as no longer changing music
            CCM_Lists.scenesChoosingMusicFor.Remove(_sceneName);

            return _newTrackName;
        }


    }
}