#if UNITY_EDITOR
using System;
using UltimateReplay;
using UnityEditor;
using UnityEngine;

namespace Code.Internal.Replays
{
    [InitializeOnLoad]
    public static class ReplayPrefabRegister
    {
        static ReplayPrefabRegister()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            FindReplayObjects();
        }

        private static void OnProjectChanged()
        {
            // Проверяем изменения только в папке Resources
            string[] resourcePaths = AssetDatabase.GetAllAssetPaths();
            bool resourcesChanged = false;

            foreach (string path in resourcePaths)
            {
                if (path.StartsWith("Assets/Resources"))
                {
                    resourcesChanged = true;
                    break;
                }
            }

            if (resourcesChanged)
            {
                FindReplayObjects();
            }
        }

        private static void FindReplayObjects()
        {
            var allPrefabs = Resources.LoadAll<GameObject>("");

            foreach (var prefab in allPrefabs)
            {
                if (prefab.TryGetComponent(out ReplayObject replayObject))
                {
                    try
                    {
                        ReplayManager.AddReplayPrefabAssetProvider(replayObject);
                    }
                    catch (InvalidOperationException e)
                    {
                    }
                }
            }
        }
    }
}
#endif