using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
//#endif

public class GlobalVibrationManager : EventDrivenBehavior
{
    /**
     * Works for both vibration and rumble
     */
    struct ShakeEvent
    {
        public int id;
        public float intensity;

        public ShakeEvent(int eventId, float intensity) : this()
        {
            this.id = eventId;
            this.intensity = intensity;
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

    protected override void Start()
    {
        base.Start();

        VibrationEvents = new List<ShakeEvent>();
        RumbleEvents = new List<ShakeEvent>();
        AvailableIds = new Queue<int>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Vibration Events with id " + GetInstanceID());
        
        eventCtrl.SubscribeEvent(typeof(VibrationEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationEventCallback));
        eventCtrl.SubscribeEvent(typeof(VibrationOverEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationOverEventCallback));
        eventCtrl.SubscribeEvent(typeof(RumbleEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleEventCallback));
        eventCtrl.SubscribeEvent(typeof(RumbleOverEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleOverEventCallback));
        eventCtrl.SubscribeEvent(typeof(ToggleShakeEvent), new GlobalEventController.Listener(GetInstanceID(), ToggleShakeEventCallback));
    }

    protected override void UnsubEvents()
    {
        base.UnsubEvents();

        print("Destroying Vibration Events with id " + GetInstanceID());

        eventCtrl.RemoveListener(typeof(VibrationEvent), VibrationEventCallback);
        eventCtrl.RemoveListener(typeof(VibrationOverEvent), VibrationOverEventCallback);
        eventCtrl.RemoveListener(typeof(RumbleEvent), RumbleEventCallback);
        eventCtrl.RemoveListener(typeof(RumbleOverEvent), RumbleOverEventCallback);
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

        if(VibrationEvents.Count > 0) {
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

    public void VibrationEventCallback(GameEvent e)
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
    }

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
}
