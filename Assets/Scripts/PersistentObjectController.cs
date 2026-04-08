using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PersistentObjectController : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public enum GameScene
    {
        MainMenu,
        LoginScene,
        Level1,
        Level2,
        Boss,
        Credits,
        Settings
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameScene currentScene = (GameScene)scene.buildIndex;

        switch (currentScene)
        {
            case GameScene.MainMenu:
            case GameScene.LoginScene:
                Destroy(gameObject);
                break;

            case GameScene.Level1:
            case GameScene.Level2:
                gameObject.SetActive(true);
                break;

            case GameScene.Credits:
            case GameScene.Settings:
                gameObject.SetActive(false);
                break;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}