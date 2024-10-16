#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.Internal.MapEditor.Editor
{
    [InitializeOnLoad]
    public class MapEditorImagePreviewGenerator
    {
        static MapEditorImagePreviewGenerator()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            CheckAndSetIcons();
        }

        private static void OnProjectChanged()
        {
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
                CheckAndSetIcons();
            }
        }

        private static void CheckAndSetIcons()
        {
            var allPrefabs = Resources.LoadAll<GameObject>("");

            foreach (var prefab in allPrefabs)
            {
                if (!prefab.TryGetComponent(out SpawnableObject spawnableObject))
                    continue;
                if (spawnableObject.icon == null)
                    GenerateAndSetIcon(spawnableObject);
            }
        }

        private static void GenerateAndSetIcon(SpawnableObject spawnableObject)
        {
            if (spawnableObject == null)
            {
                Debug.LogError("SpawnableObject is null.");
                return;
            }

            Texture2D preview = AssetPreview.GetAssetPreview(spawnableObject.gameObject);

            if (preview == null)
            {
                Debug.LogWarning("Preview generation failed or is not ready yet.");
                return;
            }

            string folderPath = "Assets/Resources/SpawnableObjectsIcons";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{spawnableObject.name}_Icon.png";
            string filePath = Path.Combine(folderPath, fileName);

            byte[] pngData = preview.EncodeToPNG();
            if (pngData != null)
            {
                File.WriteAllBytes(filePath, pngData);
                Debug.Log($"Icon saved at {filePath}");

                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Failed to encode the icon texture to PNG.");
                return;
            }

            string resourcePath = "SpawnableObjectsIcons/" + Path.GetFileNameWithoutExtension(fileName);

            Texture2D savedIcon = Resources.Load<Texture2D>(resourcePath);
            if (savedIcon != null)
            {
                spawnableObject.icon = savedIcon;
                Debug.Log("Icon assigned from saved resource.");
            }
            else
            {
                Debug.LogError("Failed to load saved icon from resources.");
            }
        }
    }
}
#endif