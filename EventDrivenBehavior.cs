using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDrivenBehavior : MonoBehaviour
{
    protected GlobalEventController eventCtrl;
    protected bool isQuittingApp = false;
    
    protected virtual void Start()
    {
        eventCtrl = GlobalEventController.GetInstance();
        Invoke("InitEvents", 0);
    }

    protected virtual void InitEvents()
    {
        if(!eventCtrl)
        {
            eventCtrl = GlobalEventController.GetInstance();
        }
    }

    protected virtual void UnsubEvents()
    {

    }

    protected virtual void OnApplicationQuit()
    {
        isQuittingApp = true;
        DecommissionObject();
    }

    protected virtual void OnDestroy()
    {
        if(!isQuittingApp)
        {
            DecommissionObject();
        }
    }

    protected virtual void DecommissionObject()
    {
        UnsubEvents();
    }
}
