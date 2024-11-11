using System.Collections.Generic;
using System.Text;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ConstructorScenarioPage : Page
    {
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Button _uploadButton;

        private ScenarioSettingsData _data;
        private string _title;
        [SerializeField] private Page createScenarioPage;
        [SerializeField] private Page scenariosManagementPage;
        
        private List<SpawnedObject> _spawnedObjects = new List<SpawnedObject>();
        
        protected override void OnOpen()
        {
            base.OnOpen();

            _leaveButton.onClick.AddListener(OnLeaveButtonClick);
            _uploadButton.onClick.AddListener(OnUploadButtonClick);
        }

        protected override void OnClose()
        {
            base.OnClose();

            _leaveButton.onClick.RemoveListener(OnLeaveButtonClick);
            _uploadButton.onClick.RemoveListener(OnUploadButtonClick);
            
            MapEditor.MapEditor.Instance.UnloadMapEditor();
        }

        protected override void OnBackClick()
        {
            base.OnBackClick();
        }
        
        public void SendData(ScenarioSettingsData dataContainer, string title)
        {
            _data = dataContainer;
            _title = title;
            MapEditor.MapEditor.Instance.LoadMapEditor(_data.mapId, _data.typeId, this);
        }
        
        private void OnLeaveButtonClick()
        {
            PopupPanel.ConfigurePopup("Выйти в меню?", "Вы уверены, что хотите удалить редактируемый сценарий? Прогресс нельзя будет восстановить.", null, "Вернуться в меню", Color.red, Color.white,
                () => { scenariosManagementPage?.Open();}, null, "Продолжить", Color.green, Color.black, () => {});
        }
        
        private void OnUploadButtonClick()
        {
            PopupPanel.ConfigurePopup("Сохранить сценарий?", "Сценарий будет опубликован и доступен для назначения в заданиях.", null, "Редактирование", Color.red, Color.white,
                () =>
                {
                }, null, "Сохранить сценарий", Color.green, Color.black,
                () =>
                {
                    if (_data == null)
                    {
                        Debug.LogError("Data is null");
                        return;
                    }

                    SetSpawnedObjects();
                    
                    var jsonData = JsonUtility.ToJson(_data);
                    print(jsonData);
                    var settingsFile = Encoding.UTF8.GetBytes(jsonData);

                    var form = new WWWForm();
                    form.AddField("name", _title);
                    form.AddBinaryData("file", settingsFile, $"Scenario.json");

                    HttpClient.PostFormData(LinkConstants.MapConfigCreateUrl, form);
            
                    _spawnedObjects.Clear();
                    scenariosManagementPage?.Open(false);
                });
        }

        private void SetSpawnedObjects()
        {
            _spawnedObjects.Clear();
            var objects = FindObjectsOfType<SpawnableObject>();

            foreach (var o in objects)
            {
                _spawnedObjects.Add(new SpawnedObject(o.gameObject.name, o.transform.position, o.transform.rotation, o.transform.lossyScale, o.transform.GetSiblingIndex()));
            }

            _data.objects = _spawnedObjects;
        }
    }
}