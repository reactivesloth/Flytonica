using System.Collections.Generic;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Drones List", menuName = "Flytoncia/Scenes/Available Drones", order = 1)]
    public class AvailableDronesSettings : ScriptableObject
    {
        public List<DroneSettings> drones;
    }
}