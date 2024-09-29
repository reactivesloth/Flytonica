using System.Linq;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.MapEditor
{
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

        public void Setup(MapEditorObjectType type)
        {
            ClosePanel();
            
            mapEditorLibraryPanel.SetActive(true);
            
            var spawnableObjects = mapEditorObjectsObjects.Objects.Where(o => o.GetComponent<SpawnableObject>().Type == type).ToList();

            foreach (GameObject o in spawnableObjects)
            {
                var button = Instantiate(buttnPrefab, content).GetComponent<MapEditorUILibraryButton>();
                button.Setup(o);
            }
        }
    }
}