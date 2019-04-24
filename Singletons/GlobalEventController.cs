using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventTypes { PlayerInput, Scene, Audio, Graphics }

public abstract class GameEvent
{
    public GameEventTypes EventType;
    public string id;
}



public class GlobalEventController : MonoBehaviour
{
    [Serializable]
    public class Listener
    {
        public int ObjectId;
        public ListenerCallback Callback;

        public Listener(int id, ListenerCallback c)
        {
            ObjectId = id;
            Callback = c;
        }
    }

    public delegate void ListenerCallback(GameEvent e);
    private static GlobalEventController s_Instance;
    private GlobalEventController() { }
    public Dictionary<Type, List<Listener>> EventList;
    
    void Start()
    {
        print("Event controller is started");
        EventList = new Dictionary<Type, List<Listener>>();
    }

    void Awake()
    {
        print("Event controller is awake");
        DontDestroyOnLoad(this);
        s_Instance = this;
    }

    public static GlobalEventController GetInstance()
    {
        if (s_Instance)
            return s_Instance;
        else
            return null;
    }

    public void QueueListener(Type t, Listener l)
    {
        //print("Registering events for object " + l.ObjectId + " and type " + t.ToString());
        if(!EventList.ContainsKey(t)) {
            //print("Registering new Event type " + t.ToString());
            EventList.Add(t, new List<Listener>());
        }

        EventList[t].Add(l);
    }

    public void BroadcastEvent(Type t, GameEvent e)
    {
        
        Listener[] listeners = FindListenerByType(t).ToArray();
        //print("Broadcasting event " + t.ToString() + " to " + listeners.Length + " listeners.");
        foreach (Listener l in listeners) {
            //print("BROADCASTING TO LISTENER" + l.ObjectId.ToString());
            l.Callback(e);
        }
    }

    public bool RemoveListener(Type t, ListenerCallback listener)
    {
        Listener l = EventList[t].Find(lVal => lVal.Callback == listener);

        return EventList[t].Remove(l);
    }

    public bool RemoveListener(Type t, int id)
    {
        Listener l = EventList[t].Find(lVal => lVal.ObjectId == id);

        return EventList[t].Remove(l);
    }

    public List<Listener> FindListenerByType(Type t)
    {
        if (!EventList.ContainsKey(t)) {
            lock(EventList) {
                EventList.Add(t, new List<Listener>());
            }
        }
            

        return EventList[t];
    }

    // Update is called once per frame
    void Update()
    {
        //print("Currently has " + EventList.Count + " event entries");
    }
}