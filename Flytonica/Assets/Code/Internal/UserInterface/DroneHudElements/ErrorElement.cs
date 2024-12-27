using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class ErrorElement: MonoBehaviour
    {
        [SerializeField] private TMP_Text massageText;
        [SerializeField] private GameObject[] errorIndicators;

        public void SetError(string massage = "")
        {
            massageText.text = massage;
            foreach (var errorIndicator in errorIndicators)
                errorIndicator.SetActive(true);
        }

        public void UnsetError()
        {
            foreach (var errorIndicator in errorIndicators)
                errorIndicator.SetActive(false);
        }
    }
}