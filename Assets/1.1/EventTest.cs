using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void NotifyEventHandler(string message);

public class Publisher
{
    public event NotifyEventHandler mNotify;

    public void TriggerEvent()
    {
        if (mNotify != null)
        {
            mNotify("666");
        }
    }
}

public class Subscriber
{
    public void OnNotify(string message)
    {
        Debug.Log("Recieved!");
    }
}
public class EventTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Publisher publisher = new Publisher();
        Subscriber subscriber = new Subscriber();
        publisher.mNotify += subscriber.OnNotify;

        publisher.TriggerEvent();
    }
}
