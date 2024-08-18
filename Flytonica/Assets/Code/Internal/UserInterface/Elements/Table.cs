using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{

    public class Table : MonoBehaviour
    {
        [SerializeField] private bool isShowHeaders = true;
        
        [SerializeField] private RectTransform numbers;
        [SerializeField] private RectTransform content;
        
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
            
            var headerRowContent = Instantiate(contentRow, content.transform);
            headerRowContent.transform.SetParent(content);
            headerRowContent.transform.localScale = Vector3.one;

            var hLayout = headerRowContent.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = true;
            hLayout.childForceExpandWidth = true;

            AddCell(headerCellPrefab, "№", numbers.gameObject);

            foreach (var title in titles)
                AddCell(headerCellPrefab, title, headerRowContent);
        }

        public void AddRow(params string[] data)
        {
            if (data.Length != titlesHeaders.Count)
            {
                Debug.LogError("Data count does not match titles count.");
                return;
            }

            var rowContent = Instantiate(contentRow, content.transform);
            rowContent.transform.SetParent(content);    
            rowContent.transform.localScale = Vector3.one;

            var hLayout = rowContent.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = true;
            hLayout.childForceExpandWidth = true;
            
            AddCell(cellPrefab, (_rows.Count + 1).ToString(), numbers.gameObject);
            
            foreach (var cellData in data)
                AddCell(cellPrefab, cellData, rowContent);
            
            _rows.Add(rowContent);
        }

        private void AddCell(GameObject prefab, string data, GameObject row)
        {
            var cell = Instantiate(prefab, row.transform);
            cell.GetComponentInChildren<TMP_Text>().text = data;
        }
    }
}