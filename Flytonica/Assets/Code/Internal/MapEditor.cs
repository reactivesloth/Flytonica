using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private AvailableMapsSettings _mapsSettings;
        [SerializeField] private GameObject cube;
        private string _savedSceneName;
        private Camera _camera;
        private bool _isEnabled;
        
        public static MapEditor Instance { get; private set; }

        private void Awake()
        {
            _camera = GetComponentInChildren<Camera>(true);
            
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadMapEditor(int sceneIndex)
        {
            _savedSceneName = _mapsSettings.maps[sceneIndex].loadingSceneName;
            _isEnabled = true;
            _camera.gameObject.SetActive(true);
            SceneManager.LoadScene(_savedSceneName, LoadSceneMode.Additive);
        }

        public void UnloadMapEditor()
        {
            SceneManager.UnloadSceneAsync(_savedSceneName);
            _isEnabled = false;
            _camera.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isEnabled)
            {
                Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var newObject = Instantiate(cube, hit.point, Quaternion.identity);
                        newObject.AddComponent<MapEditorAddedObject>();
                    }
                }

                else if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.collider.gameObject.GetComponent<MapEditorAddedObject>())
                        {
                            Destroy(hit.collider.gameObject);
                        }
                    }
                }
            }
        }
    }

    public class MapEditorAddedObject : MonoBehaviour
    {
    }
}