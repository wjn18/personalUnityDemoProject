using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup pressAnyKeyGroup;   // 挂在文字对象上的 CanvasGroup
    public float blinkSpeed = 2.0f;

    [Header("Next")]
    public string nextSceneName = "MainMenu"; // 下一个场景名
    public bool useFade = false;
    public CanvasGroup fadeGroup;          // 可选：黑色全屏 Image 的 CanvasGroup
    public float fadeDuration = 0.5f;

    bool entering = false;

    void Update()
    {
        // 1) 闪烁
        if (pressAnyKeyGroup != null && !entering)
        {
            float a = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            a = Mathf.Lerp(0.2f, 1f, a);
            pressAnyKeyGroup.alpha = a;
        }

        // 2) 任意键/点击进入
        if (!entering && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            entering = true;
            Time.timeScale = 1f;
            StartCoroutine(EnterGame());
        }
    }

    System.Collections.IEnumerator EnterGame()
    {
        // 防止文字还在闪烁
        if (pressAnyKeyGroup != null) pressAnyKeyGroup.alpha = 1f;

        if (useFade && fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = 0f;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
