using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class ReplayDroneSceneController: ReplayBehaviour
    {
        protected override void OnReplaySpawned(Vector3 position, Quaternion rotation)
        {
            base.OnReplaySpawned(position, rotation);
            if(!IsReplaying)
                return;
            
            var mainScene = SceneManager.GetSceneByName("Main");
            if (mainScene.IsValid())
                SceneManager.MoveGameObjectToScene(gameObject, mainScene);
            else
                Debug.LogWarning("Сцена 'Main' не загружена.");
        }
    }
}