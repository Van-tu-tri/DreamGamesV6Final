using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GamePlayScene");
    }
}
