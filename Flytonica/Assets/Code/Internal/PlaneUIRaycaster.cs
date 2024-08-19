using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Code.Internal
{
    public class PlaneUIRaycaster : MonoBehaviour
    {
        [SerializeField] private RectTransform ui;

        private void Update()
        {
            Raycast();
        }

        public void Init(RectTransform uiObject)
        {
            ui = uiObject;
        }

        private void Raycast()
        {
            var ray = new Ray(transform.position, transform.forward);

            if (!Physics.Raycast(ray, out var hit)) return;
            
            var textureCoordinates = hit.textureCoord;

            var screenPosition = new Vector2(
                textureCoordinates.x * ui.rect.width,
                textureCoordinates.y * ui.rect.height
            );

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            var results = new List<RaycastResult>();
            var uiRaycaster = ui.GetComponent<GraphicRaycaster>();

            if (uiRaycaster != null)
            {
                uiRaycaster.Raycast(pointerData, results);

                if (results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        var button = result.gameObject.GetComponent<Button>();
                        if (button == null) continue;
                        EmulateButtonClick(button, pointerData);
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("UI камера не имеет компонента GraphicRaycaster.");
            }
        }

        private void EmulateButtonClick(Component button, BaseEventData pointerData)
        {
            ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
        }
    }
}
