using UnityEngine;

public class TankFollowTarget : MonoBehaviour
{
    public Transform body;
    public Vector3 offset;

    void LateUpdate()
    {
        if (body == null) return;

        transform.position = body.position + offset;

        // 固定旋转，不跟随 body 旋转
        transform.rotation = Quaternion.identity;
    }
}