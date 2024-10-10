using UnityEngine;
using UnityEngine.XR;

namespace Code.Internal.XR
{
    public class XRCheckPlayerCamera : MonoBehaviour
    {
        [SerializeField] private GameObject desktopPlayer;
        [SerializeField] private GameObject XRPlayer;

        private void Start()
        {
            Invoke("Initialize", 1);
        }
        
        private void Initialize ()
        {
            if (XRSettings.isDeviceActive && XRSettings.enabled)
            {
                desktopPlayer.SetActive(false);
                XRPlayer.SetActive(true);
            }
            else
            {
                desktopPlayer.SetActive(true);
                XRPlayer.SetActive(false);
            }
        }
    }
}