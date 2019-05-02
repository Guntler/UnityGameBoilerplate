using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackOverlayUIBehavior : MonoBehaviour
{
    public GlobalEventController.ListenerCallback UpdateBlackOverlayCallback;
    public bool IsShown = false;
    public bool IsProcessing = false;

    Image blackOverlayImg;
    Color targetColor;

    public bool IsEventReady = false;

    private void Awake()
    {
        UpdateBlackOverlayCallback = UpdateBlackOverlayListener;
    }
    // Start is called before the first frame update
    void Start()
    {
        
        blackOverlayImg = GetComponent<Image>();
    }

    void SetupEvents()
    {
        print("Setting up BlackOverlay Events with id " + GetInstanceID());

        IsEventReady = true;
        GlobalEventController.GetInstance().QueueListener(typeof(UpdateBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), UpdateBlackOverlayCallback));
        GlobalEventController.GetInstance().QueueListener(typeof(ShowBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), ShowBlackOverlay));
        GlobalEventController.GetInstance().QueueListener(typeof(HideBlackOverlayEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), HideBlackOverlay));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsEventReady) {
            SetupEvents();
            return;
        }

        if (IsProcessing) {
            if (blackOverlayImg.color == targetColor) {
                IsProcessing = false;
                GlobalEventController.GetInstance().BroadcastEvent(typeof(TransitionOverBlackOverlayEvent), new TransitionOverBlackOverlayEvent());
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
