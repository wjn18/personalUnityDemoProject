using UnityEngine;

public class MuzzlePitchController : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float sensitivity = 80f;

    [Header("最大旋转速度（度/秒）")]
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
        pitchSpeed = Mathf.Clamp(pitchSpeed, -maxPitchSpeed, maxPitchSpeed);

        currentPitch += pitchSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minAngle, maxAngle);

        // 你的模型上下是绕本地 Z 轴
        transform.localRotation = Quaternion.AngleAxis(currentPitch, Vector3.forward);
    }
}