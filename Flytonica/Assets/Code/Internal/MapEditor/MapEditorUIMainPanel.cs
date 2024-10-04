using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.MapEditor
{
    public class MapEditorUIMainPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        
        [SerializeField] private Button defaultObjectsButton;
        [SerializeField] private Button spawnerPointButton;
        [SerializeField] private Button startPointButton;
        [SerializeField] private Button finishPointButton;
        [SerializeField] private Button racingGatesButton;
        [SerializeField] private Button transportObjectsButton;
        [SerializeField] private Button searchingObjectsButton;

        private void Awake()
        {
            defaultObjectsButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.Default); defaultObjectsButton.interactable = false; });
            spawnerPointButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.SpawnPoint); spawnerPointButton.interactable = false; });
            startPointButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.StartGate); startPointButton.interactable = false; });
            finishPointButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.FinishGate); finishPointButton.interactable = false; });
            racingGatesButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.RacingGate); racingGatesButton.interactable = false; });
            transportObjectsButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.TransportObject); transportObjectsButton.interactable = false; });
            searchingObjectsButton.onClick.AddListener(() => { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorObjectType.SearchingObject); searchingObjectsButton.interactable = false; });
        }

        public void ClosePanel ()
        {
            defaultObjectsButton.interactable = true;
            spawnerPointButton.interactable = true;
            startPointButton.interactable = true;
            finishPointButton.interactable = true;
            searchingObjectsButton.interactable = true;
            racingGatesButton.interactable = true;
            transportObjectsButton.interactable = true;
            
            panel.SetActive(false);
            defaultObjectsButton.gameObject.SetActive(false);
            spawnerPointButton.gameObject.SetActive(false);
            startPointButton.gameObject.SetActive(false);
            finishPointButton.gameObject.SetActive(false);
            racingGatesButton.gameObject.SetActive(false);
            transportObjectsButton.gameObject.SetActive(false);
            searchingObjectsButton.gameObject.SetActive(false);
        }
        
        public void Setup(ScenarioType scenarioType)
        {
            ClosePanel();
            
            panel.SetActive(true);
            defaultObjectsButton.gameObject.SetActive(true);
            spawnerPointButton.gameObject.SetActive(true);
            
            switch (scenarioType)
            {
                case ScenarioType.FreeFlight:
                    break;
                case ScenarioType.Tutorial:
                    break;
                case ScenarioType.Race:
                    startPointButton.gameObject.SetActive(true);
                    finishPointButton.gameObject.SetActive(true);
                    racingGatesButton.gameObject.SetActive(true);
                    break;
                case ScenarioType.Transport:
                    transportObjectsButton.gameObject.SetActive(true);
                    break;
                case ScenarioType.Searching:
                    searchingObjectsButton.gameObject.SetActive(true);
                    break;
                case ScenarioType.SearchingWithIR:
                    searchingObjectsButton.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }
}