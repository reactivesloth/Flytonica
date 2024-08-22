using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using FishNet;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Code.Internal.Network
{
    public class PlayersSpawner : MonoBehaviour
    {
        private readonly List<NetworkObject> _drones = new();

        private bool _isSpawned = false;

        public void SpawnDrones(DroneSettings settings)
        {
            if(_isSpawned)
                return;
            
            var connections = InstanceFinder.ClientManager.Clients.Values.ToArray();
            print(connections.Length);
            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            for (var i = 0; i < connections.Length; i++)
            {
                var spawn = spawners[Random.Range(0, spawners.Length)];
                var drone = InstanceFinder.NetworkManager.GetPooledInstantiated(settings.prefab, spawn.position, spawn.rotation, true);
                InstanceFinder.ServerManager.Spawn(drone, connections[i], SceneManager.GetSceneByName("Main"));
                _drones.Add(drone);
            }

            _isSpawned = true;
        }

        public void Despawn()
        {
            _isSpawned = false;
            foreach (var drone in _drones)
            {
                if (drone != null && drone.gameObject.activeSelf)
                {
                    InstanceFinder.ServerManager.Despawn(drone);
                }
            }
            _drones.Clear(); 
        }
    }
}