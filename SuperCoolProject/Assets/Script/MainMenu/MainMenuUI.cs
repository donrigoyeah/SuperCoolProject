using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        // TODO: ADD Seed or something
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
