using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
//#endif

public class GlobalVibrationManager : EventDrivenBehavior
{
    /**
     * Works for both vibration and rumble
     */
    class ShakeEvent
    {
        public string id;
        public float intensity;
        public float duration;
        public float elapsed;
        public ShakeEvent(string eventId, float intensity, float duration)
        {
            this.id = eventId;
            this.intensity = intensity;
            this.duration = duration;
        }
    }

    List<ShakeEvent> VibrationEvents;
    List<ShakeEvent> RumbleEvents;
    Queue<int> AvailableIds;
    int latestId = 0;
    
    int playerIdx = 0;
    GamePadState state;
    
    public bool AreEventsPaused = false;
    public bool IsShakeEnabled = true;

    private static GlobalVibrationManager s_Instance;
    protected GlobalVibrationManager() { }

    protected override void Start()
    {
        base.Start();

        VibrationEvents = new List<ShakeEvent>();
        RumbleEvents = new List<ShakeEvent>();
        AvailableIds = new Queue<int>();
    }

    public static GlobalVibrationManager GetInstance()
    {
        if (s_Instance)
            return s_Instance;
        else
            return null;
    }

    void Awake()
    {
        print("Vigration Controller instance is awake");
        s_Instance = this;
    }

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Vibration Events with id " + GetInstanceID());
        
