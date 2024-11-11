using System;
using UnityEngine;

namespace Code.Internal.UserInterface
{
    public class MoverWithPlayer : MonoBehaviour
    {
        private void Update()
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if(!playerObject) return;
            transform.position = playerObject.transform.position;
            
        }
    }
}
