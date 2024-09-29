using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.Network.Teacher;
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
            if (!InstanceFinder.ServerManager.Clients.ContainsValue(connection))
                return null;
            
            print($"Spawn {settings.name}");
            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            var spawn = spawners[Random.Range(0, spawners.Length)];
            var drone = InstanceFinder.NetworkManager.GetPooledInstantiated(settings.prefab, spawn.position,
                spawn.rotation, true);
            InstanceFinder.ServerManager.Spawn(drone, connection, SceneManager.GetSceneByName("Main"));
            _drones.Add(drone);
            
            PlayerManager.Instance.AddPlayer(connection, drone);
            
            return drone;
        }

        public void Despawn(NetworkConnection connection)
        {
            print(connection);
            var drone = _drones.FirstOrDefault(d =>
            {
                if(d)
                    return d.Owner == connection;
                return false;
            });
            
            if (drone == null)
                return;
            
            _drones.Remove(drone);
            PlayerManager.Instance.RemovePlayer(connection);
            InstanceFinder.ServerManager.Despawn(drone, DespawnType.Destroy);
            Destroy(drone.gameObject);
        }

        public void DespawnAll(NetworkConnection connection)
        {
            foreach (var networkObject in _drones.ToList())
            {
                _drones.Remove(networkObject);
                PlayerManager.Instance.RemovePlayer(connection);
                InstanceFinder.ServerManager.Despawn(networkObject, DespawnType.Destroy);
            }
        }
    }
}