using System;
using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.API.Wrappers.SendModels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class LicenceActivationPage : Page
    {
        [SerializeField] private Page loginPage;

        [SerializeField] private TMP_InputField keyInputField;
        [SerializeField] private Button activateButton;
        [SerializeField] private GameObject loadLicenceScreen;

        protected void Start()
        {
            Open();
        }

        private void OnEnable()
        {
            activateButton.onClick.AddListener(OnActivate);
        }

        private void OnDisable()
        {
            activateButton.onClick.RemoveListener(OnActivate);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            loadLicenceScreen.SetActive(true);
            HttpClient.Get(LinkConstants.DeviceCheckUrl(SystemInfo.deviceUniqueIdentifier), _ =>
                {
                    print(200);
                    OnLicenceValid();
                },
                (_, code) =>
                {
                    if (code == 404)
                        OnLicenceInvalid();
                    else
                        ErrorGetLicence();
                }, () => loadLicenceScreen.SetActive(false));
        }

        private void OnActivate()
        {
            var deviceInfo = new CreateDeviceRequestData(SystemInfo.deviceUniqueIdentifier);
            Debug.Log(
                $"Input text: {SystemInfo.deviceUniqueIdentifier} {keyInputField.text} \n {JsonUtility.ToJson(deviceInfo)}");
            HttpClient.Post(
                LinkConstants.DeviceCreateUrl(new Dictionary<string, string> { { "code", keyInputField.text } }),
                JsonUtility.ToJson(deviceInfo),
                response =>
                {
                    var data = JsonUtility.FromJson<DeviceData>(response);
                    SaveCheckActivationDate(DateTime.Today);
                    
                    ToLogin();
                }, (response, code) =>
                {
                    var errorData = JsonUtility.FromJson<ErrorData>(response);
                    Debug.LogError(response);
                    MakeError(errorData.detail, OnActivate);
                });
        }

        private void OnLicenceValid()
        {
            ToLogin();
        }

        private void OnLicenceInvalid()
        {
            
        }

        private void ErrorGetLicence()
        {
            if (!IsMoreThanFiveDays)
                ToLogin();
            else
                MakeError("Для обновление вашей лицензии требуется подключение к интернету", OnOpen);
        }

        private void MakeError(string error, UnityAction action)
        {
            PopupPanel.ConfigurePopup("Ошибка",
                error, 
                leftButtonAction: action, leftButtonText:"Повторить",
                rightButtonText:"Закрыть");
        }

        private void ToLogin() => loginPage?.Open();

        private void SaveCheckActivationDate(DateTime date) =>
            PlayerPrefs.SetString("Licence", date.ToShortDateString());

        private DateTime GetCheckActivationDate =>
            DateTime.TryParse(PlayerPrefs.GetString("Licence"), out var parsedDate)
                ? parsedDate
                : DateTime.MinValue;

        private bool IsMoreThanFiveDays => (DateTime.Now - GetCheckActivationDate).TotalDays > 5;
    }
}