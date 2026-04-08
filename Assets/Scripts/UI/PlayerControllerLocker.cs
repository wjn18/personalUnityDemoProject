using UnityEngine;

public class PlayerControlLocker : MonoBehaviour
{
    [Header("Player Control Scripts")]
    public MonoBehaviour movementScript;
    public MonoBehaviour lookScript;
    public MonoBehaviour attackScript;

    public bool lockCursorWhenPlayable = true;

    public void DisablePlayerControl()
    {
        if (movementScript != null) movementScript.enabled = false;
        if (lookScript != null) lookScript.enabled = false;
        if (attackScript != null) attackScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EnablePlayerControl()
    {
        if (movementScript != null) movementScript.enabled = true;
        if (lookScript != null) lookScript.enabled = true;
        if (attackScript != null) attackScript.enabled = true;

        if (lockCursorWhenPlayable)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}