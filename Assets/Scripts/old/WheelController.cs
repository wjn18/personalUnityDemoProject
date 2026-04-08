using UnityEngine;

public class WheelController : MonoBehaviour
{
    public Transform[] wheels;

    [Tooltip("轮子半径（metre）")]
    public float wheelRadius = 0.25f;

    [Tooltip("如果前进时轮子倒转，勾上它")]
    public bool invert;

    Vector3 lastPos;
    public Vector3 rollAxis = Vector3.right;

    void Start()
    {
        lastPos = transform.position;
      
}

    void Update()
    {
       
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;

        // 只取“沿车身前方”的移动距离
        float forwardDist = Vector3.Dot(delta, transform.forward);

        // 距离 -> 轮子转角： angle(rad)=s/r
        float angleDeg = (forwardDist / Mathf.Max(0.0001f, wheelRadius)) * Mathf.Rad2Deg;
        if (invert) angleDeg = -angleDeg;

        foreach (var w in wheels)
        {
           
            if (!w) continue;
            // 轮子绕本地X轴滚
            w.Rotate(rollAxis, angleDeg, Space.Self);
        }
    }
}