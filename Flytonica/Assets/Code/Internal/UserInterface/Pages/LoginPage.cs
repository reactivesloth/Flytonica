using System;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.API.Wrappers.SendModels;
using Code.Internal.UserInterface.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UserType = Code.Internal.API.Wrappers.UserType;

namespace Code.Internal.UserInterface.Pages
{
    public class LoginPage : Page
    {
        [SerializeField] private TMP_InputField loginField, passwordField;
        [SerializeField] private Button loginButton, demoButton;
        [SerializeField] private Page teacherMainMenu, studentMainMenu;
        [SerializeField] private Header teacherHeader;
        [SerializeField] private Toggle rememberMeToggleSwitcher;
        [SerializeField] private Toggle devServerToggleSwitcher;
        
        private void LoadPrefs()
        {
            loginField.text = PlayerPrefs.GetString("Login");
            passwordField.text = PlayerPrefs.GetString("Password");
            rememberMeToggleSwitcher.isOn = (PlayerPrefs.GetInt("RememberMe", 0) == 1);
            devServerToggleSwitcher.isOn = (PlayerPrefs.GetInt("DevelopmentServer", 0) == 1);
        }

        private void SetPrefs()
        {
            PlayerPrefs.SetString("RememberMe", rememberMeToggleSwitcher.isOn ? "1" : "0");
            PlayerPrefs.SetInt("DevelopmentServer", devServerToggleSwitcher.isOn ? 1 : 0);
            if (rememberMeToggleSwitcher.isOn)
            {
                PlayerPrefs.SetString("Login", loginField.text);
                PlayerPrefs.SetString("Password", passwordField.text);
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            teacherHeader.gameObject.SetActive(false);
            LoadPrefs();
            loginButton.onClick.AddListener(OnLogin);
            demoButton.onClick.AddListener(OnDemo);
            devServerToggleSwitcher.onValueChanged.AddListener(OnDevServer);
            if(HttpClient.IsAuthorized)
                Login();
        }

        private void OnDevServer(bool arg0)
        {
            PlayerPrefs.SetInt("DevelopmentServer", arg0 ? 1 : 0);
        }

        protected override void OnClose()
        {
            base.OnClose();
            loginButton.onClick.RemoveListener(OnLogin);
            demoButton.onClick.RemoveListener(OnDemo);
            devServerToggleSwitcher.onValueChanged.RemoveListener(OnDevServer);
        }

        private void OnLogin()
        {
            var jsonData = JsonUtility.ToJson(new UserAuthData(loginField.text, passwordField.text));
            HttpClient.Post(LinkConstants.AuthUrl, jsonData, OnResponseLogin, OnErrorLogin);
        }

        private void OnDemo()
        {
            studentMainMenu.Open();
        }

        private void OnResponseLogin(string response)
        {
            var authData = JsonUtility.FromJson<AuthResponseData>(response);
            HttpClient.SetAuthData(authData);

            Login();
        }

        private void Login()
        {
            switch (HttpClient.AuthData.type)
            {
                case UserType.Teacher:
                    SetPrefs();
                    teacherMainMenu.Open();
                    teacherHeader.gameObject.SetActive(true);
                    break;
                case UserType.Student:
                    SetPrefs();
                    studentMainMenu.Open();
                    break;
                case UserType.SuperAdmin:
                case UserType.Admin:
                    Debug.LogWarning("Only for Teacher or Student");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnErrorLogin(string response, long code)
        {
            Debug.LogError($"{code}: {response}");
            MakeError($"{code}: {response}", OnOpen);
        }
        
        private void MakeError(string error, UnityAction action)
        {
            PopupPanel.ConfigurePopup("Ошибка",
                error,
                leftButtonAction: action, leftButtonText: "Повторить",
                rightButtonText: "Закрыть");
        }
    }
}