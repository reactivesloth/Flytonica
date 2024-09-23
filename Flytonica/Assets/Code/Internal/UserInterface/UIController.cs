using System;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Internal.UserInterface
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [SerializeField] private Canvas canvas;
        [SerializeField] private Page pauseMenuPage, firstPage, hostControlPage;
        [SerializeField] private InputActionReference[] pauseButtons;
        [SerializeField] private GameObject drawUIPanel;

        private void Awake()
        {
            Instance = this;
            
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
                Unpause();
            else
                Pause();
        }

        public void Pause()
        {
            print("Pause");
            canvas.gameObject.SetActive(true);
            drawUIPanel.SetActive(true);
            pauseMenuPage.Open(true);
            Time.timeScale = 0;
        }

        public void Unpause()
        {
            canvas.gameObject.SetActive(false);
            drawUIPanel.SetActive(false);
            pauseMenuPage.Close();
            Time.timeScale = 1f;
        }

        public void OnGameStart()
        {
            drawUIPanel.SetActive(false);
            pauseMenuPage.Open();
            canvas.gameObject.SetActive(false);
        }

        public void OnMainMenu()
        {
            canvas.gameObject.SetActive(true);
            drawUIPanel.SetActive(true);
            firstPage.Open(true);
        }

        public void SetHostControl()
        {
            canvas.gameObject.SetActive(true);
            hostControlPage.Open();
        }
    }
}
