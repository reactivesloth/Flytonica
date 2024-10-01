using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public class Wind : MonoBehaviour
    {
        public static Wind Instance { get; private set; }

        [SerializeField] private List<WindLayerHeights> layersHeights; // for settings height of layers

        private readonly List<LayerSettings> _currentLayerSettings = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Init(List<WindLayerSettings> windSettings)
        {
            _currentLayerSettings.Clear();

            for (var i = 0; i < windSettings.Count; i++)
            {
                var windLayer = windSettings[i];

                var (minForce, maxForce) = windLayer.GetWindForce();

                var layerSettings =
                    new LayerSettings(layersHeights[i], minForce, maxForce, windLayer.GetWindDirection());
                _currentLayerSettings.Add(layerSettings);
            }
        }

        private void FixedUpdate()
        {
            if (!DroneController.Instance || !GameSceneManager.Instance.IsPlaying)
                return;

            var droneHeight = DroneController.Instance.transform.position.y;

            var layer = _currentLayerSettings.FirstOrDefault(l =>
                droneHeight >= l.heights.startHeight && droneHeight <= l.heights.finishHeight);

            if (layer.Equals(default(LayerSettings)))
                return;

            var direction = layer.direction;
            var force = layer.AverageForce;

            DroneController.Instance.WindEffect(direction, force);
        }
    }

    [Serializable]
    public struct WindLayerHeights
    {
        public float startHeight, finishHeight;
    }

    [Serializable]
    public struct LayerSettings
    {
        public WindLayerHeights heights;
        public float minForce, maxForce;
        public Vector3 direction;

        public LayerSettings(WindLayerHeights heights, float minForce, float maxForce, Vector3 direction)
        {
            this.heights = heights;
            this.minForce = minForce;
            this.maxForce = maxForce;
            this.direction = direction;
        }

        public float AverageForce => (minForce + maxForce) / 2;
    }
}