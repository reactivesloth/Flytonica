using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace Code.Internal.Avatars.Settings
{
    [CreateAssetMenu(fileName = "Avatars List", menuName = "Flytoncia/Avatars/Avatars List", order = 1)]
    public class AvatarsList : ScriptableObject
    {
        public List<Avatar> avatarsIksList;
    }

    [System.Serializable]
    public class Avatar
    {
        public VRIK avatarIk;
        public Sprite icon;
    }
}