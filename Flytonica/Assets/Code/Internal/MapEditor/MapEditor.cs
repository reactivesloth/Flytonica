using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using TransformGizmos;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private GameObject mapEditorGizmo;
        [SerializeField] private GameObject mapEditorCamera;
        [SerializeField] private AvailableMapsSettings _mapsSettings;
        [SerializeField] private GameObject currentSelectedEditorObject;
        [SerializeField] private GameObject currentObjectToSpawn;
        private ConstructorScenarioPage _constructor;
        private string _savedSceneName;
        private bool _isEnabled;
        private Camera _camera;
        private GameObject _gizmo;
        
        public static MapEditor Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadMapEditor(int sceneIndex, ScenarioType type, ConstructorScenarioPage constructor)
        {
            _constructor = constructor;
            _savedSceneName = _mapsSettings.maps[sceneIndex].loadingSceneName;
            _isEnabled = true;
            _camera = Instantiate(mapEditorCamera).GetComponent<Camera>();
            SceneManager.LoadScene(_savedSceneName, LoadSceneMode.Additive);
            MapEditorUI.Instance.InitializeMainPanel(type);
        }

        public void UnloadMapEditor()
        {
            var objects = FindObjectsOfType<SpawnableObject>();

            foreach (var o in objects)
            {
                Destroy(o.gameObject);
            }
            
            SceneManager.UnloadSceneAsync(_savedSceneName);
            _isEnabled = false;
            Destroy(_camera.gameObject);
        }

        private void Update()
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (isOverUI) return;

            if (_isEnabled)
            {
                Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (UnityEngine.Input.GetMouseButtonDown(0) && currentObjectToSpawn != null && !UnityEngine.Input.GetMouseButton(1))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (currentSelectedEditorObject == null)
                        {
                            var tr = hit.transform.root;
                            if (tr.GetComponent<SpawnableObject>())
                            {
                                currentSelectedEditorObject = tr.gameObject;
                            }
                            else
                            {
                                AddObject(hit.point);
                            }
                        }
                    }
                }

                else if (UnityEngine.Input.GetKeyDown(KeyCode.Delete))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var tr = hit.transform.root;
                        if (tr.GetComponent<SpawnableObject>())
                        {
                            RemoveObject(tr);
                        }
                        else
                        {
                            foreach (Transform t in tr)
                            {
                                if (t.GetComponent<SpawnableObject>())
                                {
                                    RemoveObject(tr);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (currentSelectedEditorObject != null)
                {
                    MapEditorUI.Instance.CloseLibraryPanel();
                    if (_gizmo == null)
                    {
                        if (_gizmo != null) Destroy(_gizmo);
                        _gizmo = Instantiate(mapEditorGizmo);
                        GizmoController.Instance.SelectTarget(currentSelectedEditorObject);
                    }

                    if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                    {
                        currentSelectedEditorObject = null;
                    }
                }

                else
                {
                    if (_gizmo != null) 
                        Destroy(_gizmo);
                }
            }
        }

        public void SelectEditorObject(GameObject obj)
        {
            if (obj == null)
            {
                currentObjectToSpawn = null;
                currentSelectedEditorObject = null;
                return;
            }
            
            currentObjectToSpawn = obj;
        }

        public void AddObject(Vector3 position)
        {
            var type = currentObjectToSpawn.GetComponent<SpawnableObject>().Type;

            var objects = FindObjectsOfType<SpawnableObject>();
            foreach (var o in objects)
            {
                if (type == o.GetComponent<SpawnableObject>().Type)
                    switch (type)
                    {
                        case MapEditorObjectType.SpawnPoint:
                        case MapEditorObjectType.StartGate:
                        case MapEditorObjectType.FinishGate:
                            Destroy(o.gameObject);
                            break;
                        default:
                            break;
                    }
            }

            var newObject = Instantiate(currentObjectToSpawn, position, Quaternion.identity);
            currentSelectedEditorObject = newObject;
        }

        public void RemoveObject(Transform t)
        {
            Destroy(t.gameObject);
        }
    }
}