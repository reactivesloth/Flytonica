using System.Collections.Generic;
using Code.Internal.API.Wrappers;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Code.Internal.MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private AvailableMapsSettings _mapsSettings;
        [SerializeField] private GameObject currentSelectedEditorObject;
        private ConstructorScenarioPage _constructor;
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
        
        public void LoadMapEditor(int sceneIndex, ScenarioType type, ConstructorScenarioPage constructor)
        {
            _constructor = constructor;
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
                        AddObject(hit.point);
                    }
                }

                else if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        var tr = hit.transform.root;
                        /*if (tr.GetComponent<MapEditorAddedObject>())
                        {
                            RemoveObject(tr);
                        }
                        else
                        {
                            foreach (Transform t in tr)
                            {
                                if (t.GetComponent<MapEditorAddedObject>())
                                {
                                    RemoveObject(tr);
                                    break;
                                }
                            }
                        }*/
                    }
                }
            }
        }

        public void SelectEditorObject(GameObject obj)
        {
            currentSelectedEditorObject = obj;
        }

        public void AddObject(Vector3 position)
        {
            var newObject = Instantiate(currentSelectedEditorObject, position, Quaternion.identity);
        }

        public void RemoveObject(Transform t)
        {
            Destroy(t.gameObject);
        }
    }
}