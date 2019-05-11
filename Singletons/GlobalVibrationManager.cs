using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
//#endif

public class GlobalVibrationManager : MonoBehaviour
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

    GlobalEventController eventCtrl;
    int playerIdx = 0;
    GamePadState state;

    public bool IsEventReady = false;

    // Start is called before the first frame update
    void Start()
    {
        VibrationEvents = new List<ShakeEvent>();
        RumbleEvents = new List<ShakeEvent>();
        AvailableIds = new Queue<int>();

        eventCtrl = GlobalEventController.GetInstance();
        
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void SetupEvents()
    {
        print("Setting up Vibration Events with id " + GetInstanceID());

        IsEventReady = true;
        eventCtrl.QueueListener(typeof(VibrationEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationEventCallback));
        eventCtrl.QueueListener(typeof(VibrationOverEvent), new GlobalEventController.Listener(GetInstanceID(), VibrationOverEventCallback));
        eventCtrl.QueueListener(typeof(RumbleEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleEventCallback));
        eventCtrl.QueueListener(typeof(RumbleOverEvent), new GlobalEventController.Listener(GetInstanceID(), RumbleOverEventCallback));
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsEventReady) {
            SetupEvents();
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
}
