using UnityEngine;
using UnityEngine.Events;

public abstract class DialogueTriggerBase : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueUI dialogueUI;
    public DialogueSequence dialogueSequence;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    public bool allowTriggerWhenDialoguePlaying = false;

    [Header("Events")]
    public UnityEvent onTriggered;

    protected bool hasTriggered = false;

    protected void TriggerDialogue()
    {
        if (triggerOnce && hasTriggered)
            return;

        if (dialogueUI == null)
        {
            Debug.LogWarning($"{name}: dialogueUI 没有绑定");
            return;
        }

        if (dialogueSequence == null)
        {
            Debug.LogWarning($"{name}: dialogueSequence 没有绑定");
            return;
        }

        if (!allowTriggerWhenDialoguePlaying && dialogueUI.IsPlaying)
        {
            Debug.Log($"{name}: 当前已有对话正在播放，忽略本次触发");
            return;
        }

        hasTriggered = true;
        dialogueUI.StartDialogue(dialogueSequence);
        onTriggered?.Invoke();
    }

    public void TriggerNow()
    {
        TriggerDialogue();
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
