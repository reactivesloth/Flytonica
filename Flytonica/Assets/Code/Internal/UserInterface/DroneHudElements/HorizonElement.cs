using UnityEngine;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class HorizonElement : MonoBehaviour
    {
        public void SetRotation(float value) => 
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -value);
    }
}