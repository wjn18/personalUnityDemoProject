using UnityEngine;

public class TankBodyMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float rotateSpeed = 12f;

    private Vector3 moveDir;

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        moveDir = new Vector3(x, 0f, z);

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}