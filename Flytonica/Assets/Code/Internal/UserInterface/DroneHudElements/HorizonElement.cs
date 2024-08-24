using UnityEngine;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class HorizonElement : MonoBehaviour
    {
        public void SetRoll(float value) => 
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -value);

        public void SetPitch(float value)
        {
            // Gibmal Lock Fix
            if (value > 180)
                value -= 360;
            if (value < -180)
                value += 360;
            
            transform.localPosition = new Vector3(0, value, 0);
        }
    }
}