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
            mainPanel.ClosePanel();
            libraryPanel.ClosePanel ();
        }

        public void InitializeMainPanel(ScenarioType type)
        {
            mainPanel.Setup(type);
        }

        public void InitializeLibraryPanel (MapEditorObjectType libraryPanelType) {
            libraryPanel.Setup(libraryPanelType);
        }
    }
}