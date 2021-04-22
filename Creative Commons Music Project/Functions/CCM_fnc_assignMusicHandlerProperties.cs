/* ----------------------------------------------------------------------------
Function: CCM_fnc_assignMusicHandlerProperties

Description:
	Takes a template GameObject and (deep) copies its properties to both CCM music
     handler objects. It also assigns a few key global values.

Parameters:
	0:  <GameObject> - The object to use as a tempalte from which to copy the properties

Returns:
	NOTHING 

Examples:
    (begin example)
		CCM_fnc_assignMusicHandlerProperties(_objectToCopyFrom);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using BepInEx.Logging;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        ///<summary>
        /// Takes a template GameObject and (deep) copies its properties to both CCM music handler objects. It also assigns a few key global values.
        ///</summary>
        internal static void CCM_fnc_assignMusicHandlerProperties(GameObject _objectToCopy)
        {
            if (!CCM_MusicHandlers.handlersInstantiated)
            {
                // if main music object is not found select an object
                if (_objectToCopy == null)
                {
                    CCM_fnc_logWithTime("CCM_fnc_assignMusicHandlerProperties: _objectToCopy is null");
                }
                else
                {
                    CCM_musicVolume = _objectToCopy.GetComponent<AudioSource>().volume;

                    // copy settings to our objects for playing music
                    CCM_MusicHandlers.musicHandler_1 = Instantiate(_objectToCopy);
                    CCM_MusicHandlers.musicHandler_1.name = "CCM_musicHandler_1";
                    DontDestroyOnLoad(CCM_MusicHandlers.musicHandler_1);
                    CCM_MusicHandlers.musicAudioSource_1 = CCM_MusicHandlers.musicHandler_1.GetComponent<AudioSource>();
                    CCM_MusicHandlers.musicAudioSource_1.volume = 0;
                    CCM_MusicHandlers.musicAudioSource_1.loop = false;
                    //CCM_MusicHandlers.musicAudioSource_1.name = "CCM_musicHandler_1_audioSource";


                    CCM_MusicHandlers.musicHandler_2 = Instantiate(_objectToCopy);
                    CCM_MusicHandlers.musicHandler_2.name = "CCM_musicHandler_2";
                    DontDestroyOnLoad(CCM_MusicHandlers.musicHandler_2);
                    CCM_MusicHandlers.musicAudioSource_2 = CCM_MusicHandlers.musicHandler_2.GetComponent<AudioSource>();
                    CCM_MusicHandlers.musicAudioSource_2.volume = 0;
                    CCM_MusicHandlers.musicAudioSource_2.loop = false;
                    //CCM_MusicHandlers.musicAudioSource_2.name = "CCM_musicHandler_2_audioSource";


                    CCM_MusicHandlers.handlersInstantiated = true;

                    CCM_logSource.Log(LogLevel.Message, "CCM_fnc_assignMusicHandlerProperties: Assigned CCM music handler objects the properties of " + _objectToCopy);
                }
            }
        }
    }
}