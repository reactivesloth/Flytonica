using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class PauseMenuPage : Page
    {
        [SerializeField] private Button toMainMenuButton, teacherHelpButton, returnToGameButton, keyBindingButton;

        protected override void OnOpen()
        {
            base.OnOpen();
            toMainMenuButton.onClick.AddListener(ToMainMenuButton);
            returnToGameButton.onClick.AddListener(ReturnToGame);
        }

        protected override void OnClose()
        {
            base.OnClose();
            toMainMenuButton.onClick.RemoveListener(ToMainMenuButton);
            returnToGameButton.onClick.RemoveListener(ReturnToGame);
        }

        private void ToMainMenuButton()
        {
            //TODO: Выход меню через сцен менеджер 
        }

        private void ReturnToGame()
        {
            Close();
        }
    }
}