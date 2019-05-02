using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Intermediary event used by the Scene Manager to effectively switch Unity scenes.
 */
public class ChangeUnitySceneEvent : GameEvent
{
    public string NewSceneSettingsName;
    public LoadSceneMode Mode;
    public bool DoUnloadPrevScene = true;

    public ChangeUnitySceneEvent(string newScene, LoadSceneMode m, bool unloadPrevScene = true) : base()
    {
        Debug.Log("Creating a new scene change " + unloadPrevScene);
        NewSceneSettingsName = newScene;
        Mode = m;
        DoUnloadPrevScene = unloadPrevScene;
    }
}

/**
 * Event to trigger a Scene change in the Scene Manager.
 */ 
public class ChangeSceneEvent : ChangeUnitySceneEvent
{
    public ChangeSceneEvent(string newScene, LoadSceneMode m, bool unloadPrevScene = true) : base(newScene, m, unloadPrevScene)
    {
        NewSceneSettingsName = newScene;
        Mode = m;
        DoUnloadPrevScene = unloadPrevScene;
    }
}

public class ChangeUnitySceneCompleteEvent : GameEvent
{
}