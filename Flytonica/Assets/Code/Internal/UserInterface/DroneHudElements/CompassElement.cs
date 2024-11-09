using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class CompassElement : MonoBehaviour
    {
        private Transform Target;
        public RectTransform CompassRoot;
        
        public RectTransform North;
        public RectTransform South;
        public RectTransform East;
        public RectTransform West;
        
        [HideInInspector] public int Grade;
        
        private int Rotation;
        
        void Update()
        {
            if (Target == null)
            {
                Target = DroneController.Instance?.transform;
            }

            if (Target != null)
            {
                Rotation = (int)Mathf.Abs(Target.eulerAngles.y) % 360;
            }
            else
            {
                Rotation = (int)Mathf.Abs(m_Transform.eulerAngles.y) % 360;
            }

            Grade = Rotation;
            if (Grade > 180)
            {
                Grade = Grade - 360;
            }

            float centerX = CompassRoot.sizeDelta.x * 0.5f;
            
            if (North != null)
                North.anchoredPosition = new Vector2((centerX - (Grade * 2) - centerX), 0);
            
            if (South != null)
                South.anchoredPosition = new Vector2((centerX - Rotation * 2 + 360) - centerX, 0);
            
            if (East != null)
                East.anchoredPosition = new Vector2((centerX - Grade * 2 + 180) - centerX, 0);
            
            if (West != null)
                West.anchoredPosition = new Vector2((centerX - Rotation * 2 + 540) - centerX, 0);
        }

        public float Angle360(Vector2 p1, Vector2 p2, Vector2 o = default(Vector2))
        {
            Vector2 v1, v2;
            if (o == default(Vector2))
            {
                v1 = p1.normalized;
                v2 = p2.normalized;
            }
            else
            {
                v1 = (p1 - o).normalized;
                v2 = (p2 - o).normalized;
            }
            float angle = Vector2.Angle(v1, v2);
            return Mathf.Sign(Vector3.Cross(v1, v2).z) < 0 ? (360 - angle) % 360 : angle;
        }

        private Transform t;
        private Transform m_Transform
        {
            get
            {
                if (t == null)
                {
                    t = this.GetComponent<Transform>();
                }
                return t;
            }
        }
    }
}
