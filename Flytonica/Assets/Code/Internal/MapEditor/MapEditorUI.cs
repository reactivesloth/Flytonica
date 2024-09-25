using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.MapEditor
{
    public class MapEditorUI : MonoBehaviour
    {
        [SerializeField] private MapEditorUIMainPanel _mainPanel;

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
            _mainPanel.ShowPanel(false);
        }

        public void InitializePanels(ScenarioType type)
        {
            _mainPanel.Setup(type);
        }
    }
}