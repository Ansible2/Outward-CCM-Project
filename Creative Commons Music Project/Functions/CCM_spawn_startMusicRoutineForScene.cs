/* ----------------------------------------------------------------------------
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


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /* ----------------------------------------------------------------------------
            CCM_spawn_startMusicRoutineForScene
        ---------------------------------------------------------------------------- */
        internal static void CCM_spawn_startMusicRoutineForScene(string _sceneName, CCM_trackTypes_enum _trackType, bool _deletePrevious = true)
        {
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: was called for scene " + _sceneName);
            bool _routineExistsForScene = CCM_Dictionaries.sceneRoutines.ContainsKey(_sceneName);
            if (_deletePrevious)
            {
                CCM_fnc_stopMusicRoutine(_sceneName);
            }

            var _routine = CCM_Instance.StartCoroutine(_fn_beginRoutine(_sceneName, _trackType));
            CCM_Dictionaries.sceneRoutines.Add(_sceneName, _routine);

        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_stopMusicRoutine
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_stopMusicRoutine(string _sceneName)
        {
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: CCM_fnc_stopMusicRoutine: was called for scene " + _sceneName);

            if (CCM_Dictionaries.sceneRoutines.ContainsKey(_sceneName))
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: CCM_fnc_stopMusicRoutine: Found scene: " + _sceneName + " was already in sceneRoutines dictionary. Stopping it now.");
                var _routine = CCM_Dictionaries.sceneRoutines[_sceneName];

                CCM_Dictionaries.sceneRoutines.Remove(_sceneName);
                CCM_Instance.StopCoroutine(_routine);
                //_routine = null;
            } 
            else
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: CCM_fnc_stopMusicRoutine: Found scene: " + _sceneName + " was NOT in sceneRoutines dictionary. No need to stop.");
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_beginRoutine
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_beginRoutine(string _sceneName, CCM_trackTypes_enum _trackType)
        {
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: was called for scene " + _sceneName);

            yield return new WaitUntil(() => CCM_Dictionaries.sceneRoutines.ContainsKey(_sceneName));
            var _routine = CCM_Dictionaries.sceneRoutines[_sceneName];
            // create a function that will select a track for a scene
            // that will set the bool to tell RPC'd functions that it is happening
            // then if someone request a track while that bool is set to true (and it isn't a change to the music routine), 
            /// just exit because they will receive the RPC'd event for playmusic on that scene.
            /// if the bool is false, THEN RPC a play music on that player per a request

            bool _sceneActive = true;
            while (_sceneActive)
            {
                var _track = _fn_decideNewTrackForScene(_trackType, _sceneName);
                string _sceneTrackFilename = _track.Filename;


                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Attempting RPC of _sceneTrackFilename: " + _sceneTrackFilename + " _sceneName: " + _sceneName);
                
                if (CCM_syncOnline)
                {
                    CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Sync Online is true, RPCing CCM_event_playMusic_RPC to all players");
                    CCM_rpc.CCM_photonView.RPC(
                        "CCM_event_playMusic_RPC",
                        PhotonTargets.All,
                        new object[] { _sceneTrackFilename, _track.FolderType,_sceneName, true }
                    );
                } 
                else
                {
                    CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Sync Online is false, directly going to CCM_fnc_playMusic");
                    CCM_rpc.CCM_fnc_playMusic(_sceneTrackFilename, _track.FolderType, true);
                }


                int _trackLength = _track.Length;
                int _sleepTime = _fn_decideTimeBetweenTracks(_trackType) + _trackLength;
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: sleep time will be: " + _sleepTime);
                CCM_fnc_logWithTime("_tracklength int: " + _trackLength);
                int _sleptTime = 0;

                while (_sleptTime < _sleepTime)
                {
                    if (CCM_Dictionaries.activePlayerScenes.ContainsValue(_sceneName))
                    {
                        yield return new WaitForSecondsRealtime(1);
                        //CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Waiting for scene: " + _sceneName);
                        _sleptTime = _sleptTime + 1;
                    }
                    else
                    {
                        CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Routine for scene: " + _sceneName + " is no longer considered active.");
                        _sleptTime = _sleepTime;
                        _sceneActive = false;
                        break;
                    }
                }
            }
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Routine for scene: " + _sceneName + " has exited its while loop.");

            // if music game object is still alive
            if (_routine != null)
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Found that routine for scene: " + _sceneName + " was not null, stopping...");
                CCM_fnc_stopMusicRoutine(_sceneName);
            } 
            else
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine: Routine for scene: " + _sceneName + " was already null, will not force stop...");
            }
            

            if (CCM_Dictionaries.sceneRoutines.ContainsKey(_sceneName) && !CCM_Dictionaries.activePlayerScenes.ContainsValue(_sceneName))
            {
                CCM_Dictionaries.sceneRoutines.Remove(_sceneName);
            }
   
        }


        /* ----------------------------------------------------------------------------
            _fn_decideTimeBetweenTracks
        ---------------------------------------------------------------------------- */
        private static int _fn_decideTimeBetweenTracks(CCM_trackTypes_enum _trackType)
        {
            // get min/max values
            var _sleepList = CCM_Dictionaries.trackSpacingFromType[_trackType];
            var _sleepTime = CCM_getRandom.Next(_sleepList[0], _sleepList[1]);
            
            return _sleepTime;
        }


        /* ----------------------------------------------------------------------------
            _fn_decideNewTrackForScene
        ---------------------------------------------------------------------------- */
        private static CCM_track _fn_decideNewTrackForScene(CCM_trackTypes_enum _trackType, string _sceneName)
        {
            // list the scene as being in the process of choosing a new track to prevent players from requesting a play event from the server in CCM_fnc_requestTrackToPlay
            CCM_Lists.scenesChoosingMusicFor.Add(_sceneName);
            
            // get a random track
            var _newTrack = CCM_fnc_grabRandomTrack(_trackType);

            if (CCM_Dictionaries.activeScenesCurrentTrack.ContainsKey(_sceneName))
            {
                CCM_Dictionaries.activeScenesCurrentTrack[_sceneName] = _newTrack;
            }
            else
            {
                CCM_Dictionaries.activeScenesCurrentTrack.Add(_sceneName, _newTrack);
            }
            // THIS MAY NEED TO GO AFTER THE RPC TO ALL PLAYERS

            // list the scene as no longer changing music
            CCM_Lists.scenesChoosingMusicFor.Remove(_sceneName);

            return _newTrack;
        }
    }
}