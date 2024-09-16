using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Elements.TableElements;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class EditScenariosPage : Page
    {
        [SerializeField] private SelectionCollectionManager scenariosRoot;
        [SerializeField] private Button createButton, deleteButton;
        [SerializeField] private Page createScenarioPage;

        protected override void OnOpen()
        {
            base.OnOpen();

            InitScenariosList();

            createButton.onClick.AddListener(OnCreate);
            deleteButton.onClick.AddListener(OnDelete);
        }

        protected override void OnClose()
        {
            base.OnClose();

            createButton.onClick.RemoveListener(OnCreate);
            deleteButton.onClick.RemoveListener(OnDelete);
        }

        private void OnCreate()
        {
            createScenarioPage?.Open();
        }

        private void OnDelete()
        {
            var popup = FindObjectOfType<PopupPanel>(true);
            popup.SetTitle("Удалить сценарий?");
            popup.SetDescription($"Вы уверены, что хотите удалить сценарий {scenariosRoot.SelectedButton.GetSaveData<ScenarioData>().name}? Его нельзя будет восстановить.");
            popup.SetLeftButton(() => Debug.Log("Удалить"), "Удалить");
            popup.SetRightButton(popup.Hide, "Отменить");
            popup.Show();
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

                scenariosRoot.Generate(generateData);
            }, Debug.LogError);
        }
    }
}