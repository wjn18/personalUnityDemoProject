using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialController : MonoBehaviour
{
    public enum StepTriggerType
    {
        ByLineIndex,
        ByDialogueSequenceFinished
    }

    [System.Serializable]
    public class TutorialStep
    {
        [Header("Trigger Type")]
        public StepTriggerType triggerType = StepTriggerType.ByLineIndex;
        public bool triggerOnce = true;

        [Header("If Triggered By Line")]
        public int triggerLine = 0;

        [Header("If Triggered By Sequence Finish")]
        public DialogueSequence triggerSequence;

        [Header("Unlock Abilities")]
        public bool unlockLook = false;
        public bool unlockMove = false;
        public bool unlockFire = false;

        [Header("Optional: Force Ability State")]
        public bool setLookState = false;
        public bool lookEnabled = false;

        public bool setMoveState = false;
        public bool moveEnabled = false;

        public bool setFireState = false;
        public bool fireEnabled = false;

        [Header("Objects To Enable")]
        public List<GameObject> objectsToEnable = new List<GameObject>();

        [Header("Objects To Disable")]
        public List<GameObject> objectsToDisable = new List<GameObject>();

        [Header("Extra Events")]
        public UnityEvent onTriggered;

        [HideInInspector] public bool hasTriggered = false;
    }

    [Header("Dialogue")]
    public DialogueUI dialogueUI;
    public bool autoStartDialogue = false;

    [Header("Optional Auto Start Sequence")]
    public DialogueSequence autoStartSequence;

    [Header("Player Scripts (Optional)")]
    public MonoBehaviour lookScript;
    public MonoBehaviour moveScript;
    public MonoBehaviour fireScript;

    [Header("Initial Lock State")]
    public bool lockLookAtStart = true;
    public bool lockMoveAtStart = true;
    public bool lockFireAtStart = true;

    [Header("Cursor Control")]
    public bool controlCursorWithLookState = false;
    public bool cursorVisibleWhenLookLocked = true;
    public bool lockCursorWhenLookEnabled = false;

    [Header("Steps")]
    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("When Any Dialogue Finished")]
    public bool unlockAllWhenDialogueFinished = false;

    [Header("Optional Events")]
    public UnityEvent onTutorialStarted;
    public UnityEvent onDialogueFinished;
    public UnityEvent onLookUnlocked;
    public UnityEvent onMoveUnlocked;
    public UnityEvent onFireUnlocked;

    public bool LookUnlocked { get; private set; }
    public bool MoveUnlocked { get; private set; }
    public bool FireUnlocked { get; private set; }

    void Start()
    {
        ApplyInitialLocks();

        if (dialogueUI != null)
        {
            dialogueUI.OnLineChanged += HandleLineChanged;
            dialogueUI.OnDialogueFinished += HandleDialogueFinished;
            dialogueUI.OnDialogueFinishedSequence += HandleDialogueFinishedSequence;
        }
        else
        {
            Debug.LogWarning("TutorialController: dialogueUI Ã»ÓÐ°ó¶¨¡£");
        }

        onTutorialStarted?.Invoke();

        if (autoStartDialogue && dialogueUI != null)
        {
            if (autoStartSequence != null)
                dialogueUI.StartDialogue(autoStartSequence);
            else
                dialogueUI.StartDialogue();
        }
    }

    void OnDestroy()
    {
        if (dialogueUI != null)
        {
            dialogueUI.OnLineChanged -= HandleLineChanged;
            dialogueUI.OnDialogueFinished -= HandleDialogueFinished;
            dialogueUI.OnDialogueFinishedSequence -= HandleDialogueFinishedSequence;
        }
    }

    void ApplyInitialLocks()
    {
        LookUnlocked = !lockLookAtStart;
        MoveUnlocked = !lockMoveAtStart;
        FireUnlocked = !lockFireAtStart;

        ApplyLookState(LookUnlocked);
        ApplyMoveState(MoveUnlocked);
        ApplyFireState(FireUnlocked);
    }

    void HandleLineChanged(int lineIndex)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            TutorialStep step = steps[i];
            if (step == null) continue;
            if (step.triggerType != StepTriggerType.ByLineIndex) continue;
            if (step.triggerOnce && step.hasTriggered) continue;

            if (lineIndex >= step.triggerLine)
            {
                ExecuteStep(step);
            }
        }
    }

    void HandleDialogueFinished()
    {
        if (unlockAllWhenDialogueFinished)
        {
            UnlockLook();
            UnlockMove();
            UnlockFire();
        }

        onDialogueFinished?.Invoke();
    }

    void HandleDialogueFinishedSequence(DialogueSequence finishedSequence)
    {
        if (finishedSequence == null) return;

        for (int i = 0; i < steps.Count; i++)
        {
            TutorialStep step = steps[i];
            if (step == null) continue;
            if (step.triggerType != StepTriggerType.ByDialogueSequenceFinished) continue;
            if (step.triggerOnce && step.hasTriggered) continue;

            if (step.triggerSequence == finishedSequence)
            {
                ExecuteStep(step);
            }
        }
    }

    void ExecuteStep(TutorialStep step)
    {
        if (step.triggerOnce)
            step.hasTriggered = true;

        if (step.unlockLook)
            UnlockLook();

        if (step.unlockMove)
            UnlockMove();

        if (step.unlockFire)
            UnlockFire();

        if (step.setLookState)
            SetLookEnabled(step.lookEnabled);

        if (step.setMoveState)
            SetMoveEnabled(step.moveEnabled);

        if (step.setFireState)
            SetFireEnabled(step.fireEnabled);

        if (step.objectsToEnable != null)
        {
            for (int i = 0; i < step.objectsToEnable.Count; i++)
            {
                if (step.objectsToEnable[i] != null)
                    step.objectsToEnable[i].SetActive(true);
            }
        }

        if (step.objectsToDisable != null)
        {
            for (int i = 0; i < step.objectsToDisable.Count; i++)
            {
                if (step.objectsToDisable[i] != null)
                    step.objectsToDisable[i].SetActive(false);
            }
        }

        step.onTriggered?.Invoke();
    }

    // =========================
    // Public API
    // =========================

    public void StartTutorialDialogue()
    {
        if (dialogueUI != null)
        {
            if (autoStartSequence != null)
                dialogueUI.StartDialogue(autoStartSequence);
            else
                dialogueUI.StartDialogue();
        }
    }

    public void StartDialogueSequence(DialogueSequence sequence)
    {
        if (dialogueUI != null && sequence != null)
            dialogueUI.StartDialogue(sequence);
    }

    public void UnlockLook()
    {
        if (LookUnlocked) return;

        LookUnlocked = true;
        ApplyLookState(true);
        onLookUnlocked?.Invoke();
    }

    public void UnlockMove()
    {
        if (MoveUnlocked) return;

        MoveUnlocked = true;
        ApplyMoveState(true);
        onMoveUnlocked?.Invoke();
    }

    public void UnlockFire()
    {
        if (FireUnlocked) return;

        FireUnlocked = true;
        ApplyFireState(true);
        onFireUnlocked?.Invoke();
    }

    public void LockLook()
    {
        LookUnlocked = false;
        ApplyLookState(false);
    }

    public void LockMove()
    {
        MoveUnlocked = false;
        ApplyMoveState(false);
    }

    public void LockFire()
    {
        FireUnlocked = false;
        ApplyFireState(false);
    }

    public void SetLookEnabled(bool enabled)
    {
        LookUnlocked = enabled;
        ApplyLookState(enabled);
    }

    public void SetMoveEnabled(bool enabled)
    {
        MoveUnlocked = enabled;
        ApplyMoveState(enabled);
    }

    public void SetFireEnabled(bool enabled)
    {
        FireUnlocked = enabled;
        ApplyFireState(enabled);
    }

    public void ResetAllSteps()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i] != null)
                steps[i].hasTriggered = false;
        }
    }

    public void JumpToLineAndEvaluate(int lineIndex)
    {
        HandleLineChanged(lineIndex);
    }

    // =========================
    // Internal Apply
    // =========================

    void ApplyLookState(bool enabled)
    {
        if (lookScript != null)
            lookScript.enabled = enabled;

        if (controlCursorWithLookState)
        {
            if (enabled)
            {
                Cursor.visible = false;

                if (lockCursorWhenLookEnabled)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = cursorVisibleWhenLookLocked;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void ApplyMoveState(bool enabled)
    {
        if (moveScript != null)
            moveScript.enabled = enabled;
    }

    void ApplyFireState(bool enabled)
    {
        if (fireScript != null)
            fireScript.enabled = enabled;
    }
}