using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface
{
    public class Page : MonoBehaviour
    {
        public static Page CurrentPage;
        public static Page PrevPage;
        
        [Header("Page base elements: ")]
        [SerializeField] [CanBeNull] private Page prevPage;
        [SerializeField] [CanBeNull] private Button backButton;
        
        protected void Awake()
        {
            backButton?.onClick.AddListener(OnBackClick);
        }

        protected void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClick);
        }

        public void Open()
        {
            PrevPage = CurrentPage;
            CurrentPage = this;
            gameObject.SetActive(true);
            OnOpen();
            
        }

        protected void Close()
        {
            gameObject.SetActive(false);
            OnClose();
        }

        protected virtual void OnOpen()
        {
            
        }
        
        protected virtual void OnClose()
        {
            
        }

        protected virtual void OnBackClick()
        {
            print(CurrentPage.gameObject.name);
            CurrentPage.Close();
            if (!prevPage)
                PrevPage?.Open();
            else
                prevPage?.Open();
        }
    }
}