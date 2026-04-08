using UnityEngine;

public class TurretFollowTankPosition : MonoBehaviour
{
    public Transform tankRoot;
    public Vector3 offset;

    void LateUpdate()
    {
        if (tankRoot == null) return;

        transform.position = tankRoot.position + offset;
    }
}
