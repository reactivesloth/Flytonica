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

            buttonDelete.gameObject.SetActive(obj.selected);
            buttonDown.gameObject.SetActive(obj.selected);
            buttonUp.gameObject.SetActive(obj.selected);

            if (obj.selected) {
                buttonDelete.onClick.RemoveAllListeners ();
                buttonDelete.onClick.AddListener( () => {MapEditor.Instance.RemoveCurrentSelectedObject ();});
            }
        }
    }
}