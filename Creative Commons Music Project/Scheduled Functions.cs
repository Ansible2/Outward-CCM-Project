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
    public class CCM_scheduled : BaseUnityPlugin
    {
        CCM_core CCM_core = new CCM_core(); // how to get another class in a different file

        internal IEnumerator CCM_fnc_startCombatMusicIntercept()
        {
            CCM_core.CCM_fnc_logWithTime("Started combat music check");
            var _musicList = CCM_core.CCM_fnc_findMusicObjectsInScene();
            var _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();

            CCM_core.CCM_doRunCombatMusicCheck = true;
            while (CCM_core.CCM_doRunCombatMusicCheck)
            {
                CCM_core.CCM_fnc_logWithTime("Looping for combat music check");
                _musicListCompare = CCM_core.CCM_fnc_findMusicObjectsInScene();
                Logger.Log(LogLevel.Message, _musicListCompare.Count);
                Logger.Log(LogLevel.Message, _musicList.Count);

                if (!CCM_core.CCM_fnc_areListsTheSame(_musicList, _musicListCompare))
                {
                    CCM_core.CCM_fnc_logWithTime("Found more music");
                    // copying list instead of referencing
                    _musicList = new List<GameObject>(_musicListCompare);
                    CCM_core.CCM_fnc_logWithTime("Adjusted music list");
                }


                yield return new WaitForSeconds(0.5f);
            }
            CCM_core.CCM_fnc_logWithTime("Ended combat music check");
        }




        internal IEnumerator CCM_fnc_waitForLoadingDone(Scene _myScene)
        {
            string _mySceneName = _myScene.name;
            if (_fn_isRealScene(_myScene))
            {
                CCM_core.CCM_fnc_logWithTime("Found real scene: " + _mySceneName);

                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for loading...");
                    yield return new WaitForSeconds(1);
                }
                CCM_core.CCM_fnc_logWithTime("Loading done. Searching for music objects...");

                var _myList = CCM_core.CCM_fnc_findMusicObjectsInScene(true);

                CCM_core.CCM_fnc_logWithTime("Music Objects Found:");
                foreach (var _x in _myList)
                {
                    Logger.Log(LogLevel.Message, _x.name);
                    Logger.Log(LogLevel.Message, _x.GetComponent<AudioSource>().clip);
                }

                while (CCM_core.CCM_doRunCombatMusicCheck)
                {
                    CCM_core.CCM_fnc_logWithTime("waiting for combat music check reset...");
                    yield return new WaitForSeconds(0.1f);
                }
                //yield return new WaitUntil(() => !CCM_doRunCombatMusicCheck);
                CCM_core.CCM_fnc_logWithTime("Reached combat music check");
                StartCoroutine(CCM_fnc_startCombatMusicIntercept());
            }
            else
            {
                CCM_core.CCM_fnc_logWithTime("Skipped fake Scene: " + _mySceneName);
            }
        }

    }
}