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

        public int _lastNumber;
        
        public void Generate<T>(List<TableButtonGenerateData<T>> datas)
        {
            Clear();
            _lastNumber = 1;
            Add(datas);
        }

        public void Add<T>(List<TableButtonGenerateData<T>> datas)
        {
            var number = _lastNumber;
            foreach (var data in datas)
            {
                Add(data);
            }

            _lastNumber = number;
        }

        public void Add<T>(TableButtonGenerateData<T> data)
        {
            var button = Instantiate(buttonPrefab, transform);
            button.InitValues(_lastNumber, data.DisplayData);
            button.InitSaveData(data.Data);
            tableRows.Add(button);
                
            var interactiveObject = button.GetComponent<InteractiveObject>();
            interactiveObject.SelectAction += () => OnRowSelected(button);
            interactiveObject.UnselectAction += () => OnRowUnselected(button);
                
            _lastNumber++;
        }

        public void Unselect()
        {
            SelectedButton?.GetComponent<InteractiveObject>()?.ToNormal();
            SelectedButton = null;
        }

        public void Clear()
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

        private void OnRowSelected(TableButton selectedButton)
        {
            print(gameObject.name);
            SelectedButton?.GetComponent<InteractiveObject>()?.ToNormal();
            SelectedButton = selectedButton;
            SelectionStateChange?.Invoke(true);
        }

        private void OnRowUnselected(TableButton unselectedButton)
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