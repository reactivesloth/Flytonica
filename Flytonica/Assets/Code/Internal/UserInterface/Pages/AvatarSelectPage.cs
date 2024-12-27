using Code.Internal.Avatars.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class AvatarSelectPage : Page
    {
        [SerializeField] private AvatarsList avatarsList;

        [SerializeField] private Button selectButton;
        [SerializeField] private Image previewImage;
        
        [SerializeField] private Transform avatarsContainer;
        [SerializeField] private Button avatarButtonPrefab;

        private int _currentAvatarIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            
            selectButton.onClick.AddListener(OnSelect);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            foreach (Transform avatar in avatarsContainer)
                Destroy(avatar.gameObject);
            
            GenerateVariants();
        }

        private void GenerateVariants()
        {
            for (var index = 0; index < avatarsList.avatarsIksList.Count; index++)
            {
                var avatarInfo = avatarsList.avatarsIksList[index];
                
                var avatarButton = Instantiate(avatarButtonPrefab, avatarsContainer);
                
                avatarButton.transform.GetChild(0).GetComponent<Image>().sprite = avatarInfo.icon;
                var index1 = index;
                avatarButton.onClick.AddListener(() =>
                {
                    previewImage.sprite = avatarInfo.icon;
                    _currentAvatarIndex = index1;
                });
            }
        }
        
        
        private void OnSelect()
        {
            PlayerPrefs.SetInt("Avatar", _currentAvatarIndex);
            OnBackClick();
        }
    }
}