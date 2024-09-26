using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Code.Internal.MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private AvailableMapsSettings _mapsSettings;
        [SerializeField] private GameObject currentSelectedEditorObject;
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
        
        public void LoadMapEditor(int sceneIndex, ScenarioType type)
        {
            _savedSceneName = _mapsSettings.maps[sceneIndex].loadingSceneName;
            _isEnabled = true;
            _camera.gameObject.SetActive(true);
            SceneManager.LoadScene(_savedSceneName, LoadSceneMode.Additive);
            MapEditorUI.Instance.InitializeMainPanel(type);
        }

        public void UnloadMapEditor()
        {
            SceneManager.UnloadSceneAsync(_savedSceneName);
            _isEnabled = false;
            _camera.gameObject.SetActive(false);
        }

        private void Update()
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (isOverUI) return;

            if (_isEnabled)
            {
                Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (UnityEngine.Input.GetMouseButtonDown(0) && currentSelectedEditorObject != null)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var newObject = Instantiate(currentSelectedEditorObject, hit.point, Quaternion.identity);
                        newObject.AddComponent<MapEditorAddedObject>();
                    }
                }

                else if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var tr = hit.transform.root;
                        if (tr.GetComponent<MapEditorAddedObject>())
                        {
                            Destroy(tr.gameObject);
                        }
                        else
                        {
                            foreach (Transform t in tr)
                            {
                                if (t.GetComponent<MapEditorAddedObject>())
                                {
                                    Destroy(t.gameObject);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SelectEditorObject(GameObject obj)
        {
            currentSelectedEditorObject = obj;
        }
    }

    public class MapEditorAddedObject : MonoBehaviour
    {
    }
}