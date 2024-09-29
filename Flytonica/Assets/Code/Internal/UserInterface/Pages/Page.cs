using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class Page : MonoBehaviour
    {
        public static Stack<Page> PrevPages = new ();
        public static Page CurrentPage;
        
        [Header("Page base elements: ")]
        [SerializeField] [CanBeNull] private Page forcePrevPage;
        [SerializeField] [CanBeNull] private Button backButton;
        
        protected virtual void Awake()
        {
            backButton?.onClick.AddListener(OnBackClick);
        }

        protected void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClick);
        }

        public virtual void Open(bool isBack = false)
        {
            if(!isBack) 
                PrevPages.Push(CurrentPage);
            CurrentPage = this;
            gameObject.SetActive(true);
            OnOpen();
            
            PrevPages?.Peek()?.Close();
        }

        public virtual void Close()
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
            CurrentPage.Close();
            if (!forcePrevPage)
                PrevPages?.Pop()?.Open(true);
            else
                forcePrevPage?.Open(true);
        }
    }
}