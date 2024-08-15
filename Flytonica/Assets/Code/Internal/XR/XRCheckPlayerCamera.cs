using System;
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