using System;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class LoginPage : Page
    {
        [SerializeField] private TMP_InputField loginField, passwordField;
        [SerializeField] private Button loginButton, demoButton;
        [SerializeField] private Page teacherMainMenu, studentMainMenu;

        private void Start()
        {
            Open();
        }

        private void LoadPrefs()
        {
            loginField.text = PlayerPrefs.GetString("Login");
            passwordField.text = PlayerPrefs.GetString("Password");
        }

        private void SetPrefs()
        {
            PlayerPrefs.SetString("Login", loginField.text);
            PlayerPrefs.SetString("Password", passwordField.text);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            LoadPrefs();
            loginButton.onClick.AddListener(OnLogin);
            demoButton.onClick.AddListener(OnDemo);
        }

        protected override void OnClose()
        {
            base.OnClose();
            loginButton.onClick.RemoveListener(OnLogin);
            demoButton.onClick.RemoveListener(OnDemo);
        }

        private void OnLogin()
        {
            var jsonData = JsonUtility.ToJson(new AuthData(loginField.text, passwordField.text));
            HttpClient.Post(LinkConstants.AuthUrl, jsonData, OnResponseLogin, OnErrorLogin);
        }

        private void OnDemo()
        {
            //TODO: Go to demo logic
            studentMainMenu.Open();
        }

        private void OnResponseLogin(string response)
        {
            var authData = JsonUtility.FromJson<AuthResponseData>(response);
            HttpClient.SetAuthData(authData);
            
            switch (authData.type)
            {
                case UserType.Teacher:
                    SetPrefs();
                    teacherMainMenu.Open();
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

        private void OnErrorLogin(string response)
        {
            Debug.LogError(response);
        }
    }
}