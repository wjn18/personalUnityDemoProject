using UnityEngine;



public class DialogueTrigger_OnEnter : DialogueTriggerBase
{
    [Header("Enter Settings")]
    public string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            TriggerDialogue();
        }
    }
}