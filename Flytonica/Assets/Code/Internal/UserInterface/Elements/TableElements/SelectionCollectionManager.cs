using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements.TableElements
{
    public class SelectionCollectionManager : MonoBehaviour
    {
        [SerializeField] private List<TableButton> tableRows;
        [SerializeField] private TableButton buttonPrefab;
        
        public TableButton SelectedButton { get; private set; }

        public event Action<bool> SelectionStateChange; 

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
                
                var interactiveObject = button.GetComponent<InteractiveObject>();
                interactiveObject.SelectAction += () => OnRowSelected(button);
                interactiveObject.UnselectAction += () => OnRowUnselected(button);
                
                number++;
            }
        }

        private void Clear()
        {
            if (SelectedButton != null)
            {
                var previousInteractiveObject = SelectedButton.GetComponent<InteractiveObject>();
                previousInteractiveObject.ToNormal();
                SelectedButton = null;
            }

            foreach (var button in tableRows)
            {
                Destroy(button.gameObject);
            }

            tableRows.Clear();
        }

        public void OnRowSelected(TableButton selectedButton)
        {
            SelectedButton?.GetComponent<InteractiveObject>()?.ToNormal();
            SelectedButton = selectedButton;
            SelectionStateChange?.Invoke(true);
        }

        public void OnRowUnselected(TableButton unselectedButton)
        {
            if(SelectedButton != unselectedButton) return;
            SelectedButton = null;
            SelectionStateChange?.Invoke(false);
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