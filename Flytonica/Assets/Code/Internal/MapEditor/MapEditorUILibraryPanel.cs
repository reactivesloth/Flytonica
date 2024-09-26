using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace Code.Internal.MapEditor
{
    public enum MapEditorUILibraryPanelType
    {
        None,
        Default,
        Spawner,
        StartPoint,
        FinishPoint,
        Racing,
        Searching,
        Transport
    }
    public class MapEditorUILibraryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject mapEditorLibraryPanel;
        [SerializeField] private Transform content;
        [SerializeField] private MapEditorUILibraryButton buttnPrefab;
        [SerializeField] private MapEditorObjectsSettings mapEditorObjectsObjects;

        public void ClosePanel ()
        {
            mapEditorLibraryPanel.SetActive(false);
            foreach (Transform t in content)
            {
                if (t.GetComponent<MapEditorUILibraryButton>())
                    Destroy(t.gameObject);
            }
        }

        public void Setup (MapEditorUILibraryPanelType type)
        {
            ClosePanel();
            mapEditorLibraryPanel.SetActive(true);
            GameObject[] objects = { };
            
            switch (type)
            {
                case MapEditorUILibraryPanelType.None:
                    ClosePanel();
                    break;
                case MapEditorUILibraryPanelType.Default:
                    objects = mapEditorObjectsObjects.genericObjects;
                    break;
                case MapEditorUILibraryPanelType.Spawner:
                    objects = new[] { mapEditorObjectsObjects.spawnerObject };
                    break;
                case MapEditorUILibraryPanelType.StartPoint:
                    objects = new[]{ mapEditorObjectsObjects.gatesStartObject };
                    break;
                case MapEditorUILibraryPanelType.FinishPoint:
                    objects = new[] { mapEditorObjectsObjects.gatesFinishObject };
                    break;
                case MapEditorUILibraryPanelType.Racing:
                    objects = mapEditorObjectsObjects.gates;
                    break;
                case MapEditorUILibraryPanelType.Searching:
                    objects = mapEditorObjectsObjects.searchingObjects;
                    break;
                case MapEditorUILibraryPanelType.Transport:
                    objects = mapEditorObjectsObjects.transportObjects;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            foreach (GameObject o in objects)
            {
                var button = Instantiate(buttnPrefab, content).GetComponent<MapEditorUILibraryButton>();
                button.Setup(o);
            }
        }
    }
}