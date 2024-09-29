using System;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DebugSceneRunner
{
    private const string MAIN_SCENE_NAME = "Main";

    static DebugSceneRunner()
    {
        EditorApplication.playModeStateChanged += StateChange;
    }

    static void StateChange(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.EnteredPlayMode)
        {
            UnloadNonMainScenesInPlaymode();
        }
    }

    private static void UnloadNonMainScenesInPlaymode()
    {
        Debug.Log(" ============== DebugSceneRunner: All non-main scenes are unloaded here ==============");
        bool isMainLoaded = false;
        for (int i = SceneManager.sceneCount - 1; i >= 0; --i)
        {
            var scene = SceneManager.GetSceneAt(i);
            var currSceneName = scene.name;

            if (!currSceneName.Equals(MAIN_SCENE_NAME, StringComparison.InvariantCultureIgnoreCase))
            {
                if (scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
            else
            {
                isMainLoaded = true;
            }
        }

        if (!isMainLoaded)
        {
            SceneManager.LoadSceneAsync(MAIN_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}
#endif