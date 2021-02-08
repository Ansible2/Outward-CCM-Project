/* ----------------------------------------------------------------------------
Function: CCM_spawn_startMusicRoutineForScene

Description:
	When a scened first gets music played, a music "routine" is started.
    
    It will handle when music is supposed to played at random times.

Parameters:
	0: _audioSoucre <AudioSource> - The AudioSource to fade

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_spawn_fadeAudioSource(_someAudioSource,3,0,true));
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;








/*
    create a gameobject to attach this routine to.
    Add these gameObjects to a list so that each is known to the server.
    Change the name of the gameobject to be the scene name + someting like "routine"
    When you need to reference a certain scenes routine, just look it up with that string.
    To stop a routine, destroy the gameObject associated to the scene.
*/





namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static void CCM_spawn_startMusicRoutineForScene(string _sceneName, int _trackType, bool _deletePrevious = true)
        {
            bool _routineExitsForScene = CCM_dictionary_sceneRoutineObjects.ContainsKey(_sceneName);
            if (_routineExitsForScene && _deletePrevious)
            {
                // call a function to delete the previous game object for it
            }

            // create object
            GameObject _musicRoutineObject = new GameObject(_sceneName + CCM_musicRoutinePostfixString);
            DontDestroyOnLoad(_musicRoutineObject);

            MonoBehaviour _musicRoutineInstance = _musicRoutineObject.GetOrAddComponent<MonoBehaviour>();
            CCM_dictionary_sceneRoutineObjects.Add(_sceneName, _musicRoutineObject);
            _musicRoutineInstance.StartCoroutine(CCM_fnc_startMusicRoutineForScene(_sceneName, _musicRoutineObject, _trackType));

        }

        private static IEnumerator CCM_fnc_startMusicRoutineForScene(string _sceneName, GameObject _musicRoutineObject, int _trackType)
        {
            while (CCM_dictionary_sceneRoutineObjects.ContainsKey(_sceneName))
            {
                yield return new WaitForSeconds(CCM_fnc_decideTimeBetweenTracks(_trackType));

                // get new track
                // set track in dictionary CCM_dictionary_activeScenesCurrentMusic
                // get everyone in the scence
                // RPC load And play onto everyone
            }


        }

        private static int CCM_fnc_decideTimeBetweenTracks(int _trackType)
        {
            int _sleepTime = 0;

            // use a switch for different spacing depending on type
            // can also use CCM_getRandom for a random factor

            return _sleepTime;
        }
    }
}

/*
    Current issue is between this and request track.

    Actually, here's what should happen.

    1. No matter what, a Vanilla PlayMusic call will ping the server for a load & play for the player's current scene
        1a. The client DOES NOT need to be waiting for a global to be defined on the other end.
        1b. Load & Play will be an RPC from the server
    
    2. The server needs to check if the request warrants a new music routine
        2a. A new music routine could be required for not having the same track type or one just not existing for that scene
        2b. If a new music routine is required. The server will select a starting track and pass that 
            track's duration to CCM_spawn_startMusicRoutineForScene so it can know how long to the first sleep will be
        2c. As is already the case, the request will be waiting on the server until a track is defined and then RPC a load&play to the requester
*/