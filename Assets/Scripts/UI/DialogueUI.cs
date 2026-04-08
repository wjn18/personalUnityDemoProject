using System;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Default Dialogue Content")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Space;

    private int currentIndex = 0;
    private bool isPlaying = false;
    private string[] currentLines;

    public DialogueSequence CurrentSequence { get; private set; }

    public event Action<int> OnLineChanged;
    public event Action OnDialogueFinished;
    public event Action<DialogueSequence> OnDialogueFinishedSequence;

    public bool IsPlaying => isPlaying;
    public int CurrentIndex => currentIndex;

    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(nextKey))
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        CurrentSequence = null;
        StartDialogue(lines);
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        if (sequence == null)
        {
            Debug.LogWarning("DialogueUI: sequence 为空");
            return;
        }

        CurrentSequence = sequence;
        StartDialogue(sequence.lines);
    }

    public void StartDialogue(string[] newLines)
    {
        if (newLines == null || newLines.Length == 0)
        {
            Debug.LogWarning("没有设置对话内容");
            return;
        }

        currentLines = newLines;
        currentIndex = 0;
        isPlaying = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = currentLines[currentIndex];

        OnLineChanged?.Invoke(currentIndex);
    }

    public void NextLine()
    {
        currentIndex++;

        if (currentLines == null || currentIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        if (dialogueText != null)
            dialogueText.text = currentLines[currentIndex];

        OnLineChanged?.Invoke(currentIndex);
    }

    public void EndDialogue()
    {
        isPlaying = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DialogueSequence finishedSequence = CurrentSequence;

        OnDialogueFinished?.Invoke();
        OnDialogueFinishedSequence?.Invoke(finishedSequence);
    }
}