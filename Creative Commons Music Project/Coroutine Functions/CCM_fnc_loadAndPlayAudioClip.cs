/* ----------------------------------------------------------------------------
Function: CCM_fnc_loadAndPlayAudioClip

Description:
	Loads and begins playing a random audio file.

Parameters:
	0: _fileName <STRING> - The name of the file (including extension)
    1: _trackType <INT> - A number that corresponds to the enum CCM_trackTypes_enum

Returns:
	NOTHING

Examples:
    (begin example)
		StartCoroutine(CCM_fnc_loadAndPlayAudioClip("myFile.ogg",0));
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Collections;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;
using Photon;
using Photon.Realtime;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_scheduled : BaseUnityPlugin
    {
        [PunRPC]
        internal static IEnumerator CCM_fnc_loadAndPlayAudioClip(string _fileName, int _trackType = 0)
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

    }
}