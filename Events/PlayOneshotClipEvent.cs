using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOneshotClipEvent : PlayAudioClip
{
    public PlayOneshotClipEvent(AudioSettings clip, GlobalAudioController.AudioClipInfoCallback call, float volume = -1.0f, float rate = 2, float delay = 0.05f, bool loop = false) : base(clip, call, volume, rate, delay, loop)
    {
    }
}
