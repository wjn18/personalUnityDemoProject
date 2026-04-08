using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;         //  MenuPanel
    public GameObject settingsPanel;     //  SettingsPanel

    [Header("Gameplay UI")]
    public GameObject hudPanel;          //  HUD
    public GameObject interactionPanel;  //  InteractionPanel

    [Header("Pause")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Cursor")]
    public bool showCursorInGameplay = true;

    private GameObject lastMenuPanel;
    private bool isPaused;

    void Start()
    {
        ResumeGameImmediate();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                // 如果当前在设置页，先返回菜单
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    BackFromSettings();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    // =========================
    // Pause / Resume
    // =========================

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        HideGameplayUI();
        HideAllMenuPanels();

        if (menuPanel != null)
            menuPanel.SetActive(true);

        lastMenuPanel = menuPanel;

        // 鼠标显示出来
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        HideAllMenuPanels();
        ShowGameplayUI();

        // 锁鼠标

        if (showCursorInGameplay)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 给 Start 用，避免开场闪菜单
    void ResumeGameImmediate()
    {
        isPaused = false;
        Time.timeScale = 1f;

        HideAllMenuPanels();
        ShowGameplayUI();
    }

    // =========================
    // Menu Navigation
    // =========================

    public void ShowMainMenu()
    {
        HideAllMenuPanels();

        if (menuPanel != null)
            menuPanel.SetActive(true);

        lastMenuPanel = menuPanel;
    }

    public void ShowSettingsFromMainMenu()
    {
        lastMenuPanel = menuPanel;

        HideAllMenuPanels();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        HideAllMenuPanels();

        if (lastMenuPanel != null)
            lastMenuPanel.SetActive(true);
        else if (menuPanel != null)
            menuPanel.SetActive(true);
    }

    // =========================
    // Helpers
    // =========================

    void HideAllMenuPanels()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void HideGameplayUI()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }

    void ShowGameplayUI()
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        if (interactionPanel != null) interactionPanel.SetActive(true);
    }

    // =========================
    // Optional
    // =========================

    public bool IsPaused()
    {
        return isPaused;
    }

    void OnDestroy()
    {
        // 防止脚本被销毁时游戏还保持暂停
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}