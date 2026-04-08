using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;





public struct Rectangle {
    private int Length;
    private int Width;

    public Rectangle(int length, int width)
    {
        Length = length;
        Width = width;
    }

    public int CalcuArea()
    {
        return Length * Width;
    }
    


}


public class StructTest : MonoBehaviour
{
    // Start is called before the first frame update

    private void Start()
    {
        Rectangle R1 = new Rectangle(10, 20);
        int s = R1.CalcuArea();
        Debug.Log( s );
    }

}
