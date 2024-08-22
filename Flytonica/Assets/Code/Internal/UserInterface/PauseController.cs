using System;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Internal.UserInterface
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private PauseMenuPage pauseMenuPage;
        [SerializeField] private InputActionReference[] pauseButtons;

        private void Awake()
        {
            foreach (var pauseButton in pauseButtons)
            {
                pauseButton.action.performed += _ => OnPauseClick();
            }
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnPauseClick();
            }
        }

        private void OnPauseClick()
        {
            if(!GameSceneManager.Instance.IsPlaying) 
                return;
            
            var isPaused = pauseMenuPage.gameObject.activeSelf;

            if (isPaused)
            {
                pauseMenuPage.Close();
                Unpause();
            }
            else
            {
                pauseMenuPage.Open();
                Pause();
            }
        }

        public void Pause()
        {
            Time.timeScale = 0;
        }

        public void Unpause()
        {
            print("Unpause()");
            Time.timeScale = 1f;
        }
    }
}
