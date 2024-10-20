using UnityEngine;

namespace Code.Internal.MapEditor
{
    public class MapEditorUISpawnedObjectsPanel : MonoBehaviour
    {
        [SerializeField] private Transform spawnedObjectContainer;
        [SerializeField] private GameObject mapEditorSpawnedObjectPanel;
        [SerializeReference] private MapEditorUISpawnedObjectButton buttonPrefab;
        [SerializeField] private Transform content;

        public void ClosePanel ()
        {
            MapEditorUISpawnedObjectButton[] buttons = FindObjectsByType<MapEditorUISpawnedObjectButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var b in buttons) {
                DestroyImmediate(b.gameObject);
            }

            mapEditorSpawnedObjectPanel.SetActive(false);
        }

        public void Setup(GameObject selected = null)
        {
            ClosePanel ();

            mapEditorSpawnedObjectPanel.SetActive(true);

            SpawnableObject[] objects = spawnedObjectContainer.GetComponentsInChildren<SpawnableObject>();
            foreach (var o in objects) {
                
                o.selected = selected != null ? o.gameObject == selected : false;

                var newButton = Instantiate (buttonPrefab, content);
                newButton.Setup (o);
            }
        }
    }
}