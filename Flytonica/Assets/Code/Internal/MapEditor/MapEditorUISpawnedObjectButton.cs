using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.MapEditor
{
    [RequireComponent(typeof(Button))]
    public class MapEditorUISpawnedObjectButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        private Button _button;

        [SerializeField] Button buttonDelete;
        [SerializeField] Button buttonUp;
        [SerializeField] Button buttonDown;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Setup(SpawnableObject obj)
        {   
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => { MapEditor.Instance.SelectObjectToEdit(obj.gameObject);});
            _text.text = obj.displayName;

            buttonDelete.gameObject.SetActive(false);
            buttonDown.gameObject.SetActive(false);
            buttonUp.gameObject.SetActive(false);

            if (obj.selected) {
                
                buttonDelete.gameObject.SetActive(true);
                buttonDelete.onClick.RemoveAllListeners ();
                buttonDelete.onClick.AddListener( () => {MapEditor.Instance.RemoveCurrentSelectedObject ();});

                if (obj.transform.GetSiblingIndex() > 0)
                {
                    buttonUp.gameObject.SetActive(true);
                    buttonUp.onClick.AddListener(() =>
                    {
                        obj.transform.SetSiblingIndex(obj.transform.GetSiblingIndex() - 1);
                        MapEditor.Instance.SelectObjectToEdit (obj.gameObject);
                    });
                }

                if (obj.transform.GetSiblingIndex() < obj.transform.root.childCount - 1)
                {
                    buttonDown.gameObject.SetActive(true);
                    buttonDown.onClick.AddListener(() =>
                    {
                        obj.transform.SetSiblingIndex(obj.transform.GetSiblingIndex() + 1);
                        MapEditor.Instance.SelectObjectToEdit (obj.gameObject);
                    });
                }
            }
        }
    }
}