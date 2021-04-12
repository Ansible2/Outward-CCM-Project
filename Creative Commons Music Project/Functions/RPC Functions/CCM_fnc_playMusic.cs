/* ----------------------------------------------------------------------------
Function: CCM_fnc_playMusic

Description:
	Starts playing a filename music

Parameters:
	0: _filename <STRING> - The name of the file you want to play
    1: _folderType <CCM_core.CCM_trackTypes_enum> - The folder type of the track (for finding its file path)   

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
        internal static void CCM_fnc_playMusic(string _filename, CCM_core.CCM_trackTypes_enum _folderType)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: was called for file " + _filename);
            if (CCM_core.CCM_Dictionaries.trackLengthFromString.ContainsKey(_filename))
            {
                bool _musicIsPlaying = false;
                if (CCM_core.CCM_MusicHandlers.nowPlayingAudioSource != null)
                {
                    _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;
                } 

                if (_musicIsPlaying)
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


        // While _trackType uses the same enum, it is not the same as the _folderType
        // _trackType is specifically the KIND of track that was requested(e.g. for a townDay or ambientNight)
        [PunRPC]
        internal void CCM_event_playMusic_RPC(string _filename, CCM_core.CCM_trackTypes_enum _folderType, string _sceneFor, bool _isDirect, CCM_core.CCM_trackTypes_enum _trackType)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: was called...");
            if (CCM_core.CCM_syncOnline)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Sync Online is on, checking if scene needs music played...");
                if (CCM_core.CCM_currentScene.name == _sceneFor)
                {
                    var _timeSinceLastGoodEvent = Time.unscaledTime - CCM_core.CCM_timeOfLastMusicEvent;
                    CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Time since last good event: " + _timeSinceLastGoodEvent);
                    if (_timeSinceLastGoodEvent < 5)
                    {
                        CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that event was successfully triggered withing 5 seconds of the previous one...");
                        bool _sameTrackLoaded = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.clip.name == _filename;
                        bool _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;

                        if (_sameTrackLoaded && _musicIsPlaying)
                        {
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that the same music is already playing, will not update.");
                        }
                        else
                        {
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that the same music is NOT already playing, will update...");
                            CCM_core.CCM_timeOfLastMusicEvent = Time.unscaledTime;
                            CCM_fnc_playMusic(_filename, _folderType);
                        }
                    }
                    else
                    {
                        CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that event was successfully triggered more then 5 seconds after the last one...");
                        CCM_core.CCM_timeOfLastMusicEvent = Time.unscaledTime;
                        CCM_fnc_playMusic(_filename, _folderType);
                    }

                    /*
                    // in instances like sleeping when the time of day transitions, clients can both request a track directly from the masterClient
                    // and can also be given the track based upon a new routine starting in CCM_spawn_startMusicRoutineForScene: _fn_beginRoutine
                    // this is to keep the same track from attempting to play twice
                    bool _directRequestWasSent = CCM_core.CCM_directRequestType != CCM_core.CCM_trackTypes_enum.EMPTY;
                    if (_directRequestWasSent)
                    {
                        CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found a direct request was made by player for type: " + CCM_core.CCM_directRequestType);
                        bool _directIsSameType = CCM_core.CCM_directRequestType == _trackType;
                        if (_isDirect && _directIsSameType)
                        {
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Event is direct and is same type, playing music...");
                            CCM_core.CCM_directRequestType = CCM_core.CCM_trackTypes_enum.EMPTY;

                            var _timeSinceLastGoodEvent = Time.unscaledTime - CCM_core.CCM_timeOfLastMusicEvent;
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Time since last good event: " + _timeSinceLastGoodEvent);
                            if (_timeSinceLastGoodEvent < 5)
                            {
                                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that event was successfully triggered withing 5 seconds of the previous one...");
                                bool _sameTrackLoaded = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.clip.name == _filename;
                                bool _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;
                                
                                if (_sameTrackLoaded && _musicIsPlaying)
                                {
                                    CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that the same music is already playing, will not update.");
                                }
                                else
                                {
                                    CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that the same music is NOT already playing, will update...");
                                    CCM_core.CCM_timeOfLastMusicEvent = Time.unscaledTime;
                                    CCM_fnc_playMusic(_filename, _folderType);
                                }
                            }
                            else
                            {
                                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found that event was successfully triggered more then 5 seconds after the last one...");
                                CCM_core.CCM_timeOfLastMusicEvent = Time.unscaledTime;
                                CCM_fnc_playMusic(_filename, _folderType);
                            }
                            

                        }
                        else
                        {
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: _isDirect? " + _isDirect + " : _directIsSameType? " + _directIsSameType);
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: _directRequestType = " + CCM_core.CCM_directRequestType + " : _trackType = " + _trackType);
                            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Event is NOT direct or is NOT proper type, will not play music");
                        }
                        
                    }
                    else
                    {
                        CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Found no direct request was made by player... _isDirect? " + _isDirect);
                        CCM_core.CCM_timeOfLastMusicEvent = Time.unscaledTime;
                        CCM_fnc_playMusic(_filename, _folderType);
                    }
                    */
                }
                else
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic_RPC: Will not play music file: " + _filename + ". _isDirect is: " + _isDirect + " and scene for is: " + _sceneFor + " while current scene is: " + CCM_core.CCM_currentScene.name);
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

            using (UnityWebRequest _request = UnityWebRequestMultimedia.GetAudioClip(_pathToFile, _audioType))
            {
                ((DownloadHandlerAudioClip)_request.downloadHandler).streamAudio = true;
                _request.SendWebRequest();

                while (!_request.isDone)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }


                CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request is done for " + _filename);

                if (_request.error != null)
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request encountered the following error: " + _request.error);
                    yield break;
                }


                AudioClip _clip = DownloadHandlerAudioClip.GetContent(_request);

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