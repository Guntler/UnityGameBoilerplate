using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSceneManager : MonoBehaviour
{
    public MapSettings CurrentMap;
    public bool IsChangingScene = false;
    public bool AutoChangeSceneOnBoot = true;

    GlobalEventController eventCtrl;

    bool isOnBootDone = false;
    public bool IsEventReady = false;
    // Start is called before the first frame update
    void Start()
    {
        eventCtrl = GlobalEventController.GetInstance();

        
    }

    void SetupEvents()
    {
        print("Setting up Scene Events with id " + GetInstanceID());

        IsEventReady = true;
        eventCtrl.QueueListener(typeof(ChangeSceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeScene));
        eventCtrl.QueueListener(typeof(ChangeUnitySceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeUnityScene));
    }

    void Update()
    {
        if (!IsEventReady) {
            SetupEvents();
            return;
        }

        if (!isOnBootDone && AutoChangeSceneOnBoot) {
            print("rebooting-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            //eventCtrl.BroadcastEvent(typeof(PlayBackgroundClip), new PlayBackgroundClip(CurrentMap.BackgroundMusic, 1));
            eventCtrl.BroadcastEvent(typeof(ChangeSceneEvent), new ChangeSceneEvent("TitleScreen", LoadSceneMode.Additive, false));
            isOnBootDone = true;
        }
        else
        {
            isOnBootDone = true;
        }

    }

    public void ChangeScene(GameEvent e)
    {
        //eventCtrl.QueueListener(typeof(ChangeUnitySceneEvent), new GlobalEventController.Listener(GetInstanceID(), ChangeUnityScene));

        string name = ((ChangeSceneEvent)e).NewSceneSettingsName;
        LoadSceneMode mode = ((ChangeSceneEvent)e).Mode;
        bool unloadPrev = ((ChangeSceneEvent)e).DoUnloadPrevScene;
        print("Beginning change scene " + name);
        eventCtrl.BroadcastEvent(typeof(ShowBlackOverlayEvent), new ShowBlackOverlayEvent());
        eventCtrl.QueueListener(typeof(TransitionOverBlackOverlayEvent), new GlobalEventController.Listener(this.GetInstanceID(),
                                                                                                                    (GameEvent ev) => {
                                                                                                                        eventCtrl.BroadcastEvent(typeof(ChangeUnitySceneEvent), new ChangeUnitySceneEvent(name, mode, unloadPrev));
                                                                                                                    }));
    }

    private void ChangeUnityScene(GameEvent e)
    {
        print("Changing unity scene");
        eventCtrl.RemoveListener(typeof(TransitionOverBlackOverlayEvent), GetInstanceID());
        IsChangingScene = true;
        ChangeUnitySceneEvent cEv = (ChangeUnitySceneEvent)e;
        StartCoroutine(ChangeSceneRoutine(cEv.NewSceneSettingsName, cEv.Mode, cEv.DoUnloadPrevScene));
    }

    public IEnumerator ChangeSceneRoutine(string newMapName, LoadSceneMode mode, bool doUnloadPrevScene)
    {
        MapSettings map = Resources.Load<MapSettings>("MapSettings/" + newMapName);
        if(map.BackgroundMusic != null) {
            eventCtrl.BroadcastEvent(typeof(FadeAudioEvent), new FadeAudioEvent(null, 0, 2, 0.05f));
        }

        if (doUnloadPrevScene && mode == LoadSceneMode.Additive) {
            Scene originalScene = SceneManager.GetSceneByName(CurrentMap.SceneName);
            AsyncOperation op2 = SceneManager.UnloadSceneAsync(originalScene);
            yield return new WaitUntil(() => op2.isDone);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(map.SceneName, mode);
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.SceneName));
        


        CurrentMap = map;

        IsChangingScene = false;

        eventCtrl.BroadcastEvent(typeof(ChangeUnitySceneCompleteEvent), new ChangeUnitySceneCompleteEvent());
        eventCtrl.BroadcastEvent(typeof(HideBlackOverlayEvent), new HideBlackOverlayEvent());

        if (map.BackgroundMusic != null) {
            eventCtrl.BroadcastEvent(typeof(PlayBackgroundClip), new PlayBackgroundClip(CurrentMap.BackgroundMusic));
        }
    }
}
