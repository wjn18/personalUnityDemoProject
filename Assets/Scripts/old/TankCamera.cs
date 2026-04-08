using UnityEngine;

public class TankCameraFollow : MonoBehaviour
{
    [Header("跟随对象")]
    public Transform TurretPivot;
    public Transform gunPitchPivot;

    [Header("位置偏移")]
    public Vector3 offset = new Vector3(0f, 4f, -6f);

    [Header("位置平滑")]
    public float positionSmooth = 8f;

    [Header("镜头最大旋转速度")]
    public float cameraYawSpeed = 70f;
    public float cameraPitchSpeed = 50f;

    void LateUpdate()
    {
        if (TurretPivot == null || gunPitchPivot == null) return;

        // 跟随位置
        Vector3 targetPos = TurretPivot.position + TurretPivot.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, positionSmooth * Time.deltaTime);

        // ===== 跟随 Yaw =====
        Quaternion targetYaw = Quaternion.Euler(0f, TurretPivot.eulerAngles.y, 0f);

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Quaternion currentYaw = Quaternion.Euler(0f, currentEuler.y, 0f);

        Quaternion newYaw = Quaternion.RotateTowards(
            currentYaw,
            targetYaw,
            cameraYawSpeed * Time.deltaTime
        );

        // ===== 跟随 Pitch（现在读 X 轴）=====
        float gunPitchX = gunPitchPivot.localEulerAngles.x;
        if (gunPitchX > 180f) gunPitchX -= 360f;

        Quaternion targetPitch = Quaternion.Euler(gunPitchX, 0f, 0f);
        Quaternion currentPitch = Quaternion.Euler(currentEuler.x, 0f, 0f);

        Quaternion newPitch = Quaternion.RotateTowards(
            currentPitch,
            targetPitch,
            cameraPitchSpeed * Time.deltaTime
        );

        // 合并
        transform.rotation = newYaw * newPitch;
    }
}