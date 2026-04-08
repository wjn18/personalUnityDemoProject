using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseInteractUI : MonoBehaviour
{
    [Header("UI Refs")]
    public GameObject root;
    public TMP_Text hintText;
    public Slider progressSlider;

    private bool visible = false;

    void Awake()
    {
        Debug.Log($"[{name}] BaseInteractUI Awake");
        Hide();
    }

    public void Show(string text, float progress01)
    {
        if (!visible)
        {
            Debug.Log($"[{name}] Show()");
            visible = true;
        }

        if (root != null)
            root.SetActive(true);

        if (hintText != null)
            hintText.text = text;

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = Mathf.Clamp01(progress01);
        }
    }

    public void Hide()
    {
        if (visible)
        {
            Debug.Log($"[{name}] Hide()");
            visible = false;
        }

        if (root != null)
            root.SetActive(false);
    }
}