using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements.TableElements
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private TMP_Text mainText;

        public void Init(string text)
        {
            mainText.text = text;
        }
    }
}