using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumbleEvent : GameEvent
{
    public float Intensity;
    public float Duration;

    public RumbleEvent(float intensity, float duration)
    {
        Intensity = intensity;
        Duration = duration;
    }
}

public class RumbleOverEvent : GameEvent
{
    public int RumbleId;

    public RumbleOverEvent(int rumbleId)
    {
        RumbleId = rumbleId;
    }
}
