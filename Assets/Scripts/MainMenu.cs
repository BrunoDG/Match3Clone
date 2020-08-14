using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() 
    {
        Debug.Log("Let's go!");
        SceneManager.LoadScene("GameScene");
    }

    public void Exit()
    {
        Debug.Log("You've quitted the game. Thanks for playing!");
        Application.Quit();
    }
}
