using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariebleTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        double price = 99.99;
        int s = (int)price;
        string Name = "DickBro";
        int Level = 10;
        float hp = 1000.0f;
        Debug.LogFormat("Name:{0}, Level:{1}, Hp:{2}", Name, Level, hp);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
