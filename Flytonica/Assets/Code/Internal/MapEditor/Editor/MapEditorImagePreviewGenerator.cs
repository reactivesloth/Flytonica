#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.Internal.MapEditor.Editor
{
    public class MapEditorImagePreviewGeneratorWindow : EditorWindow
    {
        [MenuItem("Tools/Generate Prefab Icons")]
        public static void ShowWindow()
        {
            GetWindow<MapEditorImagePreviewGeneratorWindow>("Prefab Icon Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate Icons for Spawnable Objects", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Icons"))
            {
                GenerateIconsForAllPrefabs();
            }
        }

        private static void GenerateIconsForAllPrefabs()
        {
            var allPrefabs = Resources.LoadAll<GameObject>("");

            foreach (var prefab in allPrefabs)
            {
                if (!prefab.TryGetComponent(out SpawnableObject spawnableObject))
                    continue;

                if (spawnableObject.icon == null)
                {
                    MapEditorImagePreviewGenerator.GenerateAndSetIcon(spawnableObject);
                }
            }

            Debug.Log("Icon generation complete.");
        }
    }

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

        public static void CheckAndSetIcons()
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

        public static void GenerateAndSetIcon(SpawnableObject spawnableObject)
        {
            if (spawnableObject == null)
            {
                Debug.LogError("SpawnableObject is null.");
                return;
            }

            Texture2D preview = AssetPreview.GetAssetPreview(spawnableObject.gameObject);

            if (preview == null)
            {
                Debug.LogWarning($"Preview generation failed or is not ready yet. {spawnableObject.displayName} ({spawnableObject.gameObject.name})");
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

            var resourcePath = "SpawnableObjectsIcons/" + Path.GetFileNameWithoutExtension(fileName);

            var savedIcon = Resources.Load<Texture2D>(resourcePath);
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
