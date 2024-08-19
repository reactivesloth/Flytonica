
using System;
using System.Collections.Generic;
using Code.Internal.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

namespace Code.Internal.UserInterface.Elements
{
    [RequireComponent(typeof(Button))]
    public class SelectScriptButton : MonoBehaviour
    {
        [HideInInspector][SerializeField] private Button mainButton;
        [HideInInspector][SerializeField] private InteractiveObjectView objectView;

        [Header("Own Elements:")]
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject arrowObject;
        [SerializeField] private Toggle selectToggle;

        private Sprite _standatrSprite;
        
        //private bool _isOpenList = false;
        private GameObject _list;
        private Transform _buttonsParent;
        private List<SelectScriptButton> _childButtonsList;

        public Transform ParentForButtons => _list.transform;

        public event Action<SelectScriptButton> Selected;
        
        private void OnValidate()
        {
            mainButton = GetComponent<Button>();
            objectView = GetComponent<InteractiveObjectView>();
        }

        private void OnEnable()
        {
            mainButton.onClick.AddListener(OnButtonPress);
        }
        
        public void Init(ScenarioSettings settings, GameObject list = null)
        {
            _list = list;
            arrowObject.SetActive(list != null);
            titleText.text = settings.name;
        }

        private void OnButtonPress()
        {
            OpenCloseList();
            objectView.OnPress();
            Selected?.Invoke(this);
        }
        
        private void OpenCloseList()
        {
            if(!_list) return;
            
            _list.SetActive(!_list.activeSelf);
            transform.parent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        }

        private void OnDisable()
        {
            mainButton.onClick.RemoveListener(OnButtonPress);
        }

        public void UnSelected()
        {
            objectView.ToNormal();
        }
    }
}