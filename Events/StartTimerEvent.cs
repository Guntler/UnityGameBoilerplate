using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTimerEvent : GameEvent
{
    public string TimerId;
    public float Duration;
    public TimerCallback Callback;

    public StartTimerEvent(string timerId, float duration, TimerCallback callback)
    {
        TimerId = timerId;
        Duration = duration;
        Callback = callback;
    }
}

public class TimerOverEvent : GameEvent
{
    public string TimerId;

    public TimerOverEvent(string timerId)
    {
        TimerId = timerId;
    }
}

public class ResumeTimerEvent : GameEvent
{
    public string TimerId;

    public ResumeTimerEvent(string timerId)
    {
        TimerId = timerId;
    }
}

public class PauseTimerEvent : GameEvent
{
    public string TimerId;

    public PauseTimerEvent(string timerId)
    {
        TimerId = timerId;
    }
}

public class StopTimerEvent : GameEvent
{
    public string TimerId;

    public StopTimerEvent(string timerId)
    {
        TimerId = timerId;
    }
}