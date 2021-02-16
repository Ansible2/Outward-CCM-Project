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


namespace creativeCommonsMusicProject
{
    partial class CCM_rpc
    {
        internal static void CCM_fnc_playMusic(string _filename, bool _canInterrupt = true)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: was called for file " + _filename);
            if (CCM_core.CCM_Dictionaries.audioClipFromString.ContainsKey(_filename))
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
                    _fn_playClip(_filename);
                } 
                else
                {
                    _fn_playClip(_filename);
                }
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_playMusic: Could not find an entry in CCM_dictionary_audioClipFromString for file: " + _filename);
            }

        }

        [PunRPC]
        internal void CCM_event_playMusic_RPC(string _filename, string _sceneFor, bool _canInterrupt = true)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic: was called...");

            if (CCM_core.CCM_currentScene.name == _sceneFor && _canInterrupt)
            {
                CCM_fnc_playMusic(_filename, _canInterrupt);
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_event_playMusic: Will not play music file: " + _filename + ". _canInterrupt is: " + _canInterrupt + " and scene for is: " + _sceneFor + " while current scene is: " + CCM_core.CCM_currentScene.name);
            }

        }

        private static void _fn_playClip(string _filename)
        {
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_playClip: Called for song: " + _filename);
            AudioClip _clip = CCM_core.CCM_Dictionaries.audioClipFromString[_filename];
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_playClip: Clip for " + _filename + " is named: " + _clip.name);

            GameObject _musicHandler = CCM_core.CCM_fnc_getMusicHandler();
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_playClip: Music handler for " + _filename + " is named: " + _musicHandler.name);
            AudioSource _handlerAudioSource = _musicHandler.GetComponent<AudioSource>();
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_playClip: Music handler audiosource for " + _filename + " is named: " + _handlerAudioSource);

            _handlerAudioSource.clip = _clip;
            _handlerAudioSource.clip.name = _filename;

            _handlerAudioSource.Play();
            CCM_core.CCM_logSource.LogMessage("CCM_fnc_playMusic: _fn_playClip: Handler told to play: " + _filename);
            CCM_core.CCM_spawn_fadeAudioSource(_handlerAudioSource, 3, 0.5f);
            CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler = _musicHandler;
            CCM_core.CCM_MusicHandlers.nowPlayingAudioSource = _handlerAudioSource;
        }
    }
}