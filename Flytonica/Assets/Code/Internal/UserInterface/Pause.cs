using System;
using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Internal.UserInterface
{
    public class Pause : MonoBehaviour
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
            var isPaused = pauseMenuPage.gameObject.activeSelf;

            if (isPaused)
                pauseMenuPage.Close();
            else
                pauseMenuPage.Open();
        }
    }
}
