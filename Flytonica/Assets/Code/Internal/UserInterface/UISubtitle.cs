using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface 
{
    public class UISubtitle : MonoBehaviour
    {
        private Text _text;
        [SerializeField] private float typingSpeed = 10;

        public static UISubtitle Instance;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            _text = gameObject.GetComponent<Text>();
        }

        public void ClearText()
        { 
            _text.text = string.Empty;
        }

        public void SetTextInstant(string text, float timeToClear = 0)
        {
            CancelInvoke();
            _text.text = text;
            if (timeToClear > 0)
            {
                Invoke("ClearText", timeToClear);
            }
        }
        
        public void SetText(string text, float timeToClear = 0)
        {
            CancelInvoke();
            _text.text = string.Empty;
            StopCoroutine("PrintText");
            StartCoroutine(PrintText(text));
        }

        private IEnumerator PrintText(string text, float timeToClear = 0)
        {
            var firstLetterUpText = text[0].ToString().ToUpper() + text.Substring(1);
            var textToPrint = firstLetterUpText.ToCharArray();

            foreach (var t in textToPrint)
            {
                _text.text += t;
                yield return new WaitForSecondsRealtime(0.1f / typingSpeed);
            }

            if (timeToClear > 0)
            {
                yield return new WaitForSeconds(timeToClear);
                ClearText();
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}