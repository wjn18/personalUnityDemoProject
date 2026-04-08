using UnityEngine;

public class DialogueSequenceLinker : MonoBehaviour
{
    [Header("Refs")]
    public DialogueUI dialogueUI;

    [Header("Chain")]
    public DialogueSequence fromSequence;
    public DialogueSequence toSequence;

    [Header("Settings")]
    public bool linkOnce = true;

    private bool hasLinked = false;

    private void OnEnable()
    {
        if (dialogueUI != null)
            dialogueUI.OnDialogueFinishedSequence += HandleDialogueFinishedSequence;
    }

    private void OnDisable()
    {
        if (dialogueUI != null)
            dialogueUI.OnDialogueFinishedSequence -= HandleDialogueFinishedSequence;
    }

    void HandleDialogueFinishedSequence(DialogueSequence finishedSequence)
    {
        if (linkOnce && hasLinked) return;

        if (finishedSequence == fromSequence)
        {
            hasLinked = true;

            if (dialogueUI != null && toSequence != null)
            {
                dialogueUI.StartDialogue(toSequence);
            }
        }
    }
}