using System;
using System.Collections.Generic;
using Code.Internal.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    [RequireComponent(typeof(Button))]
    public class SelectScriptButton : MonoBehaviour
    {
        [HideInInspector][SerializeField] private Button mainButton;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject openListArrow;
        
        //private bool _isOpenList = false;
        private GameObject _list;
        private Transform _buttonsParent;
        private List<SelectScriptButton> _childButtonsList;

        public Transform ParentForButtons => _list.transform;
        
        private void OnValidate()
        {
            mainButton = GetComponent<Button>();
        }

        public void Init(string title, GameObject list)
        {
            _list = list;
            mainButton.onClick.AddListener(OpenCloseList);
            openListArrow.SetActive(true);
            titleText.text = title;
        }
        
        public void Init(ScenarioSettings settings)
        {
            //TODO: ButtonHandling
            openListArrow.SetActive(false);
            titleText.text = settings.name;
        }

        private void InitListOpenButton()
        {
            
        }

        private void OpenCloseList()
        {
            _list.SetActive(!_list.activeSelf);
            transform.parent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        }

        private void OnDisable()
        {
            mainButton.onClick.RemoveListener(OpenCloseList);
        }
    }
}