using Code.Internal.UserInterface.DroneHudElements;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.UserInterface
{
    public class DroneHUD : MonoBehaviour
    {
        public static DroneHUD Instance { get; private set; }
        
        [SerializeField] private GameObject HUDPanel;
        [SerializeField] private bool keepActiveHUDPanelforXR = true;
        [field: SerializeField] public RectTransform gameUi;

        [Header("UI element")] 
        [SerializeField] private TMP_Text windText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text taskText;
        [SerializeField] private TMP_Text timeText;
        
        [field: SerializeField] public ValueElement AltValueElement { get; private set; }
        [field: SerializeField] public ValueElement SpeedValueElement { get; private set; }
        [field: SerializeField] public AimElement AimElement { get; private set; }
        [field: SerializeField] public HorizonElement HorizonElement { get; private set; }
        [field: SerializeField] public BatteryElement BatteryElement { get; private set; }
        [field: SerializeField] public MessageBox MessageBoxElement { get; private set; }
        [field: SerializeField] public SignalElement CameraSignalElement { get; private set; }
        [field: SerializeField] public SignalElement InputSignalElement { get; private set; }

        [field: SerializeField] public ValueElement HealthValueElement { get; private set; }
        [field: SerializeField] public CompassElement CompassElement { get; private set; }

        public string CurrentWindText => windText.text;
        public string CurrentTaskText => taskText.text;
        public string CurrentTimeText => timeText.text;
        
        private void Awake()
        {
            Instance = this;
        }

        public void ShowHUD(bool value)
        {
            if (keepActiveHUDPanelforXR)
            {
                if (XRSettings.isDeviceActive && XRSettings.enabled || FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include) != null)
                {
                    HUDPanel.SetActive(true);
                    return;
                }
            }

            if (HUDPanel!= null)
                HUDPanel.SetActive(value);
        }

        public void SetWind(float speed, string direction)
        {
            if (windText != null) windText.SetText($"Ветер {direction} {speed:F1} м/с");
        }

        public void SetWind(string value)
        {
            if (windText != null) windText.SetText(value);
        }

        public void SetTask(string text)
        {
            if (taskText != null) taskText.text = text;
        }

        public void SetMode(string text)
        {
            if (modeText != null) modeText.text = text;
        }

        public void SetTime(string text)
        {
            if (text == null)
            {
                timeText.text = string.Empty;
                return;
            }
            if (timeText != null) timeText.text = $"SEC {text}";
        }

        public bool IsShowing() => HUDPanel != null && HUDPanel.activeSelf;

        public void ClearMessage()
        {
            if (MessageBoxElement != null) MessageBoxElement.ClearMessage();
        }

        public void SetMessage( MessageType type,string text, float duration = 0, AudioClip clip = null, bool forcePush = false)
        {
            if (MessageBoxElement != null) MessageBoxElement.DrawMessage(type, text, duration, clip, forcePush);
        }
    }
}