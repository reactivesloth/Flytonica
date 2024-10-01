using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
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

                var (minSpeed, maxSpeed) = windLayer.GetWindSpeeds();
                var (direction, directionName) = windLayer.GetWindDirection();

                var layerSettings =
                    new LayerSettings(layersHeights[i], minSpeed, maxSpeed, direction, directionName);
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

            var direction = layer.direction;
            var speed = layer.AverageForce;
            
            DroneHUD.Instance.SetWind(speed, layer.directionName);

            DroneController.Instance.WindEffect(direction, speed);
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
        public float minSpeed, maxSpeed;
        public Vector3 direction;
        public string directionName;

        public LayerSettings(WindLayerHeights heights, float minSpeed, float maxSpeed, Vector3 direction, string directionName)
        {
            this.heights = heights;
            this.minSpeed = minSpeed;
            this.maxSpeed = maxSpeed;
            this.direction = direction;
            this.directionName = directionName;
        }

        public float AverageForce => (minSpeed + maxSpeed) / 2;
    }
}