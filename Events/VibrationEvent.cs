using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationEvent : GameEvent
{
    public float Intensity;
    public float Duration;

    public VibrationEvent(float intensity, float duration)
    {
        Intensity = intensity;
        Duration = duration;
    }
}

public class VibrationOverEvent : GameEvent
{
    public int VibrationId;

    public VibrationOverEvent(int vibrationId)
    {
        VibrationId = vibrationId;
    }
}