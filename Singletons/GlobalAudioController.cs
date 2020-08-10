using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySfxEvent : GameEvent
{
    public string sfxSettingName;
}

public class GlobalAudioController : EventDrivenBehavior
{
    public int GlobalMusicSrcs = 2;
    public int GlobalVoiceSrcs = 2;
    public int MaxGlobalSfxSrcs = 10;

    Queue<AudioSource> sfxSrcQueue;
    List<SfxInstance> sfxInstanceList;

    struct SfxInstance
    {
        public AudioSource sfxSrc;
        public AudioSettings sfxSetting;
        public bool isPaused;
    }
    
    AudioSource masterSrc;

    protected override void Start()
    {
        base.Start();

        sfxSrcQueue = new Queue<AudioSource>();
        sfxInstanceList = new List<SfxInstance>();

        GameObject masterSrcObj = new GameObject("MasterAudioSourceObject");
        masterSrcObj.transform.parent = transform;

        masterSrc = masterSrcObj.AddComponent<AudioSource>();

        DontDestroyOnLoad(this);
        
        for(int i=0; i<MaxGlobalSfxSrcs; i++)
        {
            sfxSrcQueue.Enqueue(masterSrcObj.AddComponent<AudioSource>());
        }

    }

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Audio Events with id " + GetInstanceID());

        eventCtrl.SubscribeEvent(typeof(FadeAudioEvent), new GlobalEventController.Listener(GetInstanceID(), FadeAudioVolumeCallback));
        eventCtrl.SubscribeEvent(typeof(PlayOneshotClipEvent), new GlobalEventController.Listener(GetInstanceID(), PlayOneshotClipCallback));
        eventCtrl.SubscribeEvent(typeof(PlayBackgroundClip), new GlobalEventController.Listener(GetInstanceID(), PlayBackgroundClipCallback));
    }

    void FixedUpdate()
    {
        for(int i=0; i<sfxInstanceList.Count; i++)
        {
            SfxInstance inst = sfxInstanceList[i];
            if (!inst.sfxSrc.isPlaying && !inst.isPaused)
            {
                sfxInstanceList.Remove(inst);
                i--;

                inst.sfxSrc.clip = null;
                inst.sfxSrc.loop = false;
                inst.sfxSrc.volume = 1;
                sfxSrcQueue.Enqueue(inst.sfxSrc);
            }
        }
    }

    void PlaySfxCallback(GameEvent e)
    {

    }

    public void PlayOneshotClipCallback(GameEvent e)
    {
        PlayOneshotClipEvent ev = (PlayOneshotClipEvent)e;
        masterSrc.PlayOneShot(ev.AudioObject.Clip, ev.AudioObject.DefaultVolume);
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
