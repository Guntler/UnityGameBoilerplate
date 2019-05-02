using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOneshotClipEvent : GameEvent
{
    public AudioSettings AudioObject;

    public PlayOneshotClipEvent(AudioSettings a) : base()
    {
        AudioObject = a;
    }
}
