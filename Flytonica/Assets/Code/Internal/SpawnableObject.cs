using UnityEngine;

namespace Code.Internal
{
    public enum MapEditorObjectType
    {
        Default = 0,
        
        SpawnPoint = 1,
        
        StartGate = 2,
        FinishGate = 3,
        RacingGate = 4,
        
        TransportObject = 5,
        
        SearchingObject = 6,
        SearchingIrObject = 7
    }
    
    public class SpawnableObject : MonoBehaviour
    {
        public bool selected;
        public MapEditorObjectType Type;
        public Texture2D icon;
        public string displayName;
    }
}
