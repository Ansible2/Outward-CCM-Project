/* ----------------------------------------------------------------------------
Function: CCM_spawn_startMusicRoutine

Description:
	When a scene's music plays
    
    It will handle when music is supposed to played at random times.

Parameters:
    0: _trackType <int> - The corresponding track type as in the CCM_trackTypes_enum

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_spawn_startMusicRoutine(CCM_trackTypes_enum.combat);
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
            CCM_spawn_startMusicRoutine
        ---------------------------------------------------------------------------- */
        internal static void CCM_spawn_startMusicRoutine(CCM_trackTypes_enum _trackType)
        {
            string _sceneName = CCM_currentScene.name;
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: was called for scene " + _sceneName + " with track type: " + _trackType);
            
            CCM_fnc_stopMusicRoutine();
            CCM_currentRoutine = CCM_Instance.StartCoroutine(_fn_beginRoutine(_trackType, _sceneName));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_stopMusicRoutine
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_stopMusicRoutine()
        {
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: CCM_fnc_stopMusicRoutine: was called");
            if (CCM_currentRoutine != null)
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: CCM_fnc_stopMusicRoutine: CCM_currentRoutine was NOT null, stopping...");
                CCM_Instance.StopCoroutine(CCM_currentRoutine);
                CCM_currentRoutine = null;
            }
            else
            {
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: CCM_fnc_stopMusicRoutine: CCM_currentRoutine was null, nothing to stop");
            }
            
        }


        /* ----------------------------------------------------------------------------
            _fn_beginRoutine
        ---------------------------------------------------------------------------- */
        private static IEnumerator _fn_beginRoutine(CCM_trackTypes_enum _trackType, string _sceneName)
        {
            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: was called for scene");

            bool _sceneActive = true;
            bool _isStartOfRoutine = true;
            while (_sceneActive)
            {
                var _track = _fn_decideNewTrackForScene(_trackType);
                string _sceneTrackFilename = _track.Filename;

                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: _sceneTrackFilename: " + _sceneTrackFilename);
                
                if (CCM_syncOnline)
                {
                    CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: Sync Online is true, RPCing CCM_event_playMusic_RPC to all players");
                    
                    CCM_rpc.CCM_photonView.RPC(
                        "CCM_event_playMusic_RPC",
                        PhotonTargets.All,
                        new object[] { _sceneTrackFilename, _track.FolderType, _isStartOfRoutine, _trackType}
                    );

                    // the start of a routine can be considered direct play
                    if (_isStartOfRoutine) { _isStartOfRoutine = false; }
                } 
                else
                {
                    CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: Sync Online is false, directly going to CCM_fnc_playMusic");
                    CCM_rpc.CCM_fnc_playMusic(_track.FolderType);
                }


                int _trackLength = _track.Length;
                int _sleepTime = _fn_decideTimeBetweenTracks(_trackType) + _trackLength;
                CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: sleep time will be: " + _sleepTime);
                CCM_fnc_logWithTime("_tracklength int: " + _trackLength);
                int _sleptTime = 0;

                while (_sleptTime < _sleepTime)
                {
                    if (CCM_currentScene.name == _sceneName)
                    {
                        /*
                        // if music can't play in the background and the game becomes paused, music will continue to pile up
                        if (GlobalAudioManager.MuteInBackground && Global.GamePaused)
                        {
                            yield return new WaitUntil(() => !Global.GamePaused);
                        }
                        */
                        yield return new WaitForSecondsRealtime(1);
                        //CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: Waiting for scene: " + _sceneName);
                        _sleptTime = _sleptTime + 1;
                    }
                    else
                    {
                        CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: Routine for scene: " + _sceneName + " is no longer considered active.");
                        _sleptTime = _sleepTime;
                        _sceneActive = false;
                        break;
                    }
                }
            }

            CCM_fnc_logWithTime("CCM_spawn_startMusicRoutine: _fn_beginRoutine: Routine for scene: " + _sceneName + " has exited its while loop.");   
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
        private static CCM_track _fn_decideNewTrackForScene(CCM_trackTypes_enum _trackType)
        {
            // list the scene as being in the process of choosing a new track to prevent players from requesting a play event from the server in CCM_fnc_requestTrackToPlay
            CCM_choosingTrackForScene = true;

            // get a random track
            CCM_currentTrack = CCM_fnc_grabRandomTrack(_trackType);

            CCM_choosingTrackForScene = false;

            return CCM_currentTrack;
        }
    }
}