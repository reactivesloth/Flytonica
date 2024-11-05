using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class ScrollWithButtons : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Button up, down;
        [SerializeField] private float scrollStep = 0.1f;

        private void OnEnable()
        {
            up?.onClick.AddListener(MoveUp);
            down?.onClick.AddListener(MoveDown);
        }

        private void OnDisable()
        {
            up?.onClick.RemoveListener(MoveUp);
            down?.onClick.RemoveListener(MoveDown);
        }

        private void MoveUp()
        {
            scrollView.verticalNormalizedPosition = Mathf.Clamp01(scrollView.verticalNormalizedPosition + scrollStep);
        }

        private void MoveDown()
        {
            scrollView.verticalNormalizedPosition = Mathf.Clamp01(scrollView.verticalNormalizedPosition - scrollStep);
        }
    }
}