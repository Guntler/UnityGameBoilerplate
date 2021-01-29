using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySfxEvent : GameEvent
{
    public string sfxSettingName;
}

public class GlobalAudioController : EventDrivenBehavior
{
    public delegate void AudioClipInfoCallback(int soundId);

    public int GlobalMusicSrcs = 2;
    public int GlobalVoiceSrcs = 2;
    public int MaxGlobalSfxSrcs = 10;

    Queue<AudioSource> sfxSrcQueue;
    List<SfxInstance> sfxInstanceList;
    

    Queue<int> clipIdPool;
    int currentId = 0;

    struct SfxInstance
    {
        public AudioSource sfxSrc;
        public AudioSettings sfxSetting;
        public bool isPaused;
    }

    Dictionary<int, AudioSource> clipToSourceDict;
    Dictionary<AudioSource, int> sourceToClipDict;

    AudioSource masterSrc;

    protected override void Start()
    {
        base.Start();

        sfxSrcQueue = new Queue<AudioSource>();
        sfxInstanceList = new List<SfxInstance>();
        clipIdPool = new Queue<int>();

        clipToSourceDict = new Dictionary<int, AudioSource>();
        sourceToClipDict = new Dictionary<AudioSource, int>();

        GameObject masterSrcObj = new GameObject("MasterAudioSourceObject");
        masterSrcObj.transform.parent = transform;

        masterSrc = masterSrcObj.AddComponent<AudioSource>();

        DontDestroyOnLoad(this);
        
        for(int i=0; i<GlobalMusicSrcs; i++)
        {
            sfxSrcQueue.Enqueue(masterSrcObj.AddComponent<AudioSource>());
        }
    }

    protected override void InitEvents()
    {
        base.InitEvents();

        print("Setting up Audio Events with id " + GetInstanceID());

        
        eventCtrl.SubscribeEvent(typeof(PlayAudioClip), new GlobalEventController.Listener(GetInstanceID(), PlayClipCallback));
        eventCtrl.SubscribeEvent(typeof(FadeAudioEvent), new GlobalEventController.Listener(GetInstanceID(), FadeAudioVolumeCallback));
        eventCtrl.SubscribeEvent(typeof(PlayOneshotClipEvent), new GlobalEventController.Listener(GetInstanceID(), PlayOneshotClipCallback));
        eventCtrl.SubscribeEvent(typeof(PlayBackgroundClip), new GlobalEventController.Listener(GetInstanceID(), PlayBackgroundClipCallback));
        eventCtrl.SubscribeEvent(typeof(StopAudioClip), new GlobalEventController.Listener(GetInstanceID(), StopClipCallback));
        
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

        //Check if the current background clip is still playing
        if(!masterSrc.isPlaying && sourceToClipDict.ContainsKey(masterSrc))
        {
            int obsoleteId = sourceToClipDict[masterSrc];
            sourceToClipDict.Remove(masterSrc);
            clipToSourceDict.Remove(obsoleteId);

            clipIdPool.Enqueue(obsoleteId);
        }
    }

    void StopClipCallback(GameEvent e)
    {
        StopAudioClip ev = (StopAudioClip)e;
        if(clipToSourceDict.ContainsKey(ev.ClipId))
        {
            AudioSource src = clipToSourceDict[ev.ClipId];
            //TODO: specify fade out
            src.Stop();

            clipToSourceDict.Remove(ev.ClipId);
            sourceToClipDict.Remove(src);
            
        }
    }

    void PlayClipCallback(GameEvent e)
    {
        PlayAudioClip ev = (PlayAudioClip)e;
        masterSrc.volume = 0;
        masterSrc.pitch = ev.AudioObject.DefaultPitch;
        masterSrc.clip = ev.AudioObject.Clip;
        masterSrc.loop = ev.Loop;
        

        eventCtrl.BroadcastEvent(typeof(FadeAudioEvent), new FadeAudioEvent(masterSrc, ev.AudioObject.DefaultVolume, ev.FadeRate, ev.FadeDelay));
        masterSrc.Play();

        if(sourceToClipDict.ContainsKey(masterSrc))
        {
            clipToSourceDict.Remove(sourceToClipDict[masterSrc]);
            sourceToClipDict.Remove(masterSrc);
        }

        int newId = 0;
        if (clipIdPool.Count <= 0)
        {
            newId = currentId++;
        }
        else
        {
            newId = clipIdPool.Dequeue();
        }

        clipToSourceDict.Add(newId, masterSrc);
        sourceToClipDict.Add(masterSrc, newId);

        ev.InfoCallback?.Invoke(newId);
    }

    void PlaySfxCallback(GameEvent e)
    {

    }

    //TODO: add function to control overlaps
    public void PlayOneshotClipCallback(GameEvent e)
    {
        PlayOneshotClipEvent ev = (PlayOneshotClipEvent)e;
        AudioSource src = sfxSrcQueue.Peek();
        src.PlayOneShot(ev.AudioObject.Clip, ev.Volume == -1 ? ev.AudioObject.DefaultVolume : ev.Volume);
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
