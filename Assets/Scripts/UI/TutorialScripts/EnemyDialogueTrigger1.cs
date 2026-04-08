using UnityEngine;

public class EnemyDeathDialogueTrigger : MonoBehaviour
{
    [Header("Refs")]
    public DialogueEventTrigger forwardDialogue;
    public EnemyRuntime enemyRuntime;

    [Header("Settings")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    void Awake()
    {
        if (enemyRuntime == null)
            enemyRuntime = GetComponent<EnemyRuntime>();
    }

    void OnEnable()
    {
        if (enemyRuntime != null)
            enemyRuntime.OnDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        if (enemyRuntime != null)
            enemyRuntime.OnDied -= HandleEnemyDied;
    }

    void HandleEnemyDied()
    {

        hasTriggered = true;

        if (forwardDialogue != null)
            forwardDialogue.TriggerDialogue();
        else
            Debug.LogWarning($"{name}: forwardDialogue Ã»ÓÐ°ó¶¨");
    }
}