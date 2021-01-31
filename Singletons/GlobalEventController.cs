using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventTypes { PlayerInput, Scene, Audio, Graphics }

public abstract class GameEvent
{
    public GameEventTypes EventType;
    public string id;
    public float EventDeferralDelay = 0;
}

public struct DeferredEvent
{
    public GameEvent Event;
    public float Delay;
    public System.Type Type;
}


//TODO: Add thread-driven events, which don't affect unity API stuff
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

    public delegate void SignalCallback();
    public delegate void ListenerCallback(GameEvent e);
    private static GlobalEventController s_Instance;
    private GlobalEventController() { }
    public Dictionary<Type, List<Listener>> EventList;
    public Dictionary<string, List<SignalCallback>> SignalList;
    public List<DeferredEvent> DeferredEvents = new List<DeferredEvent>();

    void Start()
    {
        print("Event controller is started");
        EventList = new Dictionary<Type, List<Listener>>();
    }

    void Awake()
    {
        print("Event controller is awake");
        //DontDestroyOnLoad(this);
        s_Instance = this;
    }

    public static GlobalEventController GetInstance()
    {
        if (s_Instance)
            return s_Instance;
        else
            return null;
    }

    public void SubscribeSignal(string s, SignalCallback c)
    {
        if (!SignalList.ContainsKey(s))
        {
            SignalList.Add(s, new List<SignalCallback>());
        }

        SignalList[s].Add(c);
    }

    public void SubscribeEvent(Type t, Listener l)
    {
        //print("Registering events for object " + l.ObjectId + " and type " + t.ToString());
        if(!EventList.ContainsKey(t)) {
            //print("Registering new Event type " + t.ToString());
            EventList.Add(t, new List<Listener>());
        }

        EventList[t].Add(l);
    }

    public bool BroadcastEvent(Type t, GameEvent e)
    {
        Listener[] listeners = FindListenerByType(t).ToArray();
        if (listeners.Length > 0)
        {
            foreach (Listener l in listeners)
            {
                //print("BROADCASTING TO LISTENER" + l.ObjectId.ToString());
                l.Callback(e);
            }
            return true;
        }
        else
        {
            if(e.EventDeferralDelay > 0 && !DeferredEvents.Exists(d => d.Type == t))
            {
                //print("Deferring an event of type: " + t.ToString() + " - " + e.ToString());
                DeferredEvent dE = new DeferredEvent();
                dE.Event = e;
                dE.Type = t;
                dE.Delay = 0;
                DeferredEvents.Add(dE);
            }
        }

        return false;
        //print("Broadcasting event " + t.ToString() + " to " + listeners.Length + " listeners.");
    }

    public bool BroadcastSignal(string s)
    {
        SignalCallback[] listeners = FindCallbacksBySignal(s).ToArray();
        if (listeners.Length > 0)
        {
            foreach (SignalCallback l in listeners)
            {
                l();
            }
            return true;
        }

        return false;
        //print("Broadcasting signal " + t.ToString() + " to " + listeners.Length + " listeners.");
    }

    void FixedUpdate()
    {
        //print("Deferred Events: " + DeferredEvents.Count);
        for(int i=0; i<DeferredEvents.Count; i++)
        {
            DeferredEvent dE = DeferredEvents[i];
            if(BroadcastEvent(dE.Type, dE.Event)) {
                DeferredEvents.Remove(dE);
            }
            else if(dE.Delay < dE.Event.EventDeferralDelay)
            {
                dE.Delay += Time.fixedDeltaTime;

                if (dE.Delay >= dE.Event.EventDeferralDelay)
                {
                    //print("Discarding a deferred event for passing limit: " + dE.Type.ToString());
                    DeferredEvents.Remove(dE);
                }
            }
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

    class TypeIdStruct
    {
        public int id;
        public Type type;
        public TypeIdStruct(Type t, int i)
        {
            type = t;
            id = i;
        }
    }

    /*public bool RemoveListener(int id)
    {
        Type[] types = EventList.Keys.ToArray();
        List<Listener>[] lists = EventList.Values.ToArray();
        List<TypeIdStruct> listsToRemove = new List<TypeIdStruct>();

        foreach(List<Listener> ls in lists)
        {
            foreach(Listener l in ls)
            {
                if(l.ObjectId == id)
                {
                    listsToRemove.Add(new TypeIdStruct());
                }
            }
        }
    }*/

    public List<SignalCallback> FindCallbacksBySignal(string s)
    {
        if (!SignalList.ContainsKey(s))
        {
            lock (EventList)
            {
                SignalList.Add(s, new List<SignalCallback>());
            }
        }

        return SignalList[s];
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