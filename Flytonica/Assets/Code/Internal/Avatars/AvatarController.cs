using System;
using Code.Internal.Avatars.Settings;
using FishNet.Connection;
using FishNet.Object;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.Avatars
{
    [Serializable]
    public class AvatarIKTargetRig
    {
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;
    }
    
    public class AvatarController : NetworkBehaviour
    {
        private static AvatarController _instance;
        [SerializeField] private bool useHands = false;
        [SerializeField] private Transform player;
        [SerializeField] private AvatarsList avatarsList;

        [SerializeField] private AvatarIKTargetRig desktopRig, vrRig;
        
        public static AvatarController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AvatarController>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<AvatarController>();
                        singletonObject.name = typeof(AvatarController).ToString() + " (Singleton)";
                    }
                    return _instance;
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }
        
        public void SpawnAvatar(NetworkConnection avatarOwner, int avatarId)
        {
            var avatar = Instantiate(avatarsList.avatarsIksList[avatarId].avatarIk, player);
            ServerManager.Spawn(avatar.GetComponent<NetworkObject>(), avatarOwner);
            InitUserAvatar(avatarOwner, avatar.GetComponent<NetworkObject>());
            InitNotOwner(avatar.GetComponent<NetworkObject>());
        }

        [TargetRpc]
        public void InitUserAvatar(NetworkConnection avatarOwner, NetworkObject avatar)
        {
            var vrIk = avatar.GetComponent<VRIK>();
            bool isVR = XRSettings.isDeviceActive && XRSettings.enabled ||
                        FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include) != null;

            var rig = isVR ? vrRig : desktopRig;
            vrIk.solver.spine.headTarget = rig.head;
            if (useHands)
            {
                vrIk.solver.leftArm.target = rig.leftHand;
                vrIk.solver.rightArm.target = rig.rightHand;
            }
        }

        [ObserversRpc]
        public void InitNotOwner(NetworkObject avatar)
        {
            if (!avatar.IsOwner)
                avatar.GetComponent<VRIK>().enabled = false;
        }
    }
}