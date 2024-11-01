using System.Collections;
using Code.Internal.Network.Teacher;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class HostPlayerButton: MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text text;
        [SerializeField] private GameObject helpObject;
        [SerializeField] private float helpSignalTime = 5f;

        private NetworkObject _drone;
        private Coroutine _helpCoroutine;

        private void OnValidate()
        {
            button = GetComponent<Button>();
        }

        public void Init(NetworkObject drone, string title)
        {
            _drone = drone;
            text.text = title;
            button.onClick.AddListener(SelectPlayer);
        }

        public void Help()
        {
            if(_helpCoroutine != null)
                StopCoroutine(_helpCoroutine);
            _helpCoroutine = StartCoroutine(HelpCoroutine());
        }

        private IEnumerator HelpCoroutine()
        {
            helpObject.SetActive(true);
            yield return new WaitForSeconds(helpSignalTime);
            helpObject.SetActive(false);
        }
        
        private void SelectPlayer()
        {
            HostCameraController.Instance.SetTargetDrone(_drone);
        }
    }
}