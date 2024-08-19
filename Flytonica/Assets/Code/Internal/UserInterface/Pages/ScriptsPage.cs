using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ScriptsPage: Page
    {
        [Header("Conponents:")]
        [SerializeField] private Transform selectScriptParent;
        [SerializeField] private ScenarioInfoPanel infoPanel;
        [SerializeField] private Button startGameButton;
        
        [Header("Prefabs:")]
        [SerializeField] private SelectScriptButton buttonPrefab;
        [SerializeField] private GameObject listPrefab;

        protected override void OnOpen()
        {
            base.OnOpen();
            startGameButton.onClick.AddListener(OnStartGame);
        }

        protected override void OnClose()
        {
            base.OnClose();
            startGameButton.onClick.RemoveListener(OnStartGame);
        }

        public void Init(IEnumerable<MapSettings> maps)
        {
            foreach (var map in maps)
            {
                var openListButton = Instantiate(buttonPrefab, selectScriptParent);
                var list = Instantiate(listPrefab, selectScriptParent);
                list.SetActive(false);
                openListButton.Init(map.name, list);

                foreach (var scenario in map.mapScenarios)
                {
                    var scenarioButton = Instantiate(buttonPrefab, list.transform);
                    scenarioButton.Init(scenario);
                }
            }
        }

        private void OnStartGame()
        {
            
        }
    }
}