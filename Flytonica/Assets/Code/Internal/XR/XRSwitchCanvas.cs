using UnityEngine;
using UnityEngine.XR;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        void Start()
        {
            gameObject.GetComponent<Canvas>().renderMode = XRSettings.isDeviceActive && XRSettings.enabled
                ? RenderMode.ScreenSpaceCamera
                : RenderMode.ScreenSpaceOverlay;
        }
    }
}