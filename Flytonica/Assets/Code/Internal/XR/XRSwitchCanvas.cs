using UnityEngine;
using UnityEngine.XR;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject panelRTUI;

        void Start()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = XRSettings.isDeviceActive && XRSettings.enabled
                ? RenderMode.ScreenSpaceCamera
                : RenderMode.ScreenSpaceOverlay;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.worldCamera = Camera.main;
                if (panelRTUI != null) panelRTUI.SetActive(false);
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = uiCamera;
                if (panelRTUI != null) panelRTUI.SetActive(true);
            }
        }
    }
}