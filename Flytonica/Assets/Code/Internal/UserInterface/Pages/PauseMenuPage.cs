using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using Code.Internal.Network;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using FishNet;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class PauseMenuPage : Page
    {
        [SerializeField] private SceneLoadingSettings sceneSettings;

        [SerializeField] private Page keyBindingPage;

        [SerializeField] private GameObject warningTextObject;

        [SerializeField] private Button toMainMenuButton,
            teacherHelpButton,
            returnToGameButton,
            keyBindingButton,
            replayButton,
            restartServerButton;

        protected new void Awake()
        {
            base.Awake();
            toMainMenuButton.onClick.AddListener(ToMainMenuButton);
            returnToGameButton.onClick.AddListener(ReturnToGame);
            replayButton.onClick.AddListener(Replay);
            teacherHelpButton.onClick.AddListener(HelpSignal);
            restartServerButton.onClick.AddListener(RestartServer);
            keyBindingButton.onClick.AddListener(OnKeyBinding);
            gameObject.SetActive(false);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            var isTeacher = HttpClient.UserData?.type == UserType.Teacher;

            warningTextObject?.SetActive(!isTeacher && sceneSettings.isTask);

            restartServerButton.gameObject.SetActive(isTeacher);

            var isOnHelpSignalButton = !isTeacher && !InstanceFinder.ServerManager.Started;
            teacherHelpButton.gameObject.SetActive(isOnHelpSignalButton);
            keyBindingButton.gameObject.SetActive(!isTeacher);
            replayButton.gameObject.SetActive(!isTeacher);
        }

        private void ToMainMenuButton()
        {
            PopupPanel.ConfigurePopup("Выйти?",
                "Вы действительно хотите выйти", null, "Выйти", Color.red,
                Color.black,
                () =>
                {
                    UIController.Instance.Unpause(false);
                    ScenarioSwitcherController.Instance.FailTask();
                }, null, "Продолжить задание", Color.green, Color.black, ReturnToGame);
            //GameSceneManager.Instance.ToMenuSingle();
        }

        private void ReturnToGame()
        {
            Time.timeScale = 1f;
            Close();
            UIController.Instance.Unpause();
        }

        private void Replay()
        {
            ReturnToGame();
            DroneController.Instance.ResetDrone();
        }

        private void HelpSignal()
        {
            DroneController.Instance.GetComponent<NetBridge>().HelpSignal();
        }

        private void RestartServer()
        {
            ReturnToGame();
            ResetController.Instance.RestartServerRequest();
        }

        private void OnKeyBinding()
        {
            keyBindingPage.Open();
        }
    }
}