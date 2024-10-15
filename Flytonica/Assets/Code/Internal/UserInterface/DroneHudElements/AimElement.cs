using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class AimElement: MonoBehaviour
    {
        [SerializeField] private Image progressImage;
        [SerializeField] private Color targetColor = Color.green;
        [SerializeField] private GameObject progress;
        [SerializeField] private Image aimIcon, cameraIcon;

        private Color _startColor;

        private Coroutine _currentAnim;

        public event Action<Color, float> OnFlash;

        public float Progress => progressImage.fillAmount;

        private void Awake()
        {
            _startColor = progressImage.color;
        }

        public void SetProgressValue(float value)   
        {
            value = Mathf.Clamp(value, 0f, 1f);

            var isValue = value > 0;
            progress.SetActive(isValue);
            cameraIcon.gameObject.SetActive(isValue);
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
            var startColor = cameraIcon.color;
            
            var currentTime = 0f;
            while (currentTime < sectionTime)
            {
                cameraIcon.color = Color.Lerp(_startColor, color, currentTime / sectionTime);
                progressImage.color = Color.Lerp(_startColor, color, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            currentTime = 0f;
            var currentColor = cameraIcon.color;
            while (currentTime < sectionTime)
            {
                cameraIcon.color = Color.Lerp(currentColor, Color.clear, currentTime / sectionTime);
                progressImage.color = Color.Lerp(currentColor, Color.clear, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            SetProgressValue(0);
            
            currentTime = 0f;
            currentColor = cameraIcon.color;
            while (currentTime < sectionTime)
            {
                cameraIcon.color = Color.Lerp(currentColor, startColor, currentTime / sectionTime);
                progressImage.color = Color.Lerp(currentColor, startColor, currentTime / sectionTime);
                yield return null;
                currentTime += Time.deltaTime;
            }
            
            _currentAnim = null;
            cameraIcon.color = startColor;
            progressImage.color = startColor;
            
            callback?.Invoke();
        }
    }
}