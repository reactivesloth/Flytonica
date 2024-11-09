using Code.Internal.Drone;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ClientUIControllerPage : MonoBehaviour
    {
        [SerializeField] private bl_MiniMap map;
        
        [SerializeField] private Button mapButton;
        [SerializeField] private GameObject mapUi;

        private bool _isFpv = false;
        
        private void Update()
        {
            if(!DroneInput.Instance)
                return;
            
            if(_isFpv == DroneInput.Instance.DroneCam)
                return;

            _isFpv = DroneInput.Instance.DroneCam;
            OnViewChanged(_isFpv);
        }

        private void Start()
        {
            mapButton.onClick.AddListener(OnOpenMap);
        }
        
        private void OnOpenMap()
        {
            map.Target = DroneController.Instance.transform;
            mapUi?.SetActive(true);
        } 

        private void OnViewChanged(bool isFpv)
        {
            print($"OnViewChanged {isFpv}");
            mapUi.SetActive(false);
            mapButton.gameObject.SetActive(!isFpv);
        }
    }
}