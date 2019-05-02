using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * If no Audio Source is specified, the master audio source component for the scene is used instead.
 */
public class FadeAudioEvent : GameEvent
{
    public AudioSource AudioSrc;
    public float TargetVolume = 0;
    public float FadeRate = 2;
    public float FadeDelay = 0.05f;

    public FadeAudioEvent(AudioSource src, float volume, float rate = 2, float delay = 0.05f)
    {
        TargetVolume = volume;
        FadeRate = rate;
        FadeDelay = delay;
        AudioSrc = src;
    }
}

public class FadeAudioCompleteEvent : GameEvent
{

}
