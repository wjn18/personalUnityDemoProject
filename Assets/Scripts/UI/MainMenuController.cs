using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Load To Play Scene")]
    public string gameSceneName = " ";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
        Debug.Log ("Load to the game");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
