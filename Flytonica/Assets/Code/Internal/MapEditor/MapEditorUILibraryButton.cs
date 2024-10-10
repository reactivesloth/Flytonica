using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.MapEditor
{
    [RequireComponent(typeof(Button))]
    public class MapEditorUILibraryButton : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        private Button _button;

        private Texture2D _texture2D;
        private GameObject _prefab;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Setup(GameObject prefab)
        {
            _texture2D = Texture2D.linearGrayTexture;
            _prefab = prefab;

            if (prefab.GetComponent<SpawnableObject>() != null)
            {
                _texture2D = prefab.GetComponent<SpawnableObject>().icon;
            }
            
            _image.texture = _texture2D;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => { MapEditor.Instance.SelectEditorObject(prefab);});
        }
    }
}