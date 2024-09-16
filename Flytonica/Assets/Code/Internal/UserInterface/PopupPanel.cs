using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface
{
    public class PopupPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText, descriptionText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Sprite standardButtonSprite;
        [SerializeField] private Color standardButtonColor = Color.white;
        [SerializeField] private Color standardButtonTextColor = Color.black;

        [Header("Left Button elements")] [SerializeField]
        private Button leftButton;

        [SerializeField] private Image leftButtonImage;
        [SerializeField] private TMP_Text leftButtonText;

        [Header("Right Button elements")] [SerializeField]
        private Button rightButton;

        [SerializeField] private Image rightButtonImage;
        [SerializeField] private TMP_Text rightButtonText;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Awake()
        {
            cancelButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            cancelButton.onClick.RemoveListener(Hide);
        }

        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        public void SetDescription(string description)
        {
            if (descriptionText != null)
                descriptionText.text = description;
        }

        public void SetLeftButton(UnityEngine.Events.UnityAction onClickAction = null, string text = null,
            Sprite sprite = null, Color? buttonColor = null,
            Color? textColor = null)
        {
            if (leftButtonImage != null)
            {
                leftButtonImage.sprite = sprite ?? standardButtonSprite;
                leftButtonImage.color = buttonColor ?? standardButtonColor;
            }

            if (leftButtonText != null)
            {
                leftButtonText.text = text ?? "Left";
                leftButtonText.color = textColor ?? standardButtonTextColor;
            }

            if (leftButton != null && onClickAction != null)
            {
                leftButton.onClick.RemoveAllListeners();
                leftButton.onClick.AddListener(onClickAction);
            }
        }

        public void SetRightButton(UnityEngine.Events.UnityAction onClickAction = null, string text = null,
            Sprite sprite = null, Color? buttonColor = null,
            Color? textColor = null)
        {
            if (rightButtonImage != null)
            {
                rightButtonImage.sprite = sprite ?? standardButtonSprite;
                rightButtonImage.color = buttonColor ?? standardButtonColor;
            }

            if (rightButtonText != null)
            {
                rightButtonText.text = text ?? "Right";
                rightButtonText.color = textColor ?? standardButtonTextColor;
            }

            if (rightButton != null && onClickAction != null)
            {
                rightButton.onClick.RemoveAllListeners();
                rightButton.onClick.AddListener(onClickAction);
            }
        }

        public void ConfigurePopup(string title, string description,
            Sprite leftButtonSprite = null, string leftButtonText = null, Color? leftButtonColor = null,
            Color? leftButtonTextColor = null, UnityEngine.Events.UnityAction leftButtonAction = null,
            Sprite rightButtonSprite = null, string rightButtonText = null, Color? rightButtonColor = null,
            Color? rightButtonTextColor = null, UnityEngine.Events.UnityAction rightButtonAction = null)
        {
            SetTitle(title);
            SetDescription(description);
            SetLeftButton(leftButtonAction, leftButtonText, leftButtonSprite, leftButtonColor, leftButtonTextColor);
            SetRightButton(rightButtonAction, rightButtonText, rightButtonSprite, rightButtonColor,
                rightButtonTextColor);
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
    }
}