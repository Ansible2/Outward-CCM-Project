/* ----------------------------------------------------------------------------
Function: CCM_fnc_changeActiveScene

Description:
	Adjusts the dictionary entry in CCM_dictionary_activePlayerScenes that tells
     what scene a player is currently in.

Parameters:
	0: _goingToSceneName <STRING> - The name of the scene that the player is going to
    1: _player <PhotonPlayer> - The player to change

Returns:
	NOTHING 

Examples:
    (begin example)
		CCM_fnc_changeActiveScene("a scene name",PhotonNetwork.player)
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
namespace creativeCommonsMusicProject
{
    partial class CCM_rpc
    {
        [PunRPC] // PunRPC functions need to be non-static
        internal void CCM_fnc_changeActiveScene(string _goingToSceneName, int _playerId)
        {
            CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: called for scene: " + _goingToSceneName + " on player ID: " + _playerId);
            bool _playerInDictionary = CCM_core.CCM_Dictionaries.activePlayerScenes.ContainsKey(_playerId);

            string _previousScene = "";
            if (_playerInDictionary)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: player ID: " + _playerId + " was in the dictionary.");
                _previousScene = CCM_core.CCM_Dictionaries.activePlayerScenes[_playerId];

                if (_goingToSceneName != _previousScene)
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: player ID: " + _playerId + " will have active scene changed as previous scene is different.");
                    CCM_core.CCM_Dictionaries.activePlayerScenes[_playerId] = _goingToSceneName;
                }
                else
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: player ID: " + _playerId + " does not need active scene updated as the scene is the same as previous.");
                }

            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: player ID: " + _playerId + " was not in the dictionary, adding...");
                CCM_core.CCM_Dictionaries.activePlayerScenes.Add(_playerId, _goingToSceneName);
            }

            bool _previousSceneStillActive = CCM_core.CCM_Dictionaries.activePlayerScenes.ContainsValue(_previousScene);
            if (!_previousSceneStillActive)
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: Scene: " + _previousScene + " is no longer active for any players");
                if (CCM_core.CCM_Dictionaries.sceneRoutines.ContainsKey(_previousScene))
                {
                    CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: Found players prior scene: " + _previousScene + " is NO LONGER active. Stopping routine...");
                    CCM_core.CCM_fnc_stopMusicRoutine(_previousScene);
                    CCM_core.CCM_Dictionaries.activeScenesTrackType.Remove(_previousScene);
                    CCM_core.CCM_Dictionaries.activeScenesCurrentTrack.Remove(_previousScene);
                }
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("CCM_fnc_changeActiveScene: Found that player prior scene: " + _previousScene + " is still active...");
            }
        }

        internal static void CCM_fnc_changeActiveScene_RPC(string _goingToSceneName, int _playerId)
        {
            CCM_photonView.RPC(
                "CCM_fnc_changeActiveScene",
                PhotonTargets.MasterClient,
                new object[] { _goingToSceneName, _playerId }
            );
        }

    }
}