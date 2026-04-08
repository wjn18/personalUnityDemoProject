using Unity.VisualScripting;
using UnityEngine;

public class TankControllerSimple : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform forwardReference; // ÍÏ BodyRoot ½øÀ´

    void Update()
    {
        float forward = 0f;
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.D)) forward = 1f;
        else if (Input.GetKey(KeyCode.A)) forward = -1f;
        else if (Input.GetKey(KeyCode.S)) horizontal = -1f;
        else if (Input.GetKey(KeyCode.W)) horizontal = 1f;

        if (forwardReference == null) forwardReference = transform;

        Vector3 forwardDir = forwardReference.forward;
        Vector3 rightDir = forwardReference.right;

        forwardDir.y = 0f;
        rightDir.y = 0f;
        forwardDir.Normalize();
        rightDir.Normalize();

        Vector3 moveDir = forwardDir * forward + rightDir * horizontal;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}