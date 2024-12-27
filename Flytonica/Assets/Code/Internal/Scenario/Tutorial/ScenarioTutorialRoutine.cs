using System;
using System.Collections;
using System.Collections.Generic;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial
{
    [Serializable]
    public class ScenarioTutorialReplique
    {
        [TextArea (3, 5)] public string text;
        public AudioClip clip;
        public Action onComplete;
    }
    
    [RequireComponent(typeof(AudioSource))]
    public class ScenarioTutorialRoutine : MonoBehaviour
    {
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public virtual void Initialize()
        {
            StopAllCoroutines();
            StartCoroutine(RunRoutine());
        }

        protected virtual IEnumerator RunRoutine()
        {
            yield return null;
        }
        
        protected virtual void Leave()
        {
            GetComponentInParent<ScenarioTutorial>().LeaveTutorial();
        }
        
        protected virtual void Finish()
        {
            DroneHUD.Instance.SetTask("Все задачи выполнены!");
            DroneHUD.Instance.ClearMessage();
            DroneHUD.Instance.SetTime(null);
            GetComponentInParent<ScenarioTutorial>().CompleteTutorial();
        }

        protected virtual void Update()
        {
            
        }
        
        protected IEnumerator PlayReplique(ScenarioTutorialReplique replique)
        {
            DroneHUD.Instance?.SetTask(replique.text);
            var lenght = replique.clip ? replique.clip.length : 2;
            DroneHUD.Instance?.SetMessage(MessageType.Normal, replique.text, lenght, replique.clip, true);
            if (replique.clip != null)
            {
                yield return new WaitForSeconds(replique.clip.length + 1f);
            }
            else
            {
                yield return new WaitForSeconds(2);
            }
        }
    }
}