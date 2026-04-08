using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider mouseSensitiveSlider;
    public Toggle invertAxisYToggle;

    [Header("Default Values")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;
    [Range(0f, 1f)] public float defaultSfxVolume = 0.5f;
    [Range(0f, 1f)] public float defaultMouseSensitive = 0.5f;
    public bool defaultInvertY = false;

    float currentMusicVolume;
    float currentSfxVolume;
    float currentMouseSensitive;
    bool currentInvertY;

    const string MusicKey = "MusicVolume";
    const string SfxKey = "SFXVolume";
    const string MouseKey = "MouseSensitive";
    const string InvertKey = "InvertY";

    void Start()
    {
        LoadSettingsToUI();
    }

    // =========================
    // UI Callbacks
    // =========================

    public void OnMusicSliderChanged(float value)
    {
        currentMusicVolume = value;
        ApplyRuntimeSettings();
    }

    public void OnSFXSliderChanged(float value)
    {
        currentSfxVolume = value;
        ApplyRuntimeSettings();
    }

    public void OnMouseSensitiveSliderChanged(float value)
    {
        currentMouseSensitive = value;
        ApplyRuntimeSettings();
    }

    public void OnInvertYToggleChanged(bool value)
    {
        currentInvertY = value;
        ApplyRuntimeSettings();
    }

    // =========================
    // Buttons
    // =========================

    public void ApplySettings()
    {
        PlayerPrefs.SetFloat(MusicKey, currentMusicVolume);
        PlayerPrefs.SetFloat(SfxKey, currentSfxVolume);
        PlayerPrefs.SetFloat(MouseKey, currentMouseSensitive);
        PlayerPrefs.SetInt(InvertKey, currentInvertY ? 1 : 0);
        PlayerPrefs.Save();

        ApplyRuntimeSettings();

        Debug.Log("Settings applied.");
    }

    public void ResetSettings()
    {
        currentMusicVolume = defaultMusicVolume;
        currentSfxVolume = defaultSfxVolume;
        currentMouseSensitive = defaultMouseSensitive;
        currentInvertY = defaultInvertY;

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(currentMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(currentSfxVolume);

        if (mouseSensitiveSlider != null)
            mouseSensitiveSlider.SetValueWithoutNotify(currentMouseSensitive);

        if (invertAxisYToggle != null)
            invertAxisYToggle.SetIsOnWithoutNotify(currentInvertY);

        ApplyRuntimeSettings();

        Debug.Log("Settings reset to defaults.");
    }

    public void LoadSettingsToUI()
    {
        currentMusicVolume = PlayerPrefs.GetFloat(MusicKey, defaultMusicVolume);
        currentSfxVolume = PlayerPrefs.GetFloat(SfxKey, defaultSfxVolume);
        currentMouseSensitive = PlayerPrefs.GetFloat(MouseKey, defaultMouseSensitive);
        currentInvertY = PlayerPrefs.GetInt(InvertKey, defaultInvertY ? 1 : 0) == 1;

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(currentMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(currentSfxVolume);

        if (mouseSensitiveSlider != null)
            mouseSensitiveSlider.SetValueWithoutNotify(currentMouseSensitive);

        if (invertAxisYToggle != null)
            invertAxisYToggle.SetIsOnWithoutNotify(currentInvertY);

        ApplyRuntimeSettings();
    }

    // =========================
    // Runtime Apply
    // =========================

    void ApplyRuntimeSettings()
    {
        // 临时做法：总音量先跟 music 走
        AudioListener.volume = currentMusicVolume;

        // 鼠标灵敏度、反转Y 不会自动作用到炮塔/镜头
        // 需要控制脚本主动读取 GetMouseSensitivity() 和 GetInvertY()
    }

    // =========================
    // Public Getters
    // 给别的脚本读取当前设置
    // =========================

    public static float GetSavedMouseSensitivity(float defaultValue = 0.5f)
    {
        return PlayerPrefs.GetFloat(MouseKey, defaultValue);
    }

    public static bool GetSavedInvertY(bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(InvertKey, defaultValue ? 1 : 0) == 1;
    }

    public float GetCurrentMouseSensitivity()
    {
        return currentMouseSensitive;
    }

    public bool GetCurrentInvertY()
    {
        return currentInvertY;
    }
}