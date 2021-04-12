﻿/* ----------------------------------------------------------------------------
Function: CCM_event_onSceneChanged

Description:
	An alias for SceneManager.sceneLoaded.
    This fires when a new scene BEGINS loading (not when it necessarily finished).

Parameters:
	0: _goingToScene <Scene> - The scene to load
    1: _mode <LoadSceneMode> - 

Returns:
	NOTHING 

Examples:
    (begin example)
		SceneManager.sceneLoaded += CCM_event_onSceneChangeStarted;
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine.SceneManagement;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal void CCM_event_onSceneChangeStarted(Scene _goingToScene, LoadSceneMode _mode)
        {
            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: called for Scene - " + _goingToScene.name);

            // sometimes this event can be called twice in a row, in order to avoid a double call of follow on functions, this is here
            bool _isMasterClient = PhotonNetwork.isMasterClient;
            //bool _sceneIsRealAndIsCurrent = CCM_currentScene != _goingToScene && CCM_fnc_isSceneReal(_goingToScene);
            if (CCM_fnc_isSceneReal(_goingToScene))
            {
                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Scene is real");
                CCM_currentScene = _goingToScene;

                bool _isNewScene = CCM_currentScene != _goingToScene;
                bool _routineIsRunning = CCM_currentRoutine != null;
                












            /*
                // if a player is joining a new multiplayer room, this will end all routines from their other session
                if (_isMasterClient)
                {
                    CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Local Machine is Master Client");

                    if (_isNewScene)
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: _goingToScene IS new scene");
                        //CCM_currentScene = _goingToScene;

                        if (_routineIsRunning)
                        {
                            // stop routine because is synced and a routine is running
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is NOT null. Stopping...");
                            //CCM_fnc_stopMusicRoutine(); // stop current routine
                        }
                        else
                        {
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is null. Won't need to stop one.");
                        }

                        // start routine
                    }
                    else
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: _goingToScene is NOT new scene");
                    }

                }
                else
                {
                    CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Local Machine is NOT Master Client.");

                    if (CCM_syncOnline)
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_syncOnline is ON");

                        if (_routineIsRunning)
                        {
                            // stop routine because is synced and a routine is running
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is NOT null. Stopping...");
                            //CCM_fnc_stopMusicRoutine(); // stop current routine
                        }
                        else
                        {
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is null. Won't need to stop one.");
                        }
                    }
                    else // if sync is not online
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_syncOnline is OFF");

                        if (_isNewScene)
                        {
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: _goingToScene IS new scene");

                            //CCM_currentScene = _goingToScene;
                            if (_routineIsRunning)
                            {
                                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is NOT null. Stopping...");
                                //CCM_fnc_stopMusicRoutine(); // stop current routine
                            }
                            else
                            {
                                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is null. Won't need to stop one.");
                            }

                            // start new routine
                        }
                        else // if this is the same scene
                        {
                            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: _goingToScene is NOT new scene");

                            if (_routineIsRunning)
                            {
                                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is NOT null. Do not need to start a new one");
                            }
                            else
                            {
                                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: CCM_currentRoutine is null. Need to start a new one");
                                // start new routine
                            }
                        }
                    }
                }
            */
            }
            else if (_goingToScene.name.ToLower().Contains("mainmenu") && CCM_MusicHandlers.nowPlayingAudioSource != null) // stop if going back to main menu
            {
                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Found that going to main menu scene and CCM music is playing. Will fade out...");
                CCM_spawn_fadeAudioSource(CCM_MusicHandlers.nowPlayingAudioSource, 3, 0, true, true);
            }
            else
            {
                CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Scene is not real.");
            }









        /*
            CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: called for Scene - " + _goingToScene.name);
            
            // sometimes this event can be called twice in a row, in order to avoid a double call of follow on functions, this is here
            if (CCM_currentScene != _goingToScene)
            {
                // if a player is joining a new multiplayer room, this will end all routines from their other session
                if (!PhotonNetwork.isMasterClient && CCM_syncOnline)
                {
                    foreach (var _x in CCM_Dictionaries.sceneRoutines)
                    {
                        CCM_fnc_stopMusicRoutine(_x.Key);
                    }
                }

                if (CCM_fnc_isSceneReal(_goingToScene))
                {
                    CCM_currentScene = _goingToScene;
                    if (CCM_syncOnline)
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Sync Online is on, RPCing CCM_fnc_changeActiveScene_RPC");
                        CCM_rpc.CCM_fnc_changeActiveScene_RPC(_goingToScene.name, PhotonNetwork.player.ID);
                    }
                    else
                    {
                        CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Sync Online is off, execing CCM_fnc_changeActiveScene");
                        CCM_rpc.CCM_rpcComponent.CCM_fnc_changeActiveScene(_goingToScene.name, PhotonNetwork.player.ID);
                    }
                }
                else if (_goingToScene.name.ToLower().Contains("mainmenu") && CCM_MusicHandlers.nowPlayingAudioSource != null) // stop if going back to main menu
                {
                    CCM_fnc_logWithTime("CCM_event_onSceneChangeStarted: Found that going to main menu scene and music is playing. Will fade out...");
                    CCM_spawn_fadeAudioSource(CCM_MusicHandlers.nowPlayingAudioSource, 3, 0, true, true);
                }
            }
         */   
        }
    }
}