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
        }

        protected override void OnBackClick()
        {
            base.OnBackClick();
        }
        
        public void SendData(ScenarioSettingsData dataContainer, string title)
        {
            _data = dataContainer;
            _title = title;
        }
        
        private void OnLeaveButtonClick()
        {
            ///PopupPanel.ShowDeleteTemplate(
                //Delete, "сценарий", scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>().name);
            createScenarioPage?.Open(false);
        }
        
        private void OnUploadButtonClick()
        {
            if (_data == null)
            {
                Debug.LogError("Data is null");
                return;
            }
            
            var jsonData = JsonUtility.ToJson(_data);
            print(jsonData);
            var settingsFile = Encoding.UTF8.GetBytes(jsonData);

            var form = new WWWForm();
            form.AddField("name", _title);
            form.AddBinaryData("file", settingsFile, $"Scenario.json");

            HttpClient.PostFormData(LinkConstants.MapConfigCreateUrl, form);
            
            scenariosManagementPage?.Open(false);
        }
    }
}