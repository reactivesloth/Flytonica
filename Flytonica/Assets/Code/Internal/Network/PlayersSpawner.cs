using System;
using System.Linq;
using Code.Internal.Drone;
using FishNet;
using FishNet.Object;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Code.Internal.Network
{
    public class PlayersSpawner : MonoBehaviour
    {
        public void SpawnDrones(DroneSettings settings)
        {
            var connections = InstanceFinder.ClientManager.Clients.Values.ToArray();
            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            for (var i = 0; i < connections.Length; i++)
            {
                var spawn = spawners[i];
                var drone = InstanceFinder.NetworkManager.GetPooledInstantiated(settings.prefab, spawn.position, spawn.rotation, true);
                InstanceFinder.ServerManager.Spawn(drone, connections[i], SceneManager.GetSceneByName("Main"));
            }
        }
    }
}