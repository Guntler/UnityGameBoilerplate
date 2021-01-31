using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TimerCallback();
public delegate void TimerInfoCallback(Timer timer);

public class Timer
{
    public TimerCallback Callback;
    public string Id = "";
    public float Time = 0;
    public float ElapsedTime = 0;
    public bool IsPaused = false;
    public bool IsDone = false;
    public bool DequeueAtEnd = false;
    public bool IsBroadcast = false;
    public bool IsProgramPaused = false;

    public TimerInfoCallback OnStart, OnTick, OnPause;

    public Timer(string id, float time, bool dequeue, TimerCallback callback, TimerInfoCallback start =null, TimerInfoCallback tick = null, TimerInfoCallback pause = null)
    {
        Id = id;
        Time = time;
        DequeueAtEnd = dequeue;

        Callback = callback;
        OnStart = start;
        OnTick = tick;
        OnPause = pause;
}

    public void Tick(float time)
    {
        if (IsProgramPaused)
        {
            return;
        }
            
        if(!IsDone && !IsPaused) {
            //Debug.Log(ElapsedTime + " -- " + Time + " -- " + Callback);
            ElapsedTime += time;
            IsDone = ElapsedTime >= Time;

            if (IsDone) {
                ElapsedTime = Time;
                //Debug.Log("CALLING CALLBACK");
                Callback?.Invoke();
            }
            else
            {
                OnTick?.Invoke(this);
            }
        }
        else {
        }
    }

    public float TimeRemaining()
    {
        return Time - ElapsedTime;
    }

    public void Restart()
    {
        ElapsedTime = 0;
        IsDone = false;
    }

    public void ToggleProgramPause(bool state)
    {
        OnPause?.Invoke(this);
        IsProgramPaused = state;
    }

    public void Stop()
    {
        Callback = null;
        ElapsedTime = Time;
        IsDone = true;
    }
}

public class TimerManager : EventDrivenBehavior
{
    /*private static TimerManager s_Instance;
    private TimerManager() { }*/

    public List<Timer> Timers = new List<Timer>();
    

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Timer Events with id " + GetInstanceID());

        eventCtrl.SubscribeEvent(typeof(StartTimerEvent), new GlobalEventController.Listener(GetInstanceID(), StartTimerCallback));
        eventCtrl.SubscribeEvent(typeof(TimerOverEvent), new GlobalEventController.Listener(GetInstanceID(), TimerOverCallback));
        eventCtrl.SubscribeEvent(typeof(PauseTimerEvent), new GlobalEventController.Listener(GetInstanceID(), PauseTimerCallback));
        eventCtrl.SubscribeEvent(typeof(ResumeTimerEvent), new GlobalEventController.Listener(GetInstanceID(), ResumeTimerCallback));
        eventCtrl.SubscribeEvent(typeof(StopTimerEvent), new GlobalEventController.Listener(GetInstanceID(), StopTimerCallback));
    }

    protected override void UnsubEvents()
    {
        base.UnsubEvents();
        eventCtrl.RemoveListener(typeof(StartTimerEvent), StartTimerCallback);
        eventCtrl.RemoveListener(typeof(TimerOverEvent), TimerOverCallback);
        eventCtrl.RemoveListener(typeof(PauseTimerEvent), PauseTimerCallback);
        eventCtrl.RemoveListener(typeof(ResumeTimerEvent), ResumeTimerCallback);
        eventCtrl.RemoveListener(typeof(StopTimerEvent), StopTimerCallback);
    }

    void Awake()
    {
        //s_Instance = this;
        //DontDestroyOnLoad(this);
    }

    /*public static TimerManager GetInstance()
    {
        if (s_Instance)
            return s_Instance;
        else
            return null;
    }*/

    public void StartTimerCallback(GameEvent e)
    {
        StartTimerEvent ev = (StartTimerEvent)e;
        Timers.Add(new Timer(ev.TimerId, ev.Duration, true, ev.Callback, ev.OnStart, ev.OnTick, ev.OnPause));
    }

    public void PauseTimerCallback(GameEvent e)
    {
        PauseTimerEvent ev = (PauseTimerEvent)e;
        Timer t = Timers.Find(tT => tT.Id == ev.TimerId);
        if(t != null) {
            t.IsPaused = true;
        }
    }

    public void ResumeTimerCallback(GameEvent e)
    {
        ResumeTimerEvent ev = (ResumeTimerEvent)e;
        Timer t = Timers.Find(tT => tT.Id == ev.TimerId);
        if (t != null) {
            t.IsPaused = false;
        }
    }

    public void StopTimerCallback(GameEvent e)
    {
        StopTimerEvent ev = (StopTimerEvent)e;
        Timer t = Timers.Find(tT => tT.Id == ev.TimerId);
        if(t != null)
            t.IsDone = true;
        //eventCtrl.BroadcastEvent(typeof(TimerOverEvent), new TimerOverEvent(ev.TimerId));
    }

    public void TimerOverCallback(GameEvent e)
    {
        TimerOverEvent ev = (TimerOverEvent)e;
        Timer t = Timers.Find(tT => tT.Id == ev.TimerId);

        if(Timers.Remove(t)) {

        }
    }
    
    void Update ()
    {

        Timer[] tArr = Timers.ToArray();

        for (int i = 0; i < Timers.Count; i++) {
            Timer t = Timers[i];

            if(t.IsPaused)
            {
                continue;
            }

            //print("TICKING " + t.Id);
            t.Tick(Time.unscaledDeltaTime);

            if(!t.IsBroadcast && t.IsDone && t.DequeueAtEnd) {
                t.IsBroadcast = true;
                //eventCtrl.BroadcastEvent(typeof(TimerOverEvent), new TimerOverEvent(t.Id));
                Timers.RemoveAt(i);
                i--;
            }
        }
	}

    public void QueueTimer(string id, float time, TimerCallback callback)
    {
        Timers.Add(new Timer(id, time, true, callback));
    }

    public bool HasTimer(string id)
    {
        return Timers.Find(t => t.Id == id) != null;
    }

    public void RemoveTimer(string id)
    {
        Timer l_tToRem = Timers.Find(t => t.Id == id);
        if(l_tToRem != null)
            Timers.Remove(l_tToRem);
    }

    private void OnApplicationFocus(bool focus)
    {
        Timer[] tArr = Timers.ToArray();

        for (int i = 0; i < Timers.Count; i++)
        {
            Timer t = Timers[i];
            t.ToggleProgramPause(false);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        Timer[] tArr = Timers.ToArray();

        for (int i = 0; i < Timers.Count; i++)
        {
            Timer t = Timers[i];
            t.ToggleProgramPause(true);
        }
    }

    protected override void OnApplicationQuit()
    {
        Timer[] tArr = Timers.ToArray();

        for (int i = 0; i < Timers.Count; i++)
        {
            Timer t = Timers[i];
            t.Stop();
        }

        Timers.Clear();
    }
}
