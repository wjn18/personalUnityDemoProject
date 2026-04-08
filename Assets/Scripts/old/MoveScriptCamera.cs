using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScriptCamera : MonoBehaviour
{

    public float moveSpeed = 3f;
    
        // Start is called before the first frame update
        void Start()
        {
            
        }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 dirWorld = Vector3.right; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;
            print("camera_moving");
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 dirWorld = Vector3.left; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;
            print("camera_moving");
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 dirWorld = Vector3.forward; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;
            print("camera_moving");
        }

        if (Input.GetKey(KeyCode.D))
        {
            Vector3 dirWorld = Vector3.back; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;
            print("camera_moving");
        }
    }
}
