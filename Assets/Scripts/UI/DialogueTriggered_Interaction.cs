using UnityEngine;

public class DialogueTrigger_OnInteract : DialogueTriggerBase
{
    [Header("Interact Settings")]
    public string targetTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("State")]
    public bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            playerInRange = false;
        }
    }
}