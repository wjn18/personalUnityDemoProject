using UnityEngine;

public class BaseTakenDialogueTrigger : MonoBehaviour
{
    [Header("Refs")]
    public BaseRuntime baseRuntime;
    public DialogueEventTrigger moveOnDialogue;

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
            baseRuntime.OnTaken += HandleBaseTaken;
    }

    void OnDisable()
    {
        if (baseRuntime != null)
            baseRuntime.OnTaken -= HandleBaseTaken;
    }

    void HandleBaseTaken()
    {

        hasTriggered = true;

        if (moveOnDialogue != null)
        {
            moveOnDialogue.TriggerDialogue();
        }
        else
        {
            Debug.LogWarning($"{name}: moveOnDialogue Ã»ÓÐ°ó¶¨");
        }
    }
}