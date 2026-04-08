using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankRigidbodyController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float turnSpeed = 90f;   // 每秒转多少度
    public float sideFriction = 8f;

    Rigidbody rb;

    float forwardInput;
    float turnInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        forwardInput = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        turnInput = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
    }

    void FixedUpdate()
    {
        // ===== 1. 前进/后退 =====
        Vector3 move = transform.forward * (forwardInput * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + move);

        // ===== 2. 左右转向 =====
        if (turnInput != 0f)
        {
            float turnAmount = turnInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // ===== 3. 防侧滑 =====
        Vector3 vel = rb.velocity;
        Vector3 flatVel = new Vector3(vel.x, 0f, vel.z);

        Vector3 right = transform.right;
        float sideSpeed = Vector3.Dot(flatVel, right);
        Vector3 sideVel = right * sideSpeed;

        rb.AddForce(-sideVel * sideFriction, ForceMode.Acceleration);
    }
}