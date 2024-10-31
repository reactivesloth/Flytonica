using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

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
            if (XRSettings.isDeviceActive && XRSettings.enabled || GameObject.FindObjectsByType<XRDeviceSimulator>(FindObjectsInactive.Include, FindObjectsSortMode.None) != null)
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