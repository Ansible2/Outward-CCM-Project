/* ----------------------------------------------------------------------------
Function: CCM_fnc_areListsTheSame

Description:
	Compares two lists of game objects to see if they have the same contents

Parameters:
	0: _list1 <LIST of GameObjects> - First list of game objects
    1: _list2 <LIST of GameObjects> - Second list of game objects

Returns:
	<BOOL> - True if lists are the same, false if not

Examples:
    (begin example)
		var _areListsTheSame = CCM_fnc_areListsTheSame(_list1,_list2);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;


namespace creativeCommonsMusicProject
{
    internal partial class CCM_core : BaseUnityPlugin
    {
        internal static bool CCM_fnc_areListsTheSame(List<GameObject> _list1, List<GameObject> _list2)
        {
            var _firstNotSecond = _list1.Except(_list2);
            var _secondNotFirst = _list2.Except(_list1);
            return !_firstNotSecond.Any() && !_secondNotFirst.Any();
        }
    }
}