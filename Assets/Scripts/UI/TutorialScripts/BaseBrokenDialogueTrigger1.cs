using UnityEngine;

public class BaseBrokenDialogueTrigger : MonoBehaviour
{
    [Header("Refs")]
    public DialogueEventTrigger baseDialogue;
    public BaseRuntime baseRuntime;

    [Header("Settings")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    void Awake()
    {
        if (baseRuntime == null)
            baseRuntime = GetComponent<BaseRuntime>();
    }

    void OnEnable()
    {
        if (baseRuntime != null)
            baseRuntime.OnBroken += HandleBaseBroken;
    }

    void OnDisable()
    {
        if (baseRuntime != null)
            baseRuntime.OnBroken -= HandleBaseBroken;
    }

    void HandleBaseBroken()
    {

        hasTriggered = true;

        if (baseDialogue != null)
            baseDialogue.TriggerDialogue();
        else
            Debug.LogWarning($"{name}: baseDialogue Ã»ÓÐ°ó¶¨");
    }
}