using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using FishNet;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class PauseMenuPage : Page
    {
        [SerializeField]
        private Button toMainMenuButton, teacherHelpButton, returnToGameButton, keyBindingButton, replayButton;

        protected new void Awake()
        {
            base.Awake();
            toMainMenuButton.onClick.AddListener(ToMainMenuButton);
            returnToGameButton.onClick.AddListener(ReturnToGame);
            replayButton.onClick.AddListener(Replay);
            gameObject.SetActive(false);
        }

        private void ToMainMenuButton()
        {
            UIController.Instance.Unpause();

            if (InstanceFinder.ServerManager.Started)
                InstanceFinder.ServerManager.StopConnection(true);
            InstanceFinder.ClientManager.StopConnection();

            GameSceneManager.Instance.ToMenuSingle();
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
    }
}