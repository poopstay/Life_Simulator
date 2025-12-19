using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene("01_Room");
        }
        else
        {
            // fallback nếu chưa có fader
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_Room");
        }
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("01_Room");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game!");
    }
}