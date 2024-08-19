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
        //public static Page PrevPage;
        
        [Header("Page base elements: ")]
        [SerializeField] [CanBeNull] private Page forcePrevPage;
        [SerializeField] [CanBeNull] private Button backButton;
        
        protected void Awake()
        {
            backButton?.onClick.AddListener(OnBackClick);
        }

        protected void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClick);
        }

        public void Open(bool isBack = false)
        {
            if(!isBack) 
                PrevPages.Push(CurrentPage);
            CurrentPage = this;
            gameObject.SetActive(true);
            OnOpen();
            
            PrevPages.Peek()?.Close();
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
            if (!forcePrevPage)
                PrevPages?.Pop()?.Open(true);
            else
                forcePrevPage?.Open(true);
        }
    }
}