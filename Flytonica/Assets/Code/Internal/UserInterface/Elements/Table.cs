using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private bool isShowHeaders = true;

        [SerializeField] private Transform numbersParent;
        [SerializeField] private Transform contentParent;

        [SerializeField] private GameObject headerCellPrefab;
        [SerializeField] private GameObject cellPrefab;

        [SerializeField] private GameObject contentRow;
        [SerializeField] private float fixedColumnWidth = 72f;

        [SerializeField] private List<string> titlesHeaders;
        [SerializeField] private List<string> testData;
        private readonly List<GameObject> _cells = new();

        private int rowCount = 0;

        private void Awake()
        {
            GenerateHeader(titlesHeaders);
            AddRow(testData.ToArray());
            AddRow(testData.ToArray());
        }
        
        private void GenerateHeader(List<string> titles)
        {
            if (!isShowHeaders)
                return;

            var headerRowContent = Instantiate(contentRow, contentParent);

            AddCell(headerCellPrefab, "№", numbersParent.gameObject, false);

            foreach (var title in titles)
                AddCell(headerCellPrefab, title, headerRowContent,false);
        }

        public void AddRow(params string[] data)
        {
            if (data.Length != titlesHeaders.Count)
            {
                Debug.LogError("Data count does not match titles count.");
                return;
            }

            var rowContent = Instantiate(contentRow, contentParent);

            rowCount++;
            
            AddCell(cellPrefab, rowCount.ToString(), numbersParent.gameObject);

            foreach (var cellData in data)
                AddCell(cellPrefab, cellData, rowContent);
            
            _cells.Add(rowContent);
        }

        private void AddCell(GameObject prefab, string data, GameObject row, bool isContent = true)
        {
            var cell = Instantiate(prefab, row.transform);

            cell.GetComponentInChildren<TMP_Text>().text = data;
            
            if(isContent)
                _cells.Add(cell);
        }

        public void Clear()
        {
            rowCount = 0;
            
            foreach (var row in _cells)
            {
                if(row)
                    Destroy(row);
            }

            _cells.Clear();
        }
    }
}