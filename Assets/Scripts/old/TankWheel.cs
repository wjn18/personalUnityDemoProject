using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class TankWheel : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float rotationspeed = 150f;
    public float angle = 2f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    

    void FixedUpdate()
    {

        if (Input.GetKey(KeyCode.W))
        {

            
            transform.rotation = Quaternion.AngleAxis(angle, axis: transform.up) * transform.rotation;

            Vector3 dirWorld = Vector3.forward; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;

            //transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            print("rotating forward");
        }
        if (Input.GetKey(KeyCode.S))
        {


            transform.rotation = Quaternion.AngleAxis(angle, axis: transform.up) * transform.rotation;

            Vector3 dirWorld = Vector3.back; // 换成 Vector3.right / Vector3.left 等即可
            transform.position += dirWorld * moveSpeed * Time.fixedDeltaTime;

            //transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            print("rotating backward");
        }
    }
}
