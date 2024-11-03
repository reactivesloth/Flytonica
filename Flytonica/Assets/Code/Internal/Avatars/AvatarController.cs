using Code.Internal.Avatars.Settings;
using FishNet.Connection;
using FishNet.Object;
using RootMotion.FinalIK;
using UnityEngine;
using Avatar = UnityEngine.Avatar;

namespace Code.Internal.Avatars
{
    public class AvatarController : NetworkBehaviour
    {
        private static AvatarController _instance;
        
        [SerializeField] private AvatarsList avatarsList;
        [SerializeField] private Transform vrHead, vrLeftHand, vrRightHand;

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
            var avatar = Instantiate(avatarsList.avatarsIksList[avatarId].avatarIk);
            ServerManager.Spawn(avatar.GetComponent<NetworkObject>(), avatarOwner);
            InitUserAvatar(avatarOwner, avatar.GetComponent<NetworkObject>());
            InitNotOwner(avatar.GetComponent<NetworkObject>());
            
        }

        [TargetRpc]
        public void InitUserAvatar(NetworkConnection avatarOwner, NetworkObject avatar)
        {
            var vrIk = avatar.GetComponent<VRIK>();
            vrIk.solver.spine.headTarget = vrHead;
            vrIk.solver.leftArm.target = vrLeftHand;
            vrIk.solver.rightArm.target = vrRightHand;
        }

        [ObserversRpc]
        public void InitNotOwner(NetworkObject avatar)
        {
            if (!avatar.IsOwner)
                avatar.GetComponent<VRIK>().enabled = false;
        }
    }
}