using UnityEngine;

public class TurretYawController : MonoBehaviour
{
    public float sensitivity = 120f;

    [Header("最大旋转速度（度/秒）")]
    public float maxYawSpeed = 90f;

    [Header("是否锁定鼠标")]
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
        if (Mathf.Abs(mouseX) < 0.001f) return;

        // 鼠标输入转成目标角速度
        float yawSpeed = mouseX * sensitivity;

        // 限制最大速度
        yawSpeed = Mathf.Clamp(yawSpeed, -maxYawSpeed, maxYawSpeed);

        // 这一帧转多少角度
        float yawDelta = yawSpeed * Time.deltaTime;

        transform.Rotate(0f, yawDelta, 0f, Space.Self);
    }
}