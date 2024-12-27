using System;
using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.Internal.MapEditor
{
    public class MapEditorUI : MonoBehaviour
    {
        [SerializeField] private MapEditorUIMainPanel mainPanel;
        [SerializeField] private MapEditorUILibraryPanel libraryPanel;
        [SerializeField] private MapEditorUISpawnedObjectsPanel spawnedObjectsPanel;

        public static MapEditorUI Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            ClosePanel();
        }

        public void ClosePanel()
        {
            CloseMainPanel();
            CloseLibraryPanel();
            CloseSpawnedObjectPanel ();
        }

        public void CloseMainPanel()
        {
            mainPanel.ClosePanel();
        }
        
        public void CloseLibraryPanel()
        {
            libraryPanel.ClosePanel();
        }

        public void CloseSpawnedObjectPanel () {
            spawnedObjectsPanel.ClosePanel ();
        }
        
        public void InitializeMainPanel(ScenarioType type)
        {
            ClosePanel();
            mainPanel.Setup(type);
        }

        public void InitializeLibraryPanel (MapEditorObjectType libraryPanelType) {
            MapEditor.Instance.SelectObjectToSpawn(null);
            MapEditor.Instance.SelectObjectToEdit(null);
            libraryPanel.Setup(libraryPanelType);
        }

        public void UpdateHierarchy(GameObject selected = null)
        {
            spawnedObjectsPanel.Setup (selected);
        }
    }
}