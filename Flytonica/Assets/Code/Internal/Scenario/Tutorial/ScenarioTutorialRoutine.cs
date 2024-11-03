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
        public string text;
        public AudioClip clip;
        public Action onComplete;
    }
    
    [RequireComponent(typeof(AudioSource))]
    public class ScenarioTutorialRoutine : MonoBehaviour
    {
        protected AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

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
            GetComponentInParent<ScenarioTutorial>().CompleteTutorial();
        }

        protected IEnumerator PlayReqlique(ScenarioTutorialReplique replique)
        {
            DroneHUD.Instance?.SetTask(replique.text);
            DroneHUD.Instance?.SetMessage(MessageType.Normal, replique.text, replique.clip != null ? replique.clip.length : 2);

            if (replique.clip != null)
            {
                _source.PlayOneShot(replique.clip);
                yield return new WaitForSeconds(replique.clip.length);
            }
            else
            {
                yield return new WaitForSeconds(2);
            }
        }
    }
}