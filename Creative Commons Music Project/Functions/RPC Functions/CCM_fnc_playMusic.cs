/* ----------------------------------------------------------------------------
Function: CCM_fnc_playMusic

Description:
	Starts playing a filename music

Parameters:
	0: _filename <STRING> - The name of the file you want to play
    1: _canInterrupt <BOOL> - Can this interrupt playing music

Returns:
	NOTHING

Examples:
    (begin example)
		CCM_fnc_playMusic("someSong.ogg",true);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;


namespace creativeCommonsMusicProject
{
    partial class CCM_rpc
    {
        internal static void CCM_fnc_playMusic(string _filename, CCM_core.CCM_trackTypes_enum _folderType, bool _canInterrupt = true)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: was called for file " + _filename);
            if (CCM_core.CCM_Dictionaries.trackLengthFromString.ContainsKey(_filename))
            {
                bool _musicIsPlaying = false;
                if (CCM_core.CCM_MusicHandlers.nowPlayingAudioSource != null)
                {
                    _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;
                } 

                if (_musicIsPlaying && !_canInterrupt)
                {
                    CCM_core.CCM_fnc_logWithTime("Did not play file: " + _filename + " because music is already playing and could not interrupt.");
                }
                else if (_musicIsPlaying && _canInterrupt)
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: Found that music was already playing on " + CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler.name + " ... Now fading it out...");
                    CCM_core.CCM_spawn_fadeAudioSource(CCM_core.CCM_MusicHandlers.nowPlayingAudioSource, 3, 0, true);
                    CCM_core.CCM_Instance.StartCoroutine(_fn_createAndPlayClip(_filename, _folderType));
                } 
                else
                {
                    CCM_core.CCM_Instance.StartCoroutine(_fn_createAndPlayClip(_filename, _folderType));
                }
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: Could not find an entry in CCM_dictionary_audioClipFromString for file: " + _filename);
            }

        }

        [PunRPC]
        internal void CCM_event_playMusic_RPC(string _filename, CCM_core.CCM_trackTypes_enum _folderType, string _sceneFor, bool _canInterrupt = true)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: was called...");
            if (CCM_core.CCM_syncOnline)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Sync Online is on, checking if scene needs music played...");
                if (CCM_core.CCM_currentScene.name == _sceneFor && _canInterrupt)
                {
                    CCM_fnc_playMusic(_filename, _folderType, _canInterrupt);
                }
                else
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Will not play music file: " + _filename + ". _canInterrupt is: " + _canInterrupt + " and scene for is: " + _sceneFor + " while current scene is: " + CCM_core.CCM_currentScene.name);
                }
            } 
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Won't execute as Sync Online is off in config.");
            }
        }

        private static IEnumerator _fn_createAndPlayClip(string _filename, CCM_core.CCM_trackTypes_enum _folderType)
        {
            CCM_core.CCM_loadingAudio = true;
            
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_createAndPlayClip: Called for song: " + _filename);


            var _folderPath = CCM_core.CCM_fnc_getTrackTypeFolderPath(_folderType);
            var _pathToFile = Path.Combine(CCM_core.CCM_Paths.FILE_PREFIX, _folderPath, _filename);
            AudioType _audioType = CCM_core.CCM_fnc_getAudioTypeFromString(_filename);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_pathToFile, _audioType))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return new WaitForSeconds(0.01f);
                }


                CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request is done for " + _filename);

                if (www.error != null)
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request encountered the following error: " + www.error);
                    yield break;
                }


                AudioClip _clip = DownloadHandlerAudioClip.GetContent(www);

                GameObject _musicHandler = CCM_core.CCM_fnc_getMusicHandler();
                CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_createAndPlayClip: Music handler for " + _filename + " is named: " + _musicHandler.name);
                AudioSource _handlerAudioSource = _musicHandler.GetComponent<AudioSource>();
                CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_createAndPlayClip: Music handler audiosource for " + _filename + " is named: " + _handlerAudioSource);


                _handlerAudioSource.clip = _clip;
                _handlerAudioSource.clip.name = _filename;

                _handlerAudioSource.Play();
                CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_createAndPlayClip: Handler told to play: " + _filename);
                CCM_core.CCM_spawn_fadeAudioSource(_handlerAudioSource, 3, 0.5f);
                CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler = _musicHandler;
                CCM_core.CCM_MusicHandlers.nowPlayingAudioSource = _handlerAudioSource;


                CCM_core.CCM_loadingAudio = false;
            }

            yield break;
            
        }
    }
}