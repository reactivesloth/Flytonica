using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface
{
    public class Page : MonoBehaviour
    {
        [Header("Page base params:")]
        [SerializeField] [CanBeNull] private Page _prevPage;
        [SerializeField] [CanBeNull] private Button _backButton;

        protected void Awake()
        {
            _backButton?.onClick.AddListener(OnBackClick);
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClick);
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnBackClick()
        {
            Close();
            _prevPage?.Open();
        }
    }
}