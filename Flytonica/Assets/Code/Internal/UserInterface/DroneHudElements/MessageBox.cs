using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{

    public enum MessageType
    {
        Normal,
        Warning,
        Error
    }
    
    [RequireComponent(typeof(AudioSource))]
    public class MessageBox : MonoBehaviour
    {
        private AudioSource _source;
        
        [SerializeField] private Color 
            normalColor = Color.white, 
            warningColor = Color.yellow, 
            errorColor = Color.red;
        
        [Header("Elements: ")] [SerializeField] private TMP_Text messageText;
        [Header("Elements: ")] [SerializeField] private Image warningImage;

        public static MessageBox Instance { get; private set; }

        public bool IsClear => messageText.text.Length < 1;
        
        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (Instance == null) Instance = this;
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneChanged;
        }

        private void SceneChanged(Scene arg0, LoadSceneMode arg1)
        {
            ClearMessage();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneChanged;
        }

        public void DrawMessage(MessageType type, string message, float duration = 0, AudioClip clip = null,
            bool forcePush = false)
        {
            CancelInvoke();
            ClearMessage();

            switch (type)
            {
                case MessageType.Normal:
                    warningImage.gameObject.SetActive(false);
                    messageText.color = normalColor;
                    break;
                case MessageType.Warning:
                    warningImage.gameObject.SetActive(true);
                    warningImage.color = warningColor;
                    messageText.color = warningColor;
                    break;
                case MessageType.Error:
                    warningImage.gameObject.SetActive(true);
                    warningImage.color = errorColor;
                    messageText.color = errorColor;
                    break;
            }

            messageText.text = message;

            if (duration > 0)
            {
                Invoke("ClearMessage", duration);
            }

            if (clip != null && (!_source.isPlaying || forcePush))
            {
                _source.Stop();
                _source.PlayOneShot(clip);
            }
        }

        public void ClearMessage()
        {
            messageText.text = string.Empty;
            warningImage.gameObject.SetActive(false);
            
        }
    }
}