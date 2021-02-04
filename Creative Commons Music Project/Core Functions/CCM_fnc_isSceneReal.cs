/* ----------------------------------------------------------------------------
Function: CCM_fnc_isSceneReal

Description:
	Checks if the provided scene is not of the "lowmemory" (loading) or main menu
     scenes.

Parameters:
	0: _scene <SCENE> - The scene you want to check

Returns:
	<BOOL> - True if scene is not main menu or loading screen

Examples:
    (begin example)
		var _sceneIsReal = CCM_fnc_isSceneReal(SceneManager.GetActiveScene());
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine.SceneManagement;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static bool CCM_fnc_isSceneReal(Scene _scene)
        {
            var _name = _scene.name.ToLower();
            CCM_fnc_logWithTime("Checking scene: " + _name);

            bool _isSceneReal = (!_name.Contains("lowmemory") && !_name.Contains("mainmenu"));
            CCM_fnc_logWithTime("Scene real? " + _isSceneReal);

            return _isSceneReal;
        }
    }
}