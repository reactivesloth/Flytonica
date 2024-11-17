using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Code.Internal
{
    public class PlaneUIRaycaster : MonoBehaviour
    {
        [SerializeField] private RectTransform ui;
        [SerializeField] private XRRayInteractor[] xrRayInteractors;
        [SerializeField] private string trackedTag;

        private GameObject _lastHoveredObject;
        private GameObject _pressedObject;

        private void Start()
        {
            Invoke("Initialize", 1);
        }

        private void Initialize()
        {
            ui.gameObject.GetComponent<Canvas>().renderMode = XRSettings.isDeviceActive && XRSettings.enabled ||
                                                              FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive
                                                                  .Include) != null
                ? RenderMode.ScreenSpaceCamera
                : RenderMode.ScreenSpaceOverlay;
        }

        private void Update()
        {
            if (xrRayInteractors.Length == 0)
                xrRayInteractors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);

            foreach (var rayInteractor in xrRayInteractors)
            {
                if (rayInteractor.TryGetCurrent3DRaycastHit(out var hit) &&
                    hit.collider.gameObject.CompareTag(trackedTag))
                {
                    var screenPosition = GetScreenPosition(hit);
                    Simulate(rayInteractor, screenPosition);
                }
                else
                {
                    ClearHoveredObject();
                }
            }
        }

        private void Simulate(XRRayInteractor rayInteractor, Vector2 screenPosition)
        {
            PointerEventData pointerData = new(EventSystem.current)
            {
                position = screenPosition
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            GameObject targetObject = raycastResults.Count > 0 ? raycastResults[0].gameObject : null;

            if (_lastHoveredObject != targetObject)
            {
                if (_lastHoveredObject != null)
                    SimulateExit(_lastHoveredObject, pointerData, ExecuteEvents.pointerExitHandler);
                if (targetObject != null)
                    SimulateEnter(targetObject, pointerData, ExecuteEvents.pointerEnterHandler);
                _lastHoveredObject = targetObject;
            }

            var inputAction = rayInteractor.uiPressInput.inputActionReferencePerformed.action;

            if (inputAction.WasPressedThisFrame())
            {
                if (targetObject != null)
                {
                    _pressedObject = targetObject;
                    pointerData.pressPosition = pointerData.position;
                    pointerData.pointerPress = targetObject;
                    pointerData.rawPointerPress = targetObject;
                    pointerData.delta = Vector2.zero;

                    ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.pointerDownHandler);

                    // Проверяем, поддерживает ли объект перетаскивание
                    if (ExecuteEvents.GetEventHandler<IDragHandler>(_pressedObject) != null)
                    {
                        pointerData.dragging = true;
                        ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.beginDragHandler);
                    }
                }
            }

            if (inputAction.IsPressed() && _pressedObject != null)
            {
                pointerData.delta = Vector2.zero;
                pointerData.position = screenPosition;

                if (pointerData.dragging)
                {
                    ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.dragHandler);
                }
            }

            if (inputAction.WasReleasedThisFrame())
            {
                if (_pressedObject != null)
                {
                    ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.pointerUpHandler);

                    if (pointerData.dragging)
                    {
                        ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.endDragHandler);
                        pointerData.dragging = false;
                    }
                    else
                    {
                        // Выполняем клик, если не было перетаскивания
                        SimulateClick(_pressedObject, pointerData, ExecuteEvents.pointerClickHandler);
                    }

                    _pressedObject = null;
                }
            }
        }

        private static void SimulateClick(GameObject obj, PointerEventData pointerData,
            ExecuteEvents.EventFunction<IPointerClickHandler> action)
        {
            ExecuteEvents.Execute(obj, pointerData, action);
            if (obj.transform.parent != null)
                SimulateClick(obj.transform.parent.gameObject, pointerData, action);
        }

        private static void SimulateEnter(GameObject obj, PointerEventData pointerData,
            ExecuteEvents.EventFunction<IPointerEnterHandler> action)
        {
            ExecuteEvents.Execute(obj, pointerData, action);
            if (obj.transform.parent != null)
                SimulateEnter(obj.transform.parent.gameObject, pointerData, action);
        }

        private static void SimulateExit(GameObject obj, PointerEventData pointerData,
            ExecuteEvents.EventFunction<IPointerExitHandler> action)
        {
            ExecuteEvents.Execute(obj, pointerData, action);
            if (obj.transform.parent != null)
                SimulateExit(obj.transform.parent.gameObject, pointerData, action);
        }

        private Vector2 GetScreenPosition(RaycastHit hit)
        {
            var textureCoordinates = hit.textureCoord;
            return new Vector2(
                textureCoordinates.x * ui.rect.width,
                textureCoordinates.y * ui.rect.height
            );
        }

        private void ClearHoveredObject()
        {
            if (_lastHoveredObject == null)
                return;
            PointerEventData pointerData = new(EventSystem.current);
            SimulateExit(_lastHoveredObject, pointerData, ExecuteEvents.pointerExitHandler);
            _lastHoveredObject = null;
        }
    }
}