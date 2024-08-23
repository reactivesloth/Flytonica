using UnityEngine;
using UnityEngine.XR;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        
        void Start()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = XRSettings.isDeviceActive && XRSettings.enabled
                ? RenderMode.ScreenSpaceCamera
                : RenderMode.ScreenSpaceOverlay;

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = Camera.main;
            }
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = uiCamera;
            }
        }
    }
}