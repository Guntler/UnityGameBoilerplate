using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioClip : GameEvent
{
    public AudioSettings AudioObject;
    public float FadeRate = 2;
    public float FadeDelay = 0.005f;
    public bool Loop = false;
    public float Volume = -1;
    public GlobalAudioController.AudioClipInfoCallback InfoCallback;

    public PlayAudioClip(AudioSettings clip, GlobalAudioController.AudioClipInfoCallback call, float volume = -1.0f, float rate = 2, float delay = 0.05f, bool loop = false)
    {
        AudioObject = clip;
        InfoCallback = call;
        FadeRate = rate;
        FadeDelay = delay;
        Volume = volume;
        Loop = loop;
    }
}

public class StopAudioClip : GameEvent
{
    public int ClipId;

    public StopAudioClip(int id)
    {
        ClipId = id;
    }
}

/**
 * Clips requested via this event should be stored in the Resources folder or in a media manager
 * Requests with a delay of 0 are considered to not have a fade.
 */
public class PlayBackgroundClip : PlayAudioClip
{
    public PlayBackgroundClip(AudioSettings clip, GlobalAudioController.AudioClipInfoCallback call, float volume = -1.0f, float rate = 2, float delay = 0.05f, bool loop = false) : base(clip, call, volume, rate, delay, loop)
    {

    }
}
