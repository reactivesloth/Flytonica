using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class LoginPage: Page
    {
        [SerializeField] private TMP_InputField loginField, passwordPage;
        [SerializeField] private Button loginButton, demoButton;
        [SerializeField] private Page teacherMainMenu, studentMainMenu;

        private void Start()
        {
            Open();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
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
            //TODO: Login logic
            studentMainMenu.Open();
        }

        private void OnDemo()
        {
            //TODO: Go to demo logic
            studentMainMenu.Open();
        }
    }
}