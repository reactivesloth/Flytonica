using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private bool isShowHeaders = true;
        
        [SerializeField] private GameObject headerCellPrefab;
        [SerializeField] private GameObject cellPrefab;
        
        [SerializeField] private GameObject contentRow;
        [SerializeField] private float fixedColumnWidth = 72f;
        
        [SerializeField] private List<string> titlesHeaders;
        [SerializeField] private List<string> testData;
        private readonly List<GameObject> _rows = new();

        private void Awake()
        {
            GenerateHeader(titlesHeaders);
            AddRow(testData.ToArray());
            AddRow(testData.ToArray());
        }
        
        private void GenerateHeader(List<string> titles)
        {
            if(!isShowHeaders)
                return;
            
            var headerRowContent = Instantiate(contentRow, transform);
            headerRowContent.transform.SetParent(transform);
            headerRowContent.transform.localScale = Vector3.one;

            AddCell(headerCellPrefab, "№", headerRowContent, fixedColumnWidth, true);

            foreach (var title in titles)
                AddCell(headerCellPrefab, title, headerRowContent, 0, false);
        }

        public void AddRow(params string[] data)
        {
            if (data.Length != titlesHeaders.Count)
            {
                Debug.LogError("Data count does not match titles count.");
                return;
            }

            var rowContent = Instantiate(contentRow, transform);
            rowContent.transform.SetParent(transform);    
            rowContent.transform.localScale = Vector3.one;
            
            AddCell(cellPrefab, (_rows.Count + 1).ToString(), rowContent, fixedColumnWidth, true);
            
            foreach (var cellData in data)
                AddCell(cellPrefab, cellData, rowContent, 0, false);
            
            _rows.Add(rowContent);
        }

        private void AddCell(GameObject prefab, string data, GameObject row, float width, bool isFixedWidth)
        {
            var cell = Instantiate(prefab, row.transform);
            
            cell.GetComponentInChildren<TMP_Text>().text = data;
        }
    }
}
