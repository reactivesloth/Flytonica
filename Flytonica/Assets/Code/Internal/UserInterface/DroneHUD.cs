using Code.Internal.UserInterface.DroneHudElements;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface
{
    public class DroneHUD : MonoBehaviour
    {
        public static DroneHUD Instance { get; private set; }
        
        [SerializeField] private GameObject HUDPanel;
        
        [Header("UI element")] 
        
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text taskText;
        [SerializeField] private TMP_Text timeText;
        
        [field: SerializeField] public ValueElement AltValueElement { get; private set; }
        [field: SerializeField] public ValueElement SpeedValueElement { get; private set; }
        [field: SerializeField] public AimElement AimElement { get; private set; }
        [field: SerializeField] public HorizonElement HorizonElement { get; private set; }
        [field: SerializeField] public BatteryElement BatteryElement { get; private set; }
        [field: SerializeField] public MessageBox MessageBoxElement { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowHUD(bool value) => HUDPanel.SetActive(value);
        
        public void SetTask(string text) => taskText.text = text;
        
        public void SetMode(string text) => modeText.text = text;
        
        public void SetTime(string text) => timeText.text = $"SEC {text}";

        public bool IsShowing() => HUDPanel.activeSelf;

        public void ClearMessage() => MessageBoxElement.ClearMessage();
        public void SetMessage( MessageType type,string text, float duration = 0) => MessageBoxElement.DrawMessage(type, text, duration);
    }
}
