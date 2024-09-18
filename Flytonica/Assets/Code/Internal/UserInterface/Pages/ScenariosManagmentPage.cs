using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements.TableElements;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ScenariosManagmentPage : Page
    {
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private SceneLoadingSettings sceneSettings;

        [SerializeField] private SelectionCollectionManager scenariosRoot;
        [SerializeField] [CanBeNull] private Button createButton, deleteButton, startButton;
        [SerializeField] private Page createScenarioPage;

        private bool _isEditMode;

        public void Init(bool isEdit)
        {
            _isEditMode = isEdit;

            createButton?.gameObject.SetActive(isEdit);
            deleteButton?.gameObject.SetActive(isEdit);

            startButton?.gameObject.SetActive(!isEdit);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            if (!_isEditMode)
                InitPreloadScenariosList();
            InitScenariosList();

            if (_isEditMode)
            {
                createButton?.onClick.AddListener(OnCreate);
                deleteButton?.onClick.AddListener(OnDelete);
                deleteButton?.gameObject.SetActive(scenariosRoot.SelectedButton);

                if (deleteButton != null) scenariosRoot.SelectionStateChange += deleteButton.gameObject.SetActive;
            }
            else
            {
                startButton?.onClick.AddListener(OnStart);

                startButton?.gameObject.SetActive(scenariosRoot.SelectedButton);
                if (startButton != null) scenariosRoot.SelectionStateChange += startButton.gameObject.SetActive;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            createButton.onClick.RemoveListener(OnCreate);
            deleteButton.onClick.RemoveListener(OnDelete);

            scenariosRoot.SelectionStateChange -= deleteButton.gameObject.SetActive;
            scenariosRoot.SelectionStateChange -= startButton.gameObject.SetActive;
        }

        private void OnCreate()
        {
            createScenarioPage?.Open();
        }

        private void OnDelete()
        {
            var popup = FindObjectOfType<PopupPanel>(true);
            popup.SetTitle("Удалить сценарий?");
            popup.SetDescription(
                $"Вы уверены, что хотите удалить сценарий {scenariosRoot.SelectedButton.GetSaveData<ScenarioData>().name}? Его нельзя будет восстановить.");
            popup.SetLeftButton(() => Debug.Log("Удалить"), "Удалить", null, Color.red, Color.white);
            popup.SetRightButton(popup.Hide, "Отменить");
            popup.Show();
        }

        private void OnStart()
        {
            //TODO: Start Game Logic
            var selectedScenario = scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>();
        }

        private void InitScenariosList()
        {
            HttpClient.Get(LinkConstants.MapConfigMultiUrl(), response =>
            {
                var scenarios = JsonUtility.FromJson<MultiScenarioDataResponse>(response).data;
                var generateData = new List<TableButtonGenerateData<ScenarioData>>();

                foreach (var scenarioData in scenarios)
                {
                    var display = new[] { scenarioData.name };
                    var data = new TableButtonGenerateData<ScenarioData>(display, scenarioData);
                    generateData.Add(data);
                }

                if(!_isEditMode)
                    scenariosRoot.Add(generateData);
                else
                    scenariosRoot.Generate(generateData);
            }, Debug.LogError);
        }


        private void InitPreloadScenariosList()
        {
            var scenarios = taskScenariosSettings.scenarios;
            var generateData = new List<TableButtonGenerateData<ScenarioSettings>>();

            foreach (var scenarioData in scenarios)
            {
                var display = new[] { scenarioData.name };
                var data = new TableButtonGenerateData<ScenarioSettings>(display, scenarioData);
                generateData.Add(data);
            }

            scenariosRoot.Generate(generateData);
        }
    }
}