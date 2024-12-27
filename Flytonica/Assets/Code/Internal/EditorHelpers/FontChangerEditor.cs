#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Code.Internal.EditorHelpers
{
    [CustomEditor(typeof(FontChanger))]
    public class FontChangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FontChanger fontChanger = (FontChanger)target;

            if (GUILayout.Button("Применить изменения шрифтов"))
            {
                fontChanger.ApplyFontChanges();
            }
        }
    }
}
#endif