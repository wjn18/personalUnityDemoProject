using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class dictionaryTest : MonoBehaviour
{
    

    // Start is called before the first frame update
    public void Start()
    {
        string s ="hello world";

        Dictionary<char, int> counts = new Dictionary<char,int>();
        foreach (char c in s)
        {
            if (c == ' ')  continue;
            if (counts.ContainsKey(c))
            {
                counts[c]++;
            }
            else
            {
                counts[c] = 1;
            }

            foreach (var pair in counts)
            {
                Debug.Log(pair.Key + " has" + pair.Value + " times");
            }
            
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
