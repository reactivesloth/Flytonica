using Code.Internal.API;
using Code.Internal.API.Wrappers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class XRPlayerHandItems : MonoBehaviour
{
    [SerializeField] private GameObject tablet;
    [SerializeField] private GameObject gamepad;

    private void Awake()
    {
        SceneManager.activeSceneChanged += SceneChanged;
        tablet.SetActive(false);
        gamepad.SetActive(false);
    }

    private void SceneChanged(Scene arg0, Scene arg1)
    {
        tablet.SetActive(false);
        gamepad.SetActive(false);

        if (!SceneManager.GetActiveScene().name.Equals("Main") && !SceneManager.GetActiveScene().name.Equals("UI"))
        {
            if (HttpClient.UserData.type == UserType.Teacher)
            {
                gamepad.SetActive(false);
                tablet.SetActive(true);
            }
            else
            {
                tablet.SetActive(true);
                gamepad.SetActive(true);
            }
        }
    }
}
