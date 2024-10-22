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
        [SerializeField] private AvailableMapsSettings _mapsSettings;
        [SerializeField] private GameObject currentSelectedEditorObject;
        [SerializeField] private GameObject currentObjectToSpawn;
        private ConstructorScenarioPage _constructor;
        private string _savedSceneName;
        private bool _isEnabled;
        private Camera _camera;
        private GameObject _gizmo;
        [SerializeField] private Transform spawnedObjectsContainer;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadMapEditor(int sceneIndex, ScenarioType type, ConstructorScenarioPage constructor)
        {
            _constructor = constructor;
            _savedSceneName = _mapsSettings.maps[sceneIndex].loadingSceneName;
            _isEnabled = true;

            SceneManager.LoadScene(_savedSceneName, LoadSceneMode.Additive);
            MapEditorUI.Instance.InitializeMainPanel(type);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == _savedSceneName)
            {
                var objects = FindObjectsOfType<SpawnableObject>();
                foreach (var o in objects)
                {
                    o.transform.SetParent(spawnedObjectsContainer);
                }
            
                MapEditorUI.Instance.UpdateHierarchy (null);
                
                _camera = Camera.main;
                if (_camera != null)
                {
                    var cameraSpawnPoint = GameObject.Find ("MapEditorCameraSpawnPoint").transform;
                    _camera.transform.SetPositionAndRotation(cameraSpawnPoint.position, cameraSpawnPoint.rotation);
                    _camera.gameObject.AddComponent<MapEditorCamera>();
                }
            }
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
            if (_camera == null) return;
            if (_camera.gameObject.GetComponent<MapEditorCamera>() != null)
                Destroy(_camera.GetComponent<MapEditorCamera>());
            _camera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (_gizmo != null)
                DestroyImmediate(_gizmo);
        }

        private void Update()
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (isOverUI) return;

            if (_isEnabled)
            {
                Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (UnityEngine.Input.GetMouseButtonDown(0) && !UnityEngine.Input.GetMouseButton(1))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var tr = hit.transform;
                        if (tr.GetComponentInParent<SpawnableObject>())
                        {
                            SelectObjectToEdit(tr.GetComponentInParent<SpawnableObject>().gameObject);
                        }
                        else if (currentObjectToSpawn != null)
                        {
                            AddObject(hit.point);
                        }
                    }
                }

                if (currentSelectedEditorObject != null)
                {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Delete))
                    {
                        RemoveObject(currentSelectedEditorObject.GetComponent<SpawnableObject>().gameObject);
                    }

                    if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                    {
                        SelectObjectToEdit(null);
                    }
                }
            }
        }

        public void SelectObjectToEdit (GameObject obj) {
            SelectObjectToSpawn(null);
            
            if (_gizmo != null) DestroyImmediate(_gizmo);

            currentSelectedEditorObject = obj;

            if (currentSelectedEditorObject != null) {
                _gizmo = Instantiate(mapEditorGizmo);
                GizmoController.Instance.SelectTarget(currentSelectedEditorObject);
                MapEditorUI.Instance.CloseLibraryPanel ();
            }

            MapEditorUI.Instance.UpdateHierarchy (currentSelectedEditorObject);
        }

        public void SelectObjectToSpawn (GameObject obj)
        {
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

            var newObject = Instantiate(currentObjectToSpawn, position, Quaternion.identity, spawnedObjectsContainer);
            SelectObjectToEdit(newObject);
        }

        public void RemoveObject(GameObject go)
        {
            DestroyImmediate(go);
            SelectObjectToEdit(null);
        }

        public void RemoveCurrentSelectedObject () {
            RemoveObject (currentSelectedEditorObject);
            SelectObjectToEdit(null);
        }
    }
}