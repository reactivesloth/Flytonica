using System;
using System.Collections;
using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class AimElement: MonoBehaviour
    {
        [SerializeField] private Sprite cameraAim, transportAim;
        [SerializeField] private Image progressImage;
        [SerializeField] private Color targetColor = Color.green;
        [SerializeField] private GameObject progress;
        [SerializeField] private Image aimIcon, actionIcon;

        private Color _startColor;

        private Coroutine _currentAnim;

        public event Action<Color, float> OnFlash;

        public float Progress => progressImage.fillAmount;

        private void Awake()
        {
            _startColor = progressImage.color;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneChanged;
        }

        private void SceneChanged(Scene arg0, LoadSceneMode arg1)
        {
            CancelInvoke();
            StopAllCoroutines();
            SetProgressValue(0);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneChanged;
        }

        public void SetActionIcon(ScenarioType scenarioType)
        {
            actionIcon.sprite = scenarioType switch
            {
                ScenarioType.Transport => transportAim,
                ScenarioType.Searching => cameraAim,
                ScenarioType.SearchingWithIR => cameraAim
            };
        }
        
        public void SetProgressValue(float value)   
        {
            value = Mathf.Clamp(value, 0f, 1f);

            var isValue = value > 0;
            progress.SetActive(isValue);
            actionIcon.gameObject.SetActive(isValue);
            aimIcon.gameObject.SetActive(!isValue);
            
            progressImage.fillAmount = value;
            progressImage.color = Color.Lerp(_startColor, targetColor, value);
        }

        public void Flash(Color color, float animTime, Action callback = null)
        {
            if(_currentAnim != null)
                return;

            OnFlash?.Invoke(color, animTime);
            _currentAnim = StartCoroutine(AnimFlash(color, animTime, callback));
        }

        private IEnumerator AnimFlash(Color color, float animTime, Action callback)
        {
            var sectionTime = animTime / 3;
            var startColor = actionIcon.color;
            
            var currentTime = 0f;
            while (currentTime < sectionTime)
            {
                actionIcon.color = Color.Lerp(_startColor, color, currentTime / sectionTime);
                progressImage.color = Color.Lerp(_startColor, color, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            currentTime = 0f;
            var currentColor = actionIcon.color;
            while (currentTime < sectionTime)
            {
                actionIcon.color = Color.Lerp(currentColor, Color.clear, currentTime / sectionTime);
                progressImage.color = Color.Lerp(currentColor, Color.clear, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            SetProgressValue(0);
            
            currentTime = 0f;
            currentColor = actionIcon.color;
            while (currentTime < sectionTime)
            {
                actionIcon.color = Color.Lerp(currentColor, startColor, currentTime / sectionTime);
                progressImage.color = Color.Lerp(currentColor, startColor, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            _currentAnim = null;
            actionIcon.color = startColor;
            progressImage.color = startColor;
            
            callback?.Invoke();
        }
    }
}