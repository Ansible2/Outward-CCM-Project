/* ----------------------------------------------------------------------------
Function: CCM_fnc_assignMusicHandlerProperties

Description:
	

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
        internal void CCM_fnc_assignMusicHandlerProperties()
        {
            // if main music object is not found select an object
            if (_mainMusicObject == null)
            {
                CCM_fnc_logWithTime("_mainMusicObject is null, finding replacement...");
            }
            else
            {
                // assign properties (if not already) to custom music objects
                if (!CCM_gameObjectPropsAssigned)
                {
                    CCM_musicVolume = _mainMusicObject.GetComponent<AudioSource>().volume;

                    // copy settings to our objects for playing music
                    CCM_musicHandler_1 = Instantiate(_mainMusicObject);
                    CCM_musicHandler_1.name = "CCM_musicHandler_1";
                    DontDestroyOnLoad(CCM_musicHandler_1);
                    CCM_musicAudiSource_1 = CCM_musicHandler_1.GetComponent<AudioSource>();
                    CCM_musicAudiSource_1.volume = 0;


                    CCM_musicHandler_2 = Instantiate(_mainMusicObject);
                    CCM_musicHandler_2.name = "CCM_musicHandler_2";
                    DontDestroyOnLoad(CCM_musicHandler_2);
                    CCM_musicAudiSource_2 = CCM_musicHandler_2.GetComponent<AudioSource>();
                    CCM_musicAudiSource_2.volume = 0;


                    CCM_gameObjectPropsAssigned = true;

                    CCM_logSource.Log(LogLevel.Message, "Assigned CCM music handler objects the properties of " + _mainMusicObject);
                }
            }
        }
    }
}