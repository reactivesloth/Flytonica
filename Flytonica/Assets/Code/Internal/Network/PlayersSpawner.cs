using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.Network.Teacher;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UltimateReplay;
using UnityEngine;
using Random = UnityEngine.Random;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Code.Internal.Network
{
    public class PlayersSpawner : MonoBehaviour
    {
        private readonly List<NetworkObject> _drones = new();

        public NetworkObject Spawn(NetworkConnection connection, DroneSettings settings)
        {
            if (!InstanceFinder.ServerManager.Clients.ContainsValue(connection))
                return null;

            var spawners = GameObject.FindGameObjectsWithTag("Respawn")
                .Select(o => o.transform).ToArray();

            var spawn = spawners[Random.Range(0, spawners.Length)];

            var spawnPosition = spawn.position + Vector3.up * 0.5f;

            var avatarPosition = spawnPosition + Vector3.back + Vector3.up;
            if (Physics.Raycast(avatarPosition, Vector3.down, out RaycastHit hit, 10))
            {
                avatarPosition.y = hit.point.y;
            }

            ConnectionController.Instance.MovePlayerSignal(connection, avatarPosition, spawn.transform.rotation);

            print(Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out hit, 10));
            
            var finishedSpawnPoint = Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out hit, 10)
                ? hit.point
                : spawn.position;

            var drone = InstanceFinder.NetworkManager.GetPooledInstantiated(settings.prefab, finishedSpawnPoint + Vector3.up * 0.1f,
                spawn.rotation, true);
            InstanceFinder.ServerManager.Spawn(drone, connection, SceneManager.GetSceneByName("Main"));
            _drones.Add(drone);

            // PlayerManager.Instance.AddPlayer(connection, drone);

            if (drone.TryGetComponent(out ReplayObject replayObject))
                ReplayManager.AddReplayObjectToRecordScenes(replayObject);

            return drone;
        }

        public void Despawn(NetworkConnection connection)
        {
            var drone = _drones.FirstOrDefault(d =>
            {
                if (d)
                    return d.Owner == connection;
                return false;
            });

            if (drone == null)
                return;

            if (drone.TryGetComponent(out ReplayObject replayObject))
                ReplayManager.RemoveReplayObjectFromRecordScenes(replayObject);
            _drones.Remove(drone);
            // PlayerManager.Instance.RemovePlayer(connection);
            InstanceFinder.ServerManager.Despawn(drone, DespawnType.Destroy);
            Destroy(drone.gameObject);
        }
    }
}