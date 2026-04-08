using UnityEngine;
using TMPro;

public class HintUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TMP_Text hintText;

    [Header("Preset Messages")]
    [TextArea(2, 4)] public string lookHint = "Move your mouse to look around";
    [TextArea(2, 4)] public string moveHint = "Press WASD to move";
    [TextArea(2, 4)] public string fireHint = "Use the left mouse buttom to fire";
    [TextArea(2, 4)] public string continueHint = "Press space to continue";
    [TextArea(2, 4)] public string forwardHint = "Go north to eliminate the enemy around the base";
    [TextArea(2, 4)] public string takenHint = "Repair the base to take it";
    [TextArea(2, 4)] public string moveOnHint = "Go north to move on";
    public void ShowHint(string message)
    {
        if (root != null)
            root.SetActive(true);

        if (hintText != null)
            hintText.text = message;
    }

    public void HideHint()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void ShowLookHint()
    {
        ShowHint(lookHint);
    }

    public void ShowMoveHint()
    {
        ShowHint(moveHint);
    }

    public void ShowFireHint()
    {
        ShowHint(fireHint);
    }

    public void ShowContinueHint()
    {
        ShowHint(continueHint);
    }
    public void ShowForwardHint()
    {
        ShowHint(forwardHint);
    }
    public void ShowTakenHint()
    {
        ShowHint(takenHint);
    }
    public void ShowMoveOnHint()
    {
        ShowHint(moveOnHint);
    }
    public void ClearHintText()
    {
        if (hintText != null)
            hintText.text = "";
    }
}