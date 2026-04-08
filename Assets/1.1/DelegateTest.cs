using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class DelegateTest : MonoBehaviour
{
    // Start is called before the first frame update


    public delegate int MathDelegate(int x, int y);

    public int Multyply(int valueA, int valueB)
    {
        int valueC = valueA * valueB;
        Debug.Log(valueC);
        return valueC;
    }
    void Start()
    {
        MathDelegate area = new MathDelegate(Multyply);
        area(3, 4);
    }
   
}
