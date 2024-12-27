using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.UserInterface.Pages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class Header : MonoBehaviour
    {
        [SerializeField] private LoginPage loginPage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button logoutButton;

        private void OnEnable()
        {
            logoutButton?.onClick.AddListener(Logout);

            if (HttpClient.IsAuthorized)
                RequestAndSetUserData();
        }

        private void OnDisable()
        {
            logoutButton?.onClick.RemoveListener(Logout);
        }

        private void Logout()
        {
            HttpClient.Logout();
            loginPage?.Open();
        }

        private void RequestAndSetUserData()
        {
            if (HttpClient.UserData == null)
                HttpClient.Get(LinkConstants.UserInfoUrl, data =>
                    {
                        HttpClient.SetUserData(JsonUtility.FromJson<UserData>(data));
                        SetData();
                    },
                    (error, code) => Debug.LogError(error));
            else
                SetData();
        }

        private void SetData()
        {
            var data = HttpClient.UserData;
            if (nameText != null)
                nameText.text = data.name;
        }
    }
}