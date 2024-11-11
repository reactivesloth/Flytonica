
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Code.Internal.UserInterface
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Page pauseMenuPage, firstPage;
        [SerializeField] private InputActionReference[] pauseButtons;
        //[SerializeField] private GameObject drawUIPanel;
        
        private float savedAudioVolume;
        
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

            pauseMenuPage.Open(true);
            if (isPaused)
                Unpause();
            else
                Pause();
        }

        public void Pause()
        {
            print("Pause");
            mainPanel.gameObject.SetActive(true);
            pauseMenuPage.Open(true);
            DroneInput.Instance?.MenuCameraHandle(true);
            Time.timeScale = 0.01f;
            savedAudioVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }

        public void Unpause(bool isChangeCamera = true)
        {
            mainPanel.gameObject.SetActive(false);
            pauseMenuPage.Close();
            Time.timeScale = 1f;
            if(isChangeCamera)
                DroneInput.Instance?.MenuCameraHandle(false);
            AudioListener.volume = savedAudioVolume;
        }

        public void OnGameStart()
        {
            Page.CurrentPage.Close();
            mainPanel.gameObject.SetActive(false);
        }

        public void OnMainMenu()
        {
            mainPanel.gameObject.SetActive(true);
            firstPage.Open(true);
        }
    }
}
