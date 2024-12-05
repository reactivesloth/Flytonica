using TMPro;
using UnityEngine;

namespace Code.Internal.EditorHelpers
{
    public class FontChanger : MonoBehaviour
    {
        [SerializeField] private Transform teacherUI, studentUI;
        [SerializeField] private TMP_FontAsset teacherFont, studentFont;
        [SerializeField] private TMP_FontAsset defaultFont;

        public void ApplyFontChanges()
        {
    #if UNITY_EDITOR
            // Обновляем шрифты в teacherUI
            if (teacherUI != null && teacherFont != null)
            {
                var teacherTexts = teacherUI.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmpText in teacherTexts)
                {
                    tmpText.font = teacherFont;
                    UnityEditor.EditorUtility.SetDirty(tmpText);
                }
            }

            // Обновляем шрифты в studentUI
            if (studentUI != null && studentFont != null)
            {
                var studentTexts = studentUI.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmpText in studentTexts)
                {
                    tmpText.font = studentFont;
                    UnityEditor.EditorUtility.SetDirty(tmpText);
                }
            }

            // Назначаем шрифт по умолчанию всем остальным TMP_Text
            if (defaultFont != null)
            {
                var allTexts = FindObjectsOfType<TMP_Text>(true);
                foreach (var tmpText in allTexts)
                {
                    bool isChildOfTeacher = teacherUI != null && tmpText.transform.IsChildOf(teacherUI);
                    bool isChildOfStudent = studentUI != null && tmpText.transform.IsChildOf(studentUI);

                    if (!isChildOfTeacher && !isChildOfStudent)
                    {
                        tmpText.font = defaultFont;
                        UnityEditor.EditorUtility.SetDirty(tmpText);
                    }
                }
            }

            // Помечаем сцену как измененную
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    #endif
        }
    }
}
