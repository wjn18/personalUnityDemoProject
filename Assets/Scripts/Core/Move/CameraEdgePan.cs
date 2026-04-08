using UnityEngine;

public class CameraEdgePan : MonoBehaviour
{
    [Header("Neutral")]
    public Vector3 neutralLocalPosition = new Vector3(0f, 12f, -8f);

    [Header("Edge Pan")]
    public float edgeSize = 80f;          // 距离屏幕边缘多少像素开始触发
    public float maxOffsetX = 4f;         // 左右最大偏移
    public float maxOffsetZ = 4f;         // 前后最大偏移
    public float moveSpeed = 8f;          // 偏移变化速度
    public bool useSmoothReturn = true;

    [Header("Direction Mapping")]
    public bool invertHorizontal = false; // 如果左右反了就勾这个
    public bool invertVertical = false;   // 如果上下反了就勾这个

    private Vector3 currentOffset;

    void LateUpdate()
    {
        Vector3 mouse = Input.mousePosition;

        float targetX = 0f;
        float targetZ = 0f;

        // 左右边缘
        if (mouse.x <= edgeSize)
        {
            float t = 1f - (mouse.x / edgeSize);
            targetX = -Mathf.Clamp(t, 0f, 1f) * maxOffsetX;
        }
        else if (mouse.x >= Screen.width - edgeSize)
        {
            float t = (mouse.x - (Screen.width - edgeSize)) / edgeSize;
            targetX = Mathf.Clamp(t, 0f, 1f) * maxOffsetX;
        }

        // 上下边缘
        if (mouse.y <= edgeSize)
        {
            float t = 1f - (mouse.y / edgeSize);
            targetZ = -Mathf.Clamp(t, 0f, 1f) * maxOffsetZ;
        }
        else if (mouse.y >= Screen.height - edgeSize)
        {
            float t = (mouse.y - (Screen.height - edgeSize)) / edgeSize;
            targetZ = Mathf.Clamp(t, 0f, 1f) * maxOffsetZ;
        }

        if (invertHorizontal) targetX = -targetX;
        if (invertVertical) targetZ = -targetZ;

        Vector3 targetOffset = new Vector3(targetX, 0f, targetZ);

        if (useSmoothReturn)
        {
            currentOffset = Vector3.Lerp(
                currentOffset,
                targetOffset,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            currentOffset = Vector3.MoveTowards(
                currentOffset,
                targetOffset,
                moveSpeed * Time.deltaTime
            );
        }

        // 保险再限制一次最大范围
        currentOffset.x = Mathf.Clamp(currentOffset.x, -maxOffsetX, maxOffsetX);
        currentOffset.z = Mathf.Clamp(currentOffset.z, -maxOffsetZ, maxOffsetZ);

        transform.localPosition = neutralLocalPosition + currentOffset;
    }
}