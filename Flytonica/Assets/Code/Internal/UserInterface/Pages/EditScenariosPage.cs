using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.UserInterface.Elements.TableElements;
using UnityEngine;

namespace Code.Internal.UserInterface.Pages
{
    public class EditScenariosPage: Page
    {
        [SerializeField] private SelectionCollectionManager scenariosRoot;

        protected override void OnOpen()
        {
            base.OnOpen();
            
            HttpClient.Get(LinkConstants.MapConfigMultiUrl(), response =>
            {
                var scenarios = JsonUtility.FromJson<MultiAssignedScenarioDataResponse>(response).data;

                var generateData = new List<TableButtonGenerateData<AssignedScenarioData>>();

                foreach (var scenarioData in scenarios)
                {
                    var display = new string[] { scenarioData.scenario_name };
                    var data = new TableButtonGenerateData<AssignedScenarioData>(display, scenarioData);
                    generateData.Add(data);
                }
                
                scenariosRoot.Generate<AssignedScenarioData>(generateData);
            }, Debug.LogError);
        }

        private void GenerateTable()
        {
            //TODO: Генерация таблицы 
        }
    }
}