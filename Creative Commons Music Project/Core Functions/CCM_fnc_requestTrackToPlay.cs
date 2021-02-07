/* ----------------------------------------------------------------------------
Function: CCM_fnc_requestTrackToPlay

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

    partial class CCM_rpc
    {
        internal static List<string> CCM_list_scenesChoosingMusicFor = new List<string>();

        internal static void CCM_fnc_requestTrackToPlay_RPC(int _trackType, int _playerId, string _playersScene)
        {
            CCM_photonView.RPC(
                "CCM_fnc_requestTrackToPlay",
                PhotonTargets.MasterClient,
                new object[] { _trackType, _playerId , _playersScene }
            );
        }

        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {
            CCM_core.CCM_Instance.StartCoroutine(CCM_spawn_requestTrackToPlay(_trackType, _playerId, _playersScene));
        }

        internal static IEnumerator CCM_spawn_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {

            // this still needs to be able to return tracks that are already in active scenes for other players
            bool _sceneHasCurrentMusic = CCM_core.CCM_dictionary_activeScenesCurrentMusic.ContainsKey(_playersScene);
            bool _sceneMusicIsBeingChosen = CCM_list_scenesChoosingMusicFor.Contains(_playersScene);
            if (_sceneHasCurrentMusic && !_sceneMusicIsBeingChosen)
            {
                string _sceneTrackFileName = CCM_core.CCM_dictionary_activeScenesCurrentMusic[_playersScene];
                CCM_photonView.RPC(
                    "CCM_fnc_requestTrackToPlay_sendBack",
                    PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                    new object[] { _sceneTrackFileName }
                );
            }
            else
            {

                bool _updateDictionaries = true;
                // if scene is already getting music selected for it
                if (_sceneMusicIsBeingChosen)
                {
                    _updateDictionaries = false;
                    while (CCM_list_scenesChoosingMusicFor.Contains(_playersScene))
                    {
                        CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: waiting for _playersScene to have track selected from other call");
                        // sleep 0.3 second
                        yield return new WaitForSeconds(0.3f);
                    }
                }
                else
                {
                    CCM_list_scenesChoosingMusicFor.Add(_playersScene);
                }

                string _randomTrackFilename;
                if (_updateDictionaries)
                { 
                    List<string> _possibleTracks = CCM_core.CCM_fnc_getAllAvailableReplacementTracks(_trackType);
                    _randomTrackFilename = CCM_core.CCM_fnc_grabRandomTrack(_possibleTracks);

                    if (_sceneHasCurrentMusic)
                    {
                        CCM_core.CCM_dictionary_activeScenesCurrentMusic[_playersScene] = _randomTrackFilename;
                    }
                    else
                    {
                        CCM_core.CCM_dictionary_activeScenesCurrentMusic.Add(_playersScene, _randomTrackFilename);
                    }

                    CCM_list_scenesChoosingMusicFor.Remove(_playersScene);
                }
                else
                {
                    _randomTrackFilename = CCM_core.CCM_dictionary_activeScenesCurrentMusic[_playersScene];
                }

                CCM_photonView.RPC(
                    "CCM_fnc_requestTrackToPlay_sendBack",
                    PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                    new object[] { _randomTrackFilename }
                );
            }
        }

        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay_sendBack(string _trackFilename)
        {
            CCM_core.CCM_trackToPlay = _trackFilename;
        }
    }
}