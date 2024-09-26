using System;
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
            defaultObjectsButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.Default); });
            spawnerPointButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.Spawner); });
            startPointButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.StartPoint); });
            finishPointButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.FinishPoint); });
            racingGatesButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.Racing); });
            transportObjectsButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.Transport); });
            searchingObjectsButton.onClick.AddListener(()=> { MapEditorUI.Instance.InitializeLibraryPanel(MapEditorUILibraryPanelType.Searching); });
        }

        public void ClosePanel ()
        {
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