using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements.TableElements
{
    public class TableButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text numberText;

        [SerializeField] private Cell[] cells;
        

        private object _saveData = null;
        public void InitSaveData<T>(T value) => _saveData = value;
        public T GetSaveData<T>() => (T)_saveData;

        public void InitValues(int number, params string[] values)
        {
            numberText.text = number.ToString();
            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];
                cells[index].Init(value);
            }
        }
    }
}