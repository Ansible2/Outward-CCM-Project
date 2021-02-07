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

        internal static void CCM_fnc_requestTrackToPlay_RPC(int _trackType, int _playerId)
        {
            CCM_photonView.RPC(
                "CCM_fnc_requestTrackToPlay",
                PhotonTargets.MasterClient,
                new object[] { _trackType, _playerId }
            );
        }

        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay(int _trackType, int _playerId)
        {
            CCM_core.CCM_Instance.StartCoroutine(CCM_spawn_requestTrackToPlay(_trackType, _playerId));
        }

        internal static IEnumerator CCM_spawn_requestTrackToPlay(int _trackType, int _playerId)
        {

            // this still needs to be able to return tracks that are already in active scenes for other players
            // also the below use of containsKey is not really all that effective as if a player already has a 
                //Scene in the dictionary, it'll just use that one. SHould just pass as a param









            if (!CCM_core.CCM_dictionary_activePlayerScenes.ContainsKey(_playerId))
            {
                // wait until player has sent scene info to master client
                while (!CCM_core.CCM_dictionary_activePlayerScenes.ContainsKey(_playerId))
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: waiting for _playerId to send scene info...");
                    // sleep 0.3 second
                    yield return new WaitForSeconds(0.3f);
                }
            }

            string _playersScene = "";
            if (!CCM_core.CCM_dictionary_activePlayerScenes.TryGetValue(_playerId, out _playersScene))
            {
                CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: Encountered error while trying to get scene value");
            }

            bool _updateDictionaries = true;
            // if scene is already getting music selected for it
            if (CCM_list_scenesChoosingMusicFor.Contains(_playersScene))
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

            string _randomTrack;
            if (_updateDictionaries)
            { 
                List<string> _possibleTracks = CCM_core.CCM_fnc_getAllAvailableReplacementTracks(_trackType);
                _randomTrack = CCM_core.CCM_fnc_grabRandomTrack(_possibleTracks);

                if (CCM_core.CCM_dictionary_activeScenesCurrentMusic.ContainsKey(_playersScene))
                {
                    CCM_core.CCM_dictionary_activeScenesCurrentMusic[_playersScene] = _randomTrack;
                }
                else
                {
                    CCM_core.CCM_dictionary_activeScenesCurrentMusic.Add(_playersScene, _randomTrack);
                }

                CCM_list_scenesChoosingMusicFor.Remove(_playersScene);
            }
            else
            {
                _randomTrack = CCM_core.CCM_dictionary_activeScenesCurrentMusic[_playersScene];
            }

            CCM_photonView.RPC(
                "CCM_fnc_requestTrackToPlay_sendBack",
                PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                new object[] { _randomTrack }
            );
        }

        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay_sendBack(string _trackFilename)
        {
            CCM_core.CCM_trackToPlay = _trackFilename;
        }
    }
}