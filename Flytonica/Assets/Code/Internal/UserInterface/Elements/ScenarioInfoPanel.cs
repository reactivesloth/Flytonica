using Code.Internal.SceneManagement;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class ScenarioInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text typeMarkText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        public void Open(ScenarioSettings scenarioSettings)
        {
            gameObject.SetActive(true);
            Init(scenarioSettings);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        private void Init(ScenarioSettings scenarioSettings)
        {
            titleText.text = scenarioSettings.name;
        }
    }
}
