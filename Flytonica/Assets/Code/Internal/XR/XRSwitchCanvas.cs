using UnityEngine;
using UnityEngine.XR;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        void Start()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = XRSettings.isDeviceActive && XRSettings.enabled
                ? RenderMode.WorldSpace
                : RenderMode.ScreenSpaceOverlay;

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
}