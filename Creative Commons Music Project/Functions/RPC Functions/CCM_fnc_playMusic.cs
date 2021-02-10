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
        [PunRPC]
        internal void CCM_fnc_playMusic(string _filename, bool _canInterrupt = true)
        {
            if (CCM_core.CCM_Dictionaries.audioClipFromString.ContainsKey(_filename))
            {
                bool _musicIsPlaying = CCM_core.CCM_MusicHandlers.nowPlayingAudioSource.isPlaying;
                if (_musicIsPlaying && !_canInterrupt)
                {
                    CCM_core.CCM_fnc_logWithTime("Did not play file: " + _filename + " because music is already playing and could not interrupt.");
                }
                else if (_musicIsPlaying && _canInterrupt)
                {
                    CCM_core.CCM_spawn_fadeAudioSource(CCM_core.CCM_MusicHandlers.nowPlayingAudioSource);
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

        private static void _fn_playClip(string _filename)
        {
            AudioClip _clip = CCM_core.CCM_Dictionaries.audioClipFromString[_filename];
            GameObject _musicHandler = CCM_core.CCM_fnc_getMusicHandler();
            AudioSource _handlerAudioSource = _musicHandler.GetComponent<AudioSource>();

            _handlerAudioSource.clip = _clip;
            _handlerAudioSource.clip.name = _filename;

            _handlerAudioSource.Play();
            CCM_core.CCM_spawn_fadeAudioSource(_handlerAudioSource, 3, 0.5f);
            CCM_core.CCM_MusicHandlers.nowPlayingMusicHandler = _musicHandler;
            CCM_core.CCM_MusicHandlers.nowPlayingAudioSource = _handlerAudioSource;
        }
    }
}