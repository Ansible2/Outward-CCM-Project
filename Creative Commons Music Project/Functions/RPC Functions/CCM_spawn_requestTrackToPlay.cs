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

        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay_RPC
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_requestTrackToPlay_RPC(int _trackType, int _playerId, string _playersScene)
        {
            CCM_photonView.RPC(
                "CCM_spawn_requestTrackToPlay",
                PhotonTargets.MasterClient,
                new object[] { _trackType, _playerId , _playersScene }
            );
        }


        /* ----------------------------------------------------------------------------
            CCM_spawn_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        [PunRPC]
        internal void CCM_spawn_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {
            CCM_core.CCM_Instance.StartCoroutine(CCM_fnc_requestTrackToPlay(_trackType, _playerId, _playersScene));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        internal static IEnumerator CCM_fnc_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {
            bool _trackTypeChanged = false;
            if (CCM_core.CCM_Dictionaries.activeScenesTrackType.ContainsKey(_playersScene))
            {
                if (CCM_core.CCM_Dictionaries.activeScenesTrackType[_playersScene] != _trackType)
                {
                    _trackTypeChanged = true;
                    CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: track type was changed for " + _playersScene);
                }
            }
            else
            {
                CCM_core.CCM_Dictionaries.activeScenesTrackType.Add(_playersScene, _trackType);
            }



            // this still needs to be able to return tracks that are already in active scenes for other players
            bool _sceneHasCurrentMusic = CCM_core.CCM_Dictionaries.activeScenesCurrentMusic.ContainsKey(_playersScene);
            bool _sceneMusicIsBeingChosen = CCM_list_scenesChoosingMusicFor.Contains(_playersScene);
            if (_sceneHasCurrentMusic && !_sceneMusicIsBeingChosen && !_trackTypeChanged)
            {
                string _sceneTrackFileName = CCM_core.CCM_Dictionaries.activeScenesCurrentMusic[_playersScene];
                CCM_photonView.RPC(
                    "CCM_fnc_playMusic",
                    PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                    new object[] { _sceneTrackFileName, true }
                );
            }
            else
            {

                bool _updateDictionaries = true;
                // if scene is already getting music selected for it, then wait
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
                        CCM_core.CCM_Dictionaries.activeScenesCurrentMusic[_playersScene] = _randomTrackFilename;
                    }
                    else
                    {
                        CCM_core.CCM_Dictionaries.activeScenesCurrentMusic.Add(_playersScene, _randomTrackFilename);
                    }

                    CCM_list_scenesChoosingMusicFor.Remove(_playersScene);
                }
                else
                {
                    _randomTrackFilename = CCM_core.CCM_Dictionaries.activeScenesCurrentMusic[_playersScene];
                }

                CCM_photonView.RPC(
                    "CCM_fnc_playMusic",
                    PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                    new object[] { _randomTrackFilename , true}
                );

                CCM_core.CCM_spawn_startMusicRoutineForScene(_playersScene, _trackType);
            }
        }
    }
}