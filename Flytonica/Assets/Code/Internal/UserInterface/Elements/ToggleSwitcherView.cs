using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitcherView : MonoBehaviour
    {
        [SerializeField] private RectTransform handleRect;
        
        [SerializeField] private RectTransform onPosition;
        [SerializeField] private RectTransform offPosition;
        
        [SerializeField] private GameObject onBackGround;
        
        [SerializeField] private Color onColor = new(203, 255, 61);
        [SerializeField] private Color offColor = Color.black;
        
        [HideInInspector][SerializeField] private Toggle toggle;
        [HideInInspector][SerializeField] private Image handleImage;

        private void OnValidate()
        {
            toggle = GetComponent<Toggle>();
            handleImage = handleRect.GetComponent<Image>();
            if (!handleImage)
                Debug.LogError("Handle Rect does not have an Image component.");
        }

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(UpdateHandlePosition);
            UpdateHandlePosition(toggle.isOn);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(UpdateHandlePosition);
        }

        private void UpdateHandlePosition(bool isOn)
        {
            handleRect.position = isOn ? onPosition.position : offPosition.position;
            onBackGround?.SetActive(isOn);
            if (handleImage != null)
                handleImage.color = isOn ? onColor : offColor;
            
        }
    }
}