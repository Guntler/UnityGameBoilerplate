using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Clips requested via this event should be stored in the Resources folder or in a media manager
 * Requests with a delay of 0 are considered to not have a fade.
 */
public class PlayBackgroundClip : GameEvent
{
    public AudioSettings AudioObject;
    public float FadeRate = 2;
    public float FadeDelay = 0.005f;

    public PlayBackgroundClip(AudioSettings clip, float rate = 2, float delay = 0.05f)
    {
        AudioObject = clip;
        FadeRate = rate;
        FadeDelay = delay;
    }
}
