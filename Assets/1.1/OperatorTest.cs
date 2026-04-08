using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class  Box 
{
    private double length;
    private double width;
    private double height;
    public double _Volume;
    public double Rate;

    public double Volume
    {
        get
        {
            return _Volume;
        }
        set
        {
            _Volume = value;
        }
    }
    public Box() { }
    public Box(double length, double width, double height)
    {
        this.length = length;
        this.width = width;
        this.height = height;
        this.Volume = length * width * height;
    }

    public static Box operator + (Box box1 , Box box2)
    {
        Box box = new Box();
        box.Volume = box1.Volume + box2.Volume;
        return box;
    }

    public static Box operator / (Box box1, Box box2)
    {
        Box box = new Box();
        box.Rate = box1.Volume / box2.Volume;
        return box;
    }

}


public class OperatorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Box box1 = new Box(2, 2, 2);
        Box box2 = new Box(4, 4, 2);
        Box box3 = box1 / box2;
        Debug.Log("rate is " + box3.Rate);

    }

    // Update is called once per frame
    
}
