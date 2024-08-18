using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class StudentMainMenuPage: Page
    {
        [SerializeField] private Button tasksButton, singleScriptsButton, toRoomButton,
            deviceInfoButton, selectAvatarButton;
        [SerializeField] private ScriptsPage scriptsPage;

        protected override void OnOpen()
        {
            base.OnOpen();
            singleScriptsButton.onClick.AddListener(OnSingleScripts);
        }

        protected override void OnClose()
        {
            base.OnClose();
            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
        }

        private void OnSingleScripts()
        {
            scriptsPage.Init();
            scriptsPage.Open();
        }
    }
}