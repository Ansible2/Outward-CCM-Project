/* ----------------------------------------------------------------------------
Function: CCM_fnc_findMusicObjectsInScene

Description:
	Finds all the (BGM) music objects within the current scene and returns their
     names in a list.

Parameters:
	0: _findAll <BOOL> - If set to true, all GameObjects will be searched through (which is very slow)
        If false, only the GameObjects listed in the CCM_combatMusicList will be searched for.

Returns:
	<LIST of STRINGS> - Names of all music objects in a list

Examples:
    (begin example)
		var _listOfMusicObjectNames = CCM_fnc_findMusicObjectsInScene(true);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static List<GameObject> CCM_fnc_findMusicObjectsInScene(bool _findAll = false)
        {
            List<GameObject> _myList = new List<GameObject>();
            if (_findAll)
            {
                _myList = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(x => x.name.StartsWith("BGM_")).ToList();
            }
            else
            {
                // collect names of already found objects for comparison
                // lists are by default pass by ref in classes
                List<string> _myListNames = new List<string>();
                foreach (var _y in _myList)
                {
                    _myListNames.Add(_y.name);
                }

                foreach (var _x in CCM_combatMusicList)
                {
                    if (!_myListNames.Contains(_x))
                    {
                        var _theObject = GameObject.Find(_x);
                        if (_theObject != null)
                        {
                            _myList.Add(_theObject);
                            CCM_fnc_logWithTime("Added an object");
                        }
                    }
                }
            }

            return _myList;
        }

    }
}