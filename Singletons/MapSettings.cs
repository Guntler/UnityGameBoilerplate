using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * TODO: In the future, this can include a list of scenes for level streaming
 */
[CreateAssetMenu]
public class MapSettings : ScriptableObject
{
    public string MapName;
    public string SceneName;
    public AudioSettings BackgroundMusic;

}
