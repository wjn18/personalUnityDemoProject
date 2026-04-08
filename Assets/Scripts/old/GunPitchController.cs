using UnityEngine;

public class GunPitchController : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float sensitivity = 80f;

    [Header("最大旋转速度")]
    public float maxPitchSpeed = 60f;

    [Header("俯仰角限制")]
    public float minAngle = -15f;
    public float maxAngle = 15f;

    float currentPitch = 0f;

    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseY) < 0.001f) return;

        float pitchSpeed = -mouseY * sensitivity;

        // 限制最大速度
        pitchSpeed = Mathf.Clamp(pitchSpeed, -maxPitchSpeed, maxPitchSpeed);

        currentPitch += pitchSpeed * Time.deltaTime;

        // 限制角度
        currentPitch = Mathf.Clamp(currentPitch, minAngle, maxAngle);

        // 绕本地 X 轴旋转
        transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}