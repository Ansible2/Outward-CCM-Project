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
        /* ----------------------------------------------------------------------------
            CCM_fnc_playMusic
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Plays a given track file
        ///</summary>
        internal static void CCM_fnc_playMusic(string _filename, CCM_core.CCM_trackTypes_enum _folderType)
        {
            CCM_core.CCM_fnc_log.withTime.message("CCM_fnc_playMusic: was called for file " + _filename);
            if (CCM_core.CCM_Dictionaries.trackLengthFromString.ContainsKey(_filename))
            {
                bool _musicIsPlaying = false;
                if (CCM_core.CCM_MusicHandlers.nowPlayingAudioSource != null)
                {
                    _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;
                } 


                if (_musicIsPlaying)
                {
                    CCM_core.CCM_fnc_log.debug("CCM_fnc_playMusic: Found that music was already playing on " + CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler.name + " ... Now fading it out...");
                    CCM_core.CCM_spawn_fadeAudioSource(CCM_core.CCM_MusicHandlers.nowPlayingAudioSource, 3, 0, true);              
                }

                CCM_core.CCM_Instance.StartCoroutine(_fn_createAndPlayClip(_filename, _folderType));
            }
            else
            {
                CCM_core.CCM_fnc_log.error("CCM_fnc_playMusic: ERROR Could not find an entry in CCM_Dictionaries.trackLengthFromString for file: " + _filename);
            }

        }


        /* ----------------------------------------------------------------------------
            CCM_event_playMusic_RPC
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// The remote version of CCM_fnc_playMusic
        ///</summary>
        [PunRPC]
        internal void CCM_event_playMusic_RPC(string _filename, CCM_core.CCM_trackTypes_enum _folderType)
        {
            CCM_core.CCM_fnc_log.withTime.debug("CCM_event_playMusic_RPC: was called...");

            if (CCM_core.CCM_syncOnline)
            {
                CCM_core.CCM_fnc_log.withTime.info("CCM_event_playMusic_RPC: Sync Online is ON, continuing with remotely triggered event...");

                CCM_fnc_playMusic(_filename, _folderType);

            } 
            else
            {
                CCM_core.CCM_fnc_log.withTime.debug("CCM_event_playMusic_RPC: Won't execute as Sync Online is off in config.");
            }
        }


        /* ----------------------------------------------------------------------------
            _fn_createAndPlayClip
        ---------------------------------------------------------------------------- */
        ///<summary>
        /// Creates an audioclip and plays it for a given filename and CCM_trackTypes_enum
        ///</summary>
        private static IEnumerator _fn_createAndPlayClip(string _filename, CCM_core.CCM_trackTypes_enum _folderType)
        {
            CCM_core.CCM_loadingAudio = true;

            CCM_core.CCM_fnc_log.withTime.debug("CCM_fnc_playMusic: _fn_createAndPlayClip: Called for song: " + _filename);


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


                CCM_core.CCM_fnc_log.withTime.debug("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request is done for " + _filename);

                if (_request.error != null)
                {
                    CCM_core.CCM_fnc_log.withTime.error("CCM_fnc_playMusic: _fn_createAndPlayClip: Web request encountered the following error: " + _request.error);
                    yield break;
                }


                AudioClip _clip = DownloadHandlerAudioClip.GetContent(_request);

                GameObject _musicHandler = CCM_core.CCM_fnc_getMusicHandler();
                CCM_core.CCM_fnc_log.info("CCM_fnc_playMusic: _fn_createAndPlayClip: Music handler for " + _filename + " is named: " + _musicHandler.name);
                AudioSource _handlerAudioSource = _musicHandler.GetComponent<AudioSource>();
                CCM_core.CCM_fnc_log.info("CCM_fnc_playMusic: _fn_createAndPlayClip: Music handler audiosource for " + _filename + " is named: " + _handlerAudioSource);


                _handlerAudioSource.clip = _clip;
                _handlerAudioSource.clip.name = _filename;

                _handlerAudioSource.Play();
                CCM_core.CCM_fnc_log.debug("CCM_fnc_playMusic: _fn_createAndPlayClip: Handler told to play: " + _filename);
                CCM_core.CCM_spawn_fadeAudioSource(_handlerAudioSource, 3, 0.5f);
                CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler = _musicHandler;
                CCM_core.CCM_MusicHandlers.nowPlayingAudioSource = _handlerAudioSource;


                CCM_core.CCM_loadingAudio = false;
            }

            yield break;
            
        }
    }
}