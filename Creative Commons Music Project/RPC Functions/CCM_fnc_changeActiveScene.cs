/* ----------------------------------------------------------------------------
Function: CCM_fnc_changeActiveScene

Description:
	Adjusts the dictionary entry in CCM_dictionary_activePlayerScenes that tells
     what scene a player is currently in.

Parameters:
	0: _sceneName <STRING> - The name of the scene
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
        [PunRPC]
        internal static void CCM_fnc_changeActiveScene(string _sceneName, int _playerId)
        {
            bool _playerInDictionary = CCM_core.CCM_dictionary_activePlayerScenes.ContainsKey(_playerId);
            if (_playerInDictionary)
            {
                CCM_core.CCM_dictionary_activePlayerScenes.Add(_playerId, _sceneName);
            }
            else
            {
                CCM_core.CCM_dictionary_activePlayerScenes[_playerId] = _sceneName;
            }
        }

        internal void CCM_fnc_changeActiveSceneQuery(string _sceneName, int _playerId)
        {
            PhotonView photonView = PhotonView.Get(this);

            photonView.RPC(
                "CCM_fnc_changeActiveScene",
                PhotonTargets.AllViaServer,
                new object[] { _sceneName, _playerId }
            );
        }

    }
}

/*
    CCM_getPhotonViewGameObject.AddComponent<PhotonView>();
    DontDestroyOnLoad(CCM_getPhotonViewGameObject);
    CCM_photonView = CCM_getPhotonViewGameObject.GetOrAddComponent<PhotonView>();
    CCM_photonView = PhotonView.Get(PhotonNetwork.player);

*/