using System;
using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class FlytonicaReplayBehaviour : ReplayBehaviour
    {
        protected void Start()
        {
            //SceneManager.sceneUnloaded += OnSceneUnload;
        }

        protected override void OnDestroy()
        {
            Debug.Log($"Объект {name} разрушен");
            if (!IsRecording)
                return;
            ReplayManager.RemoveReplayObjectFromRecordScenes(ReplayObject);
            Debug.Log($"Объект {name} удалён из записи");
            //SceneManager.sceneUnloaded -= OnSceneUnload;
            base.OnDestroy();
        }

        protected override void OnReplaySpawned(Vector3 position, Quaternion rotation)
        {
            base.OnReplaySpawned(position, rotation);
            if (!IsReplaying)
                return;

            var mainScene = SceneManager.GetSceneByName("Main");
            if (mainScene.IsValid())
                SceneManager.MoveGameObjectToScene(gameObject, mainScene);
            else
                Debug.LogWarning("Сцена 'Main' не загружена.");
        }

        private void OnSceneUnload(Scene unloadedScene)
        {
            if (gameObject.scene != unloadedScene)
                return;
        }
    }
}