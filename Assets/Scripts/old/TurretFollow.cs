using UnityEngine;

public class TurretFollow : MonoBehaviour
{
    public float sensitivity = 180f;
    public bool lockCursor = true;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        if (Mathf.Abs(mouseX) < 0.0001f) return;

        float yaw = mouseX * sensitivity * Time.deltaTime;

        transform.Rotate(0f, yaw, 0f, Space.Self);
    }
}