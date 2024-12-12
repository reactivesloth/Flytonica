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
        if (tablet != null)
            tablet.SetActive(false);
        if (gamepad != null)
            gamepad.SetActive(false);
    }

    private void SceneChanged(Scene arg0, Scene arg1)
    {
        if (tablet != null)
            tablet.SetActive(false);
        if (gamepad != null)
            gamepad.SetActive(false);

        if (!SceneManager.GetActiveScene().name.Equals("Main") && !SceneManager.GetActiveScene().name.Equals("UI"))
        {
            if (HttpClient.UserData.type == UserType.Teacher)
            {
                if (gamepad != null)
                    gamepad.SetActive(false);
                if (tablet != null)
                    tablet.SetActive(true);
            }
            else
            {
                if (tablet != null)
                    tablet.SetActive(true);
                if (gamepad != null)
                    gamepad.SetActive(true);
            }
        }
    }
}
