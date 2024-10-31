using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject panelRTUI;

        private void Start()
        {
            Invoke("Initialize", 1);
        }
        
        private void Initialize ()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = XRSettings.isDeviceActive && XRSettings.enabled || GameObject.FindObjectsByType<XRDeviceSimulator>(FindObjectsInactive.Include, FindObjectsSortMode.None) != null
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