using System.Linq;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.MapEditor
{
    public class MapEditorUISpawnedObjectsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject mapEditorSpawnedObjectPanel;
        [SerializeField] private Transform content;

        public void ClosePanel ()
        {
            mapEditorSpawnedObjectPanel.SetActive(false);
        
        }

        public void Setup()
        {
            ClosePanel();
            
            mapEditorSpawnedObjectPanel.SetActive(true);
        }
    }
}