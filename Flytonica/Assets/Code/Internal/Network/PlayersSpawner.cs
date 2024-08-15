using System.Linq;
using Code.Internal.Drone;
using FishNet;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Code.Internal.Network
{
    public class PlayersSpawner : MonoBehaviour
    {
        [Tooltip("Prefab to spawn for the player.")] [SerializeField]
        private DroneController dronesPrefabs;

        public void SpawnDrones()
        {
            var connections = InstanceFinder.ClientManager.Clients.Values.ToArray();
            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            for (var i = 0; i < connections.Length; i++)
            {
                var spawn = spawners[i];
                var drone = InstanceFinder.NetworkManager
                    .GetPooledInstantiated(dronesPrefabs.NetworkObject, spawn.position, spawn.rotation, true);
                InstanceFinder.ServerManager.Spawn(drone, connections[i], SceneManager.GetActiveScene());
            }
        }
    }
}