using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1); // loads your first room scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}