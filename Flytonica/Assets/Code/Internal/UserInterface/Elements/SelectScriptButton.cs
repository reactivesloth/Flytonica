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

        private GameObject _list;
        private Transform _buttonsParent;
        private List<SelectScriptButton> _childButtonsList;

        public SelectScriptButton ParentButton { get; private set; }
        public List<SelectScriptButton> ChildButtons { get; private set; } = new List<SelectScriptButton>();
        public bool ToggleIsOn => selectToggle.isOn;

        public event Action<SelectScriptButton> Selected;
        public event Action<SelectScriptButton> ToggleChanged;

        public State State => @object.State;

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

        public void Init(ScenarioSettings settings, string number = "#", GameObject list = null,
            bool isTaskInit = false, bool isOnToggle = false)
        {
            _list = list;
            selectToggle.gameObject.SetActive(!isTaskInit);
            selectToggle.gameObject.SetActive(isOnToggle);

            numberText.text = number;
            titleText.text = settings.name;

            selectToggle?.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void SetParent(SelectScriptButton parent)
        {
            ParentButton = parent;
            parent?.ChildButtons.Add(this);
            ParentButton?.UpdateArrowVisibility(ParentButton.State);
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
            @object.SelectWithoutNotify();
        }

        public void SetToggleState(bool isOn, bool notify = false)
        {
            if (notify)
            {
                selectToggle.isOn = isOn;
            }
            else
            {
                selectToggle.SetIsOnWithoutNotify(isOn);
            }
        }

        private void OnSelectAction()
        {
            Selected?.Invoke(this);
            selectToggle.isOn = true;
            if (ChildButtons.Count > 0)
                OpenList();
        }

        private void OnUnselectAction()
        {
            Selected?.Invoke(null);
            
            if(!selectToggle.gameObject.activeSelf)
                selectToggle.isOn = false;
            
            if (ChildButtons.Count > 0)
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

        private void OnToggleValueChanged(bool isOn)
        {
            ToggleChanged?.Invoke(this);
            UpdateParentToggle(); // Добавляем вызов обновления родительских галочек при изменении состояния
        }
        
        /// <summary>
        /// Обновляет состояние родительской галочки на основе состояния дочерних галочек.
        /// Если у родителя нет выбранных дочерних элементов, снимаем его галочку.
        /// Если хотя бы один потомок выбран, ставим галочку родителю.
        /// Затем рекурсивно поднимаемся вверх по иерархии.
        /// </summary>
        public void UpdateParentToggle()
        {
            if (ParentButton == null)
                return;

            bool anyChildSelected = false;
            foreach (var child in ParentButton.ChildButtons)
            {
                if (child.ToggleIsOn)
                {
                    anyChildSelected = true;
                    break;
                }
            }

            // Обновляем состояние родителя без уведомлений, чтобы не вызвать лишние события
            ParentButton.SetToggleState(anyChildSelected, false);

            // Рекурсивно обновляем родителей
            ParentButton.UpdateParentToggle();
        }

        private void OnDisable()
        {
            @object.SelectAction -= OnSelectAction;
            @object.UnselectAction -= OnUnselectAction;
            @object.StateChanged -= UpdateArrowVisibility;
            //selectToggle?.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        public void Select()
        {
            @object.Select();
            OpenList();
        }

        public void UnSelected()
        {
            @object.ToNormal();
            CloseList();
        }
    }
}