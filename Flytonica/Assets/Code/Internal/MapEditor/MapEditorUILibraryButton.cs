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

        private void Update()
        {
            if (_prefab == null) return;
            #if UNITY_EDITOR
            if (_texture2D == Texture2D.linearGrayTexture)
            {
                _image.texture = AssetPreview.GetAssetPreview(_prefab);
            }
            #endif
        }

        public void Setup(GameObject prefab)
        {
            _texture2D = Texture2D.linearGrayTexture;
            _prefab = prefab;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => { MapEditor.Instance.SelectEditorObject(prefab);});
        }
    }
}