using UltimateReplay;
using UnityEngine;

namespace Code.Internal.Replays
{
    public class PlayersReplayBehaviour: ReplayBehaviour
    {
        protected void Update()
        {  
            if(!IsRecording) 
                return;
            
            foreach (var droneReplayBehaviour in FindObjectsByType<DroneReplayBehaviour>(FindObjectsSortMode.None))
            {
                if(!droneReplayBehaviour) continue;
                ReplayManager.AddReplayObjectToRecordScenes(droneReplayBehaviour.ReplayObject);
            }
        }
    }
}