/* ----------------------------------------------------------------------------
Function: CCM_spawn_loadAndPlayAudioClip

Description:
	Loads and begins playing a random audio file.

Parameters:
	0: _fileName <STRING> - The name of the file (including extension)
    1: _trackType <INT> - A number that corresponds to the enum CCM_trackTypes_enum

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_spawn_loadAndPlayAudioClip("myFile.ogg",0));
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Networking;


namespace creativeCommonsMusicProject
{
    partial class CCM_rpc
    {
        internal static IEnumerator CCM_spawn_loadAndPlayAudioClip(string _fileName, int _trackType = 0, bool _canInterrupt = false)
        {
            CCM_core.CCM_loadingAudio = true;
            CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Loading audio...");         

            string _folderPath = CCM_core.CCM_fnc_getTrackTypeFolderPath(_trackType);
            string _formattedPath = CCM_core.CCM_fnc_buildFilePath(_folderPath, _fileName, true);

            CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Attempting to play audio at path: " + _formattedPath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_formattedPath, AudioType.OGGVORBIS))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Web request is done");

                if (www.error != null)
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Web request encountered the following error:");
                    CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: " + www.error);
                    yield break;
                }

                var clip = DownloadHandlerAudioClip.GetContent(www);

                var _musicObjectToPlayOn = CCM_core.CCM_fnc_getMusicHandler();
                CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Using music object " + _musicObjectToPlayOn.name);
                AudioSource _objectAudioSource = _musicObjectToPlayOn.GetComponent<AudioSource>();
                _objectAudioSource.clip = clip;
                _objectAudioSource.clip.name = _fileName; // file name has extension
                //_objectAudioSource.volume

                CCM_core.CCM_loadingAudio = false;

                CCM_core.CCM_fnc_logWithTime("CCM_spawn_loadAndPlayAudioClip: Do play");
                _objectAudioSource.Play();
                CCM_core.CCM_Instance.StartCoroutine(CCM_core.CCM_spawn_fadeAudioSource(_objectAudioSource, 3, CCM_core.CCM_musicVolume));
                CCM_core.CCM_nowPlayingMusicHandler = _musicObjectToPlayOn;
                CCM_core.CCM_nowPlayingAudioSource = _objectAudioSource;
            }
        }

        [PunRPC]
        internal void CCM_fnc_loadAndPlayAudioClip_RPC(string _fileName, int _trackType = 0, bool _canInterrupt = false)
        {
            StartCoroutine(CCM_spawn_loadAndPlayAudioClip(_fileName, _trackType, _canInterrupt));
        }
    }
}