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
            
            var headerRow = new GameObject("HeaderRow", typeof(RectTransform));
            headerRow.transform.SetParent(content);
            headerRow.transform.localScale = Vector3.one;

            var hLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = true;
            hLayout.childForceExpandWidth = true;

            AddCell(headerCellPrefab, "№", headerRow, true);

            foreach (var title in titles)
                AddCell(headerCellPrefab, title, headerRow);
        }

        public void AddRow(params string[] data)
        {
            if (data.Length != titlesHeaders.Count)
            {
                Debug.LogError("Data count does not match titles count.");
                return;
            }

            GameObject rowObject = new GameObject("Row", typeof(RectTransform));
            rowObject.transform.SetParent(content);
            rowObject.transform.localScale = Vector3.one;

            HorizontalLayoutGroup hLayout = rowObject.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = true;
            hLayout.childForceExpandWidth = true;
            
            AddCell(cellPrefab, (_rows.Count + 1).ToString(), rowObject, true);
            
            foreach (var cellData in data)
                AddCell(cellPrefab, cellData, rowObject);
            
            _rows.Add(rowObject);
        }

        private void AddCell(GameObject prefab, string data, GameObject row, bool isFixWight = false)
        {
            var cell = Instantiate(prefab, row.transform);
            cell.GetComponentInChildren<TMP_Text>().text = data;
            if(!isFixWight) 
                return;
        }
    }
}