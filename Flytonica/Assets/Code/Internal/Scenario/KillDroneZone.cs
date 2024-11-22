using System;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

public class KillDroneZone : MonoBehaviour
{
    public string message;
    public AudioClip messageKillClip;
 
    private void OnTriggerEnter(Collider other)
    {
        DroneController drone = null;
        if (other.GetComponentInParent<DroneController>() == DroneController.Instance)
        {
            drone = DroneController.Instance;
        }
        if (drone== null) return;
        
        DroneHUD.Instance.SetMessage(MessageType.Error, message, messageKillClip.length, messageKillClip);
        drone.BroadcastMessage("ApplyDamage", 1000);
    }
}