        /*eventCtrl.SubscribeEvent(typeof(VibrationEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationEventCallback));
        eventCtrl.SubscribeEvent(typeof(VibrationOverEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationOverEventCallback));
        eventCtrl.SubscribeEvent(typeof(RumbleEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleEventCallback));
        eventCtrl.SubscribeEvent(typeof(RumbleOverEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleOverEventCallback));*/
        eventCtrl.SubscribeEvent(typeof(ToggleShakeEvent), new GlobalEventController.Listener(GetInstanceID(), ToggleShakeEventCallback));
    }

    protected override void UnsubEvents()
    {
        base.UnsubEvents();

        print("Destroying Vibration Events with id " + GetInstanceID());

        /*eventCtrl.RemoveListener(typeof(VibrationEvent), VibrationEventCallback);
        eventCtrl.RemoveListener(typeof(VibrationOverEvent), VibrationOverEventCallback);
        eventCtrl.RemoveListener(typeof(RumbleEvent), RumbleEventCallback);
        eventCtrl.RemoveListener(typeof(RumbleOverEvent), RumbleOverEventCallback);*/
        eventCtrl.RemoveListener(typeof(ToggleShakeEvent), ToggleShakeEventCallback);
    }

    void Update()
    {
        if(AreEventsPaused || !IsShakeEnabled)
        {
            return;
        }

        state = GamePad.GetState((PlayerIndex)playerIdx);

        float curRumble = 0;
        float curVibration = 0;

        for (int i = 0; i < VibrationEvents.Count; i++)
        {
            VibrationEvents[i].elapsed += Time.deltaTime;
            if (VibrationEvents[i].duration != -1 && VibrationEvents[i].elapsed >= VibrationEvents[i].duration)
            {
                VibrationEvents.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < RumbleEvents.Count; i++)
        {
            RumbleEvents[i].elapsed += Time.deltaTime;
            if (RumbleEvents[i].duration != -1 && RumbleEvents[i].elapsed >= RumbleEvents[i].duration)
            {
                RumbleEvents.RemoveAt(i);
                i--;
            }
        }

        if (VibrationEvents.Count > 0) {
            curVibration = VibrationEvents[0].intensity;
        }

        if (RumbleEvents.Count > 0) {
            curRumble = RumbleEvents[0].intensity;
        }
        //print("SETTING SHAKE: " + curVibration + " -- " + curRumble);
        GamePad.SetVibration((PlayerIndex)playerIdx, curRumble, curVibration);
    }

    public int GetShakeId()
    {
        int id;
        if (AvailableIds.Count == 0) {
            id = latestId;
            latestId++;
        }
        else {
            id = AvailableIds.Dequeue();
        }

        return id;
    }

    public void ToggleShakeEventCallback(GameEvent e)
    {
        ToggleShakeEvent ev = (ToggleShakeEvent)e;

        IsShakeEnabled = ev.NewState;
    }

    /*public void VibrationEventCallback(GameEvent e)
    {
        VibrationEvent ev = (VibrationEvent)e;
        ShakeEvent[] evArr = VibrationEvents.ToArray();

        int idxToInsert = 0;

        for(int i=0; i<evArr.Length; i++) {
            ShakeEvent evI = evArr[i];

            if (evI.intensity < ev.Intensity) {
                idxToInsert = i - 1;
            }
        }

        if(idxToInsert < 0) {
            idxToInsert = 0;
        }

        int id = GetShakeId();
        //print("QUEUEING VIBRATION EVENT " + ev.Intensity);
        VibrationEvents.Insert(idxToInsert, new ShakeEvent(id, ev.Intensity));

        eventCtrl.BroadcastEvent(typeof(StartTimerEvent), new StartTimerEvent("vibrationEvent_" + id, ev.Duration, () => {
            eventCtrl.BroadcastEvent(typeof(VibrationOverEvent), new VibrationOverEvent(id));
        }));
    }

    public void VibrationOverEventCallback(GameEvent e)
    {
        VibrationOverEvent ev = (VibrationOverEvent)e;
        ShakeEvent sEv = VibrationEvents.Find(vEv => vEv.id == ev.VibrationId);

        if (VibrationEvents.Remove(sEv)) {
            AvailableIds.Enqueue(sEv.id);
        }
    }

    public void RumbleEventCallback(GameEvent e)
    {
        RumbleEvent ev = (RumbleEvent)e;
        ShakeEvent[] evArr = RumbleEvents.ToArray();

        int idxToInsert = 0;

        for (int i = 0; i < evArr.Length; i++) {
            ShakeEvent evI = evArr[i];

            if (evI.intensity < ev.Intensity) {
                idxToInsert = i - 1;
            }
        }

        if (idxToInsert < 0) {
            idxToInsert = 0;
        }

        int id = GetShakeId();

        RumbleEvents.Insert(idxToInsert, new ShakeEvent(id, ev.Intensity));

        eventCtrl.BroadcastEvent(typeof(StartTimerEvent), new StartTimerEvent("rumbleEvent_" + id, ev.Duration, () => {
            eventCtrl.BroadcastEvent(typeof(RumbleOverEvent), new RumbleOverEvent(id));
        }));
    }

    public void RumbleOverEventCallback(GameEvent e)
    {
        RumbleOverEvent ev = (RumbleOverEvent)e;
        ShakeEvent sEv = RumbleEvents.Find(rEv => rEv.id == ev.RumbleId);

        if (RumbleEvents.Remove(sEv)) {
            AvailableIds.Enqueue(sEv.id);
        }
    }*/

    private void OnApplicationFocus(bool focus)
    {
        AreEventsPaused = false;
    }

    private void OnApplicationPause(bool pause)
    {
        AreEventsPaused = true;
        GamePad.SetVibration((PlayerIndex)playerIdx, 0, 0);
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        VibrationEvents.Clear();
        RumbleEvents.Clear();
        GamePad.SetVibration((PlayerIndex)playerIdx, 0, 0);
    }

    public void AddVibrationEvent(float intensity, float duration, string eventId = "")
    {
        VibrationEvents.Add(new ShakeEvent(eventId, intensity, duration));

        VibrationEvents.OrderByDescending(e => e.intensity);
    }

    public void AddRumbleEvent(float intensity, float duration, string eventId = "")
    {
        RumbleEvents.Add(new ShakeEvent(eventId, intensity, duration));

        RumbleEvents.OrderByDescending(e => e.intensity);
    }

    public void RemoveVibrationEvent(string id)
    {
        ShakeEvent ev;
        if ((ev = VibrationEvents.Find(e => e.id == id)) != null)
        {
            VibrationEvents.Remove(ev);
        }
    }

    public void RemoveRumbleEvent(string id)
    {
        ShakeEvent ev;
        if((ev = RumbleEvents.Find(e => e.id == id)) != null)
        {
            RumbleEvents.Remove(ev);
        }
    }
}
