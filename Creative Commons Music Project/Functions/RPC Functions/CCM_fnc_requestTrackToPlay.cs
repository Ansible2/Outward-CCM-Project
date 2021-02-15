﻿/* ----------------------------------------------------------------------------
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
        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay_RPC
        ---------------------------------------------------------------------------- */
        internal static void CCM_fnc_requestTrackToPlay_RPC(int _trackType, int _playerId, string _playersScene)
        {
            CCM_photonView.RPC(
                "CCM_fnc_requestTrackToPlay",
                PhotonTargets.MasterClient,
                new object[] { _trackType, _playerId , _playersScene }
            );
        }


        /* ----------------------------------------------------------------------------
            CCM_spawn_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        internal void CCM_spawn_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {
            //CCM_core.CCM_Instance.StartCoroutine(CCM_fnc_requestTrackToPlay(_trackType, _playerId, _playersScene));
        }


        /* ----------------------------------------------------------------------------
            CCM_fnc_requestTrackToPlay
        ---------------------------------------------------------------------------- */
        [PunRPC]
        internal void CCM_fnc_requestTrackToPlay(int _trackType, int _playerId, string _playersScene)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: was called...");
            bool _startNewRoutine = false;
            bool _rpcDirectToPlayer = false;

            // Check if track type changed
            if (CCM_core.CCM_Dictionaries.activeScenesTrackType.ContainsKey(_playersScene))
            {
                if (CCM_core.CCM_Dictionaries.activeScenesTrackType[_playersScene] != _trackType)
                {
                    // start a new routine for track type changes
                    _startNewRoutine = true;
                    CCM_core.CCM_fnc_logWithTime("CCM_spawn_requestTrackToPlay: track type was changed for " + _playersScene);
                }
            }
            else
            {
                // if scene was not active we need to start a new routine
                CCM_core.CCM_Dictionaries.activeScenesTrackType.Add(_playersScene, _trackType);
                _startNewRoutine = true;
            }

            // this still needs to be able to return tracks that are already in active scenes for other players
            bool _sceneHasCurrentMusic = CCM_core.CCM_Dictionaries.activeScenesCurrentMusic.ContainsKey(_playersScene);
            bool _sceneMusicIsBeingChosen = CCM_core.CCM_Lists.scenesChoosingMusicFor.Contains(_playersScene);
            if (_startNewRoutine)
            {
                CCM_core.CCM_spawn_startMusicRoutineForScene(_playersScene, _trackType);
            }
            else if (_sceneHasCurrentMusic && !_startNewRoutine && !_sceneMusicIsBeingChosen)
            {
                _rpcDirectToPlayer = true;
            }



            if (_rpcDirectToPlayer)
            {
                string _sceneTrackFileName = CCM_core.CCM_Dictionaries.activeScenesCurrentMusic[_playersScene];
                CCM_photonView.RPC(
                    "CCM_event_playMusic",
                    PhotonNetwork.player.Get(_playerId), // Questionable if this will not just get the local player ID (RPC does take PhotonPlayer as an arguement)
                    new object[] { _sceneTrackFileName, _playersScene, true }
                );
            }
                
        }
    }
}