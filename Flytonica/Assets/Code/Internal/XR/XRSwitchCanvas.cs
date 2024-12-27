using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.XR
{
    public class XRSwitchCanvas : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject panelRTUI;

        private bool _isXr;

        private void Start()
        {
            Invoke("Initialize", 1);
        }

        private void Initialize()
        {
            var canvas = gameObject.GetComponent<Canvas>();

            _isXr = XRSettings.isDeviceActive && XRSettings.enabled ||
                    FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include) != null;

            canvas.renderMode = _isXr ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;

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

        private void OnEnable()
        {
            if(_isXr)
                panelRTUI.SetActive(true);
        }

        private void OnDisable()
        {
            if(_isXr)
                panelRTUI.SetActive(false);
        }
    }
}