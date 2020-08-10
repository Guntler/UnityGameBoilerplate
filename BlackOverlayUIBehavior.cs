using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackOverlayUIBehavior : EventDrivenBehavior
{
    public GlobalEventController.ListenerCallback UpdateBlackOverlayCallback;
    public bool IsShown = false;
    public bool IsProcessing = false;

    Image blackOverlayImg;
    Color targetColor;

    private void Awake()
    {
        UpdateBlackOverlayCallback = UpdateBlackOverlayListener;
    }

    protected override void Start()
    {
        base.Start();

        blackOverlayImg = GetComponent<Image>();
    }

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up BlackOverlay Events with id " + GetInstanceID());

        eventCtrl.SubscribeEvent(typeof(UpdateBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), UpdateBlackOverlayCallback));
        eventCtrl.SubscribeEvent(typeof(ShowBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), ShowBlackOverlay));
        eventCtrl.SubscribeEvent(typeof(HideBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), HideBlackOverlay));
    }

    // Update is called once per frame
    void Update()
    {
        if (IsProcessing) {
            if (blackOverlayImg.color == targetColor) {
                IsProcessing = false;
                eventCtrl.BroadcastEvent(typeof(TransitionOverBlackOverlayEvent), new TransitionOverBlackOverlayEvent());
            }
        }
    }

    public void UpdateBlackOverlayListener(GameEvent e)
    {
        UpdateBlackOverlayEvent uEvent = (UpdateBlackOverlayEvent)e;
        if(uEvent.IsShowOverlay) {
            ShowBlackOverlay(e);
        }
        else {
            HideBlackOverlay(e);
        }
    }

    public void ShowBlackOverlay(GameEvent e)
    {
        IsProcessing = true;
        targetColor = Color.black;
        StartCoroutine(Utilities.LerpColor(blackOverlayImg, targetColor, 4f, 0.005f));
    }

    public void HideBlackOverlay(GameEvent e)
    {
        IsProcessing = true;
        targetColor = new Color(0, 0, 0, 0);
        StartCoroutine(Utilities.LerpColor(blackOverlayImg, targetColor, 4f, 0.005f));
    }
    
}
