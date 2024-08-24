using Code.Internal.UserInterface.DroneHudElements;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface
{
    public class DroneHUD : MonoBehaviour
    {
        public static DroneHUD Instance { get; private set; }
        
        [Header("UI element")] 
        
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text taskText;
        [SerializeField] private TMP_Text timeText;
        
        [field: SerializeField] public ValueElement AltValueElement { get; private set; }
        [field: SerializeField] public ValueElement SpeedValueElement { get; private set; }
        [field: SerializeField] public AimElement AimElement { get; private set; }
        [field: SerializeField] public HorizonElement HorizonElement { get; private set; }
        [field: SerializeField] public ErrorElement ErrorElement { get; private set; }
        [field: SerializeField] public BatteryElement BatteryElement { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetTask(string text) => taskText.text = text;
        
        public void SetMode(string text) => modeText.text = text;
        
        public void SetTime(string text) => modeText.text = $"SECS {text}";
    }
}
