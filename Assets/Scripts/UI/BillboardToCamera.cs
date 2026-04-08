using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    Camera cam;
    public float baseScale = 0.01f;
    public float refDistance = 10f;


    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        transform.forward = cam.transform.forward;
        float d = Vector3.Distance(transform.position, cam.transform.position);
        float s = baseScale * (d / refDistance);
        transform.localScale = Vector3.one * s;
    }
}