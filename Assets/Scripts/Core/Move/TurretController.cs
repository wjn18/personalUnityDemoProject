using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Camera cam;
    public float rotateSpeed = 10f;
    public LayerMask groundMask;

    [Header("Tutorial Control")]
    public bool canAim = true;

    void Update()
    {
        if (!canAim) return;

        if (cam == null)
            cam = Camera.main;

        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 targetPoint = hit.point;
            RotateToPoint(targetPoint);
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 targetPoint = ray.GetPoint(enter);
                RotateToPoint(targetPoint);
            }
        }
    }

    void RotateToPoint(Vector3 targetPoint)
    {
        Vector3 dir = targetPoint - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}