
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
        [HideInInspector][SerializeField] private InteractiveObject @object;

        [Header("Own Elements:")]
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject arrowObject, arrowObjectDown;
        [SerializeField] private Toggle selectToggle;
        
        //private bool _isOpenList = false;
        private GameObject _list;
        private Transform _buttonsParent;
        private List<SelectScriptButton> _childButtonsList;

        public Transform ParentForButtons => _list.transform;

        public event Action<SelectScriptButton> Selected;
        
        private void OnValidate()
        {
            mainButton = GetComponent<Button>();
            @object = GetComponent<InteractiveObject>();
        }

        private void OnEnable()
        {
            mainButton.onClick.AddListener(OnButtonPress);
        }
        
        public void Init(ScenarioSettings settings, string number = "", GameObject list = null)
        {
            _list = list;
            arrowObject.SetActive(list != null);

            numberText.text = number;
            titleText.text = settings.name;
        }

        public void OnButtonPress()
        {
            OpenCloseList();
            //@object.OnPress();
            
            arrowObject.SetActive(@object.State != State.Selected && _list != null);
            arrowObjectDown.SetActive(@object.State == State.Selected && _list != null);

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
            @object.ToNormal();
        }
    }
}