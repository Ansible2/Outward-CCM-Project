/* ----------------------------------------------------------------------------
Function: CCM_fnc_findMainMusicObject

Description:
	Because there is only one BGM music object active at a time, this function is used
     to find that object return it 

Parameters:
	0:  <> - 

Returns:
	<> - 

Examples:
    (begin example)
		
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System;
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
    internal partial class CCM_core : BaseUnityPlugin
    {
        internal static GameObject CCM_fnc_findMainMusicObject(Scene _sceneToCheck)
        {
            GameObject _mainMusicObject = null;
            if (CCM_fnc_isSceneReal(_sceneToCheck))
            {
                // get music objects currently active in the scene
                var _myList = CCM_fnc_findMusicObjectsInScene(true);

                // Print list of music objects & their clips
                // find music object to use for playing
                CCM_fnc_logWithTime("Music Objects Found:");
                bool _wasLooping, _wasPlaying;
                foreach (var _x in _myList)
                {
                    // stop playing music
                    _wasPlaying = CCM_fnc_stopMusicFromPlaying(_x);
                    // stop looping
                    _wasLooping = CCM_fnc_stopMusicFromLooping(_x);

                    CCM_logSource.Log(LogLevel.Message, _x.name);
                    CCM_logSource.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);

                    // if object was both playing and looping it would be the object that we can use to play music, so save it
                    if (_wasLooping && _wasPlaying)
                    {
                        _mainMusicObject = _x;
                        CCM_logSource.Log(LogLevel.Message, "Found a main music object: " + _mainMusicObject);
                    }
                }

                
            }

            return _mainMusicObject;
        }
    }
}