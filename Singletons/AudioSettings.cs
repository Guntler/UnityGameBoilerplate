using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AudioSettings : ScriptableObject
{
    public AudioClip Clip;
    public float DefaultVolume = 1;
    public float DefaultPitch = 1;
}
