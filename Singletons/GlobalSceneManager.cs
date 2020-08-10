using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSceneManager : EventDrivenBehavior
{
    public MapSettings CurrentMap;
    public bool IsChangingScene = false;
    public bool AutoChangeSceneOnBoot = true;
    public string DefaultMap = "TitleScreen";

    bool isOnBootDone = false;

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Scene Events with id " + GetInstanceID());

        eventCtrl.SubscribeEvent(typeof(ChangeSceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeScene));
        eventCtrl.SubscribeEvent(typeof(ChangeUnitySceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeUnityScene));
    }

    protected override void UnsubEvents()
    {
        base.UnsubEvents();

        eventCtrl.RemoveListener(typeof(ChangeSceneEvent), ChangeScene);
        eventCtrl.RemoveListener(typeof(ChangeUnitySceneEvent), ChangeUnityScene);
    }

    void Update()
    {
        if (!isOnBootDone && AutoChangeSceneOnBoot) {
            print("rebooting-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            //eventCtrl.BroadcastEvent(typeof(PlayBackgroundClip), new PlayBackgroundClip(CurrentMap.BackgroundMusic, 1));
            eventCtrl.BroadcastEvent(typeof(ChangeSceneEvent), new ChangeSceneEvent(DefaultMap, LoadSceneMode.Additive, false));
            isOnBootDone = true;
        }
        else
        {
            isOnBootDone = true;
        }

    }

    public void ChangeScene(GameEvent e)
    {
        //eventCtrl.SubscribeEvent(typeof(ChangeUnitySceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeUnityScene));

        string name = ((ChangeSceneEvent)e).NewSceneSettingsName;
        LoadSceneMode mode = ((ChangeSceneEvent)e).Mode;
        bool unloadPrev = ((ChangeSceneEvent)e).DoUnloadPrevScene;
        print("Beginning change scene " + name);
        eventCtrl.BroadcastEvent(typeof(ShowBlackOverlayEvent), new ShowBlackOverlayEvent());
        eventCtrl.SubscribeEvent(typeof(TransitionOverBlackOverlayEvent), new GlobalEventController.Listener(this.GetInstanceID(),
                                                                                                                    (GameEvent ev) => {
                                                                                                                        eventCtrl.BroadcastEvent(typeof(ChangeUnitySceneEvent), new ChangeUnitySceneEvent(name, mode, unloadPrev));
                                                                                                                    }));
    }

    private void ChangeUnityScene(GameEvent e)
    {
        eventCtrl.RemoveListener(typeof(TransitionOverBlackOverlayEvent), GetInstanceID());
        IsChangingScene = true;
        ChangeUnitySceneEvent cEv = (ChangeUnitySceneEvent)e;
        print("Changing unity scene: " + cEv.NewSceneSettingsName);
        StartCoroutine(ChangeSceneRoutine(cEv.NewSceneSettingsName, cEv.Mode, cEv.DoUnloadPrevScene));
    }

    public IEnumerator ChangeSceneRoutine(string newMapName, LoadSceneMode mode, bool doUnloadPrevScene)
    {
        print("Loading settings for: " + newMapName);
        MapSettings map = Resources.Load<MapSettings>("MapSettings/" + newMapName);
        if(map.BackgroundMusic != null) {
            eventCtrl.BroadcastEvent(typeof(FadeAudioEvent), new FadeAudioEvent(null, 0, 2, 0.05f));
        }

        if(CurrentMap && map)
        {
            print("Loaded from: " + CurrentMap.SceneName + " to " + map.SceneName);
        }

        if (doUnloadPrevScene && mode == LoadSceneMode.Additive) {
            Scene originalScene;
            AsyncOperation op2;
            if (CurrentMap)
            {
                originalScene = SceneManager.GetSceneByName(CurrentMap.SceneName);
                op2 = SceneManager.UnloadSceneAsync(originalScene);
                yield return new WaitUntil(() => op2.isDone);
            }
            else
            {
                int openSceneCount = SceneManager.sceneCount;
                for(int i=0; i<openSceneCount; i++)
                {
                    Scene loadedScene = SceneManager.GetSceneAt(i);
                    if (loadedScene.isLoaded)
                    {
                        if(loadedScene.name == "MasterScene")
                        {
                            continue;
                        }
                        else
                        {
                            print("Unloading scene: " + loadedScene.name + " with index " + i);
                            op2 = SceneManager.UnloadSceneAsync(loadedScene);
                            yield return new WaitUntil(() => op2.isDone);
                        }
                    }
                }
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(map.SceneName, mode);
        op.completed += (AsyncOperation o) => { SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.SceneName)); };
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);
        
        CurrentMap = map;

        IsChangingScene = false;

        eventCtrl.BroadcastEvent(typeof(ChangeUnitySceneCompleteEvent), new ChangeUnitySceneCompleteEvent());
        eventCtrl.BroadcastEvent(typeof(HideBlackOverlayEvent), new HideBlackOverlayEvent());

        if (map.BackgroundMusic != null) {
            eventCtrl.BroadcastEvent(typeof(PlayBackgroundClip), new PlayBackgroundClip(CurrentMap.BackgroundMusic));
        }
    }
}
