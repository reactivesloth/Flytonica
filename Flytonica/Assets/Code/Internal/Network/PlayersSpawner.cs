using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Code.Internal.Network
{
    public class PlayersSpawner : MonoBehaviour
    {
        private readonly List<NetworkObject> _drones = new();

        public NetworkObject Spawn(NetworkConnection connection, DroneSettings settings, bool isTeacher = false)
        {
            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            var spawn = spawners[Random.Range(0, spawners.Length)];
            var drone = InstanceFinder.NetworkManager.GetPooledInstantiated(settings.prefab, spawn.position,
                spawn.rotation, true);
            InstanceFinder.ServerManager.Spawn(drone, connection, SceneManager.GetSceneByName("Main"));
            _drones.Add(drone);
            return drone;
        }

        public void Despawn(NetworkConnection connection)
        {
            var drone = _drones.FirstOrDefault(d => d.GetComponent<NetworkObject>().Owner == connection);
            if (drone == null)
                return;
            _drones.Remove(drone);
            InstanceFinder.ServerManager.Despawn(drone, DespawnType.Destroy);
        }

        public void DespawnAll()
        {
            foreach (var networkObject in _drones.ToList())
            {
                _drones.Remove(networkObject);
                InstanceFinder.ServerManager.Despawn(networkObject, DespawnType.Destroy);
            }
        }
    }
}