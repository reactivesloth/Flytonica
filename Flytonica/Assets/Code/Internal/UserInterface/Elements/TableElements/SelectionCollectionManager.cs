using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements.TableElements
{
    public class SelectionCollectionManager : MonoBehaviour
    {
        [SerializeField] private List<TableButton> tableRows;
        [SerializeField] private TableButton buttonPrefab;
        
        
        private TableButton _selectedButton;

        public TableButton SelectedButton => _selectedButton;
        
        private void Start()
        {
            foreach (var row in tableRows)
            {
                var interactiveObject = row.GetComponent<InteractiveObjectView>();
                interactiveObject.OnPressAction += () => OnRowSelected(row);
            }
        }

        public void Generate<T>(List<TableButtonGenerateData<T>> datas)
        {
            Clear();
            var number = 1;
            foreach (var data in datas)
            {
                var button = Instantiate(buttonPrefab, transform);
                button.InitValues(number, data.DisplayData);
                button.InitSaveData(data.Data);
                tableRows.Add(button);
                number++;
            }
        }

        private void Clear()
        {
            if (_selectedButton != null)
            {
                var previousInteractiveObject = _selectedButton.GetComponent<InteractiveObjectView>();
                previousInteractiveObject.ToNormal();
                _selectedButton = null;
            }

            foreach (var button in tableRows)
            {
                Destroy(button.gameObject);
            }

            tableRows.Clear();
        }

        private void OnRowSelected(TableButton selectedButton)
        {
            if (_selectedButton != null && _selectedButton != selectedButton)
            {
                var previousInteractiveObject = _selectedButton.GetComponent<InteractiveObjectView>();
                previousInteractiveObject.ToNormal();
            }

            _selectedButton = selectedButton;
            var interactiveObjectView = _selectedButton.GetComponent<InteractiveObjectView>();
            interactiveObjectView.Select();
        }
    }

    public struct TableButtonGenerateData<T>
    {
        public string[] DisplayData;
        public T Data;

        public TableButtonGenerateData(string[] displayData, T data)
        {
            DisplayData = displayData;
            Data = data;
        }
    }
}