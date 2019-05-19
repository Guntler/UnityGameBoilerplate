using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameInfo : ScriptableObject
{
    public string GameName = "Sector Alpha";
    public string BuildVersion = "Build Version: v0.2-18.5.19-Alpha";
    public DateTime ReleaseDate;
}
