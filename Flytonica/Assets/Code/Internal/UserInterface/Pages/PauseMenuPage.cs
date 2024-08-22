using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using FishNet;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class PauseMenuPage : Page
    {
        [SerializeField] private PauseController pauseController;
        [SerializeField] private Button toMainMenuButton, teacherHelpButton, returnToGameButton, keyBindingButton;

        protected new void Awake()
        {
            base.Awake();
            toMainMenuButton.onClick.AddListener(ToMainMenuButton);
            returnToGameButton.onClick.AddListener(ReturnToGame);
            gameObject.SetActive(false);
        }
        
        public override void Open(bool isBack = false)
        {
            gameObject.SetActive(true);
            print(InstanceFinder.ClientManager.Clients.Values.Count);
        }

        private void ToMainMenuButton()
        {
            pauseController.Unpause();
            GameSceneManager.Instance.ToMenuSingle();
            Close();
        }

        private void ReturnToGame()
        {
            Time.timeScale = 1f;
            Close();
            pauseController.Unpause();
        }
    }
}