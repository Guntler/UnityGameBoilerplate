using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioController : MonoBehaviour
{
    GlobalEventController eventCtrl;
    AudioSource masterSrc;

    public bool IsEventReady = false;

    void Start()
    {
        GameObject masterSrcObj = new GameObject("MasterAudioSourceObject");
        masterSrcObj.transform.parent = transform;

        masterSrc = masterSrcObj.AddComponent<AudioSource>();

        DontDestroyOnLoad(this);

        eventCtrl = GlobalEventController.GetInstance();
        
    }

    void SetupEvents()
    {
        print("Setting up Audio Events with id " + GetInstanceID());

        IsEventReady = true;
        eventCtrl.QueueListener(typeof(FadeAudioEvent), new GlobalEventController.Listener(GetInstanceID(), FadeAudioVolumeCallback));
        eventCtrl.QueueListener(typeof(PlayBackgroundClip), new GlobalEventController.Listener(GetInstanceID(), PlayBackgroundClipCallback));
    }

    void Update()
    {
        if (!IsEventReady) {
            SetupEvents();
            return;
        }
    }

    public void PlayBackgroundClipCallback(GameEvent e)
    {
        
        PlayBackgroundClip ev = (PlayBackgroundClip)e;
        masterSrc.volume = 0;
        masterSrc.pitch = ev.AudioObject.DefaultPitch;
        masterSrc.clip = ev.AudioObject.Clip;

        eventCtrl.BroadcastEvent(typeof(FadeAudioEvent), new FadeAudioEvent(null, ev.AudioObject.DefaultVolume, ev.FadeRate, ev.FadeDelay));
        masterSrc.Play();
    }

    public void FadeAudioVolumeCallback(GameEvent e)
    {
        FadeAudioEvent ev = (FadeAudioEvent)e;
        StopCoroutine("FadeAudioVolume");
        StartCoroutine(FadeAudioVolume(ev.AudioSrc != null ? ev.AudioSrc : masterSrc, ev.TargetVolume, ev.FadeRate, ev.FadeDelay));
    }

    public IEnumerator FadeAudioVolume(AudioSource src, float target, float rate, float delay)
    {
        while (!Utilities.FloatApprox(src.volume, target, 0.1f)) {
            if (!src)
                break;
            src.volume = Mathf.Lerp(src.volume, target, rate * Time.deltaTime);
            yield return new WaitForSeconds(delay);
        }

        if (src) {
            src.volume = target;
        }

        eventCtrl.BroadcastEvent(typeof(FadeAudioCompleteEvent), new FadeAudioCompleteEvent());

        yield return null;
    }
}
