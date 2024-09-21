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
        [HideInInspector] [SerializeField] private InteractiveObject @object;

        [Header("Own Elements:")] [SerializeField]
        private TMP_Text numberText;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject arrowObject, arrowObjectDown;
        [SerializeField] private Toggle selectToggle;

        //private bool _isOpenList = false;
        private GameObject _list;
        private Transform _buttonsParent;
        private List<SelectScriptButton> _childButtonsList;

        public SelectScriptButton ParentButton { get; private set; }
        public List<SelectScriptButton> ChildButtons { get; private set; } = new List<SelectScriptButton>();

        public event Action<SelectScriptButton> Selected;

        private void OnValidate()
        {   
            @object = GetComponent<InteractiveObject>();
        }

        private void OnEnable()
        {
            @object.SelectAction += OnSelectAction;
            @object.UnselectAction += OnUnselectAction;
            @object.StateChanged += UpdateArrowVisibility;
        }

        public void Init(ScenarioSettings settings, string number = "#", GameObject list = null, bool isTaskInit = false)
        {
            _list = list;
            selectToggle.gameObject.SetActive(!isTaskInit);

            numberText.text = number;
            titleText.text = settings.name;

            UpdateArrowVisibility(list == null ? State.Non : State.Selected);

            if (isTaskInit && ParentButton != null)
            {
                @object.Interactable = false;
                @object.enabled = false; 
            }
        }

        public void SetParent(SelectScriptButton parent)
        {
            ParentButton = parent;
            parent?.ChildButtons.Add(this);
        }

        public SelectScriptButton GetRootButton()
        {
            var root = this;
            while (root.ParentButton != null)
            {
                root = root.ParentButton;
            }

            return root;
        }

        public IEnumerable<SelectScriptButton> GetAllDescendants()
        {
            foreach (var child in ChildButtons)
            {
                yield return child;
                foreach (var descendant in child.GetAllDescendants())
                {
                    yield return descendant;
                }
            }
        }

        public void SelectWithoutNotify()
        {
            selectToggle.isOn = true;
            @object.SelectWithoutNotify();
        }

        private void OnSelectAction()
        {
            selectToggle.isOn = true;
            Selected?.Invoke(this);
            OpenList();
        }

        private void OnUnselectAction()
        {
            selectToggle.isOn = false;
            CloseList();
        }

        private void OpenList()
        {
            if (!_list) return;

            _list.SetActive(true);
            transform.parent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        }

        private void CloseList()
        {
            if (!_list) return;

            _list.SetActive(false);
            transform.parent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        }

        private void UpdateArrowVisibility(State state)
        {
            var hasChildren = ChildButtons is { Count: > 0 };

            if (!hasChildren)
            {
                arrowObject.SetActive(false);
                arrowObjectDown.SetActive(false);
            }
            else
            {
                if (state == State.Selected)
                {
                    arrowObject.SetActive(false);
                    arrowObjectDown.SetActive(true);
                }
                else
                {
                    arrowObject.SetActive(true);
                    arrowObjectDown.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            @object.SelectAction -= OnSelectAction;
            @object.UnselectAction -= OnUnselectAction;
            @object.StateChanged -= UpdateArrowVisibility;
        }

        public void Select()
        {
            @object.Select();
        }

        public void UnSelected()
        {
            selectToggle.isOn = false;
            @object.ToNormal();
        }
    }
}