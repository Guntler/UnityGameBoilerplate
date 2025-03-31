using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : EventDrivenBehavior
{
    //TODO: should be kept??
    public GameObject PlayerObject;
    protected float goalTimeScale = 1.0f;
    protected float realTimeScale = 1.0f;

    Stack<float> prevTimeScales;
    Coroutine currentLerpEvent;

    protected override void Start()
    {
        base.Start();
        prevTimeScales = new Stack<float>();
    }

    protected override void InitEvents()
    {
        base.InitEvents();

        eventCtrl.SubscribeEvent(typeof(LerpToTimeScaleEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), LerpToTimeScaleCallback));
        eventCtrl.SubscribeEvent(typeof(StopLerpToTimeScaleEvent), new GlobalEventController.Listener(gameObject.GetInstanceID(), StopLerpToTimeScaleCallback));
    }

    void LerpToTimeScaleCallback(GameEvent e)
    {
        LerpToTimeScaleEvent ev = (LerpToTimeScaleEvent)e;
        if(goalTimeScale >= ev.ScaleGoal)
        {
            goalTimeScale = ev.ScaleGoal;
        }

        if(currentLerpEvent != null)
        {
            StopCoroutine(currentLerpEvent);
        }

        print("Lerping time to " + ev.ScaleGoal);
        currentLerpEvent = StartCoroutine(TimeScaleLerp(ev));
    }

    void StopLerpToTimeScaleCallback(GameEvent e)
    {
        if (currentLerpEvent != null)
        {
            StopCoroutine(currentLerpEvent);
        }

        prevTimeScales.Clear();

        //TODO: add some smoothing?
        goalTimeScale = 1.0f;
        realTimeScale = 1.0f;
        Time.timeScale = realTimeScale;
    }

    IEnumerator TimeScaleLerp(LerpToTimeScaleEvent ev)
    {
        //Store previous Time Scale
        prevTimeScales.Push(realTimeScale);

        //print(realTimeScale + " -|||- " + goalTimeScale);

        //Lerp to new Time Scale
        while (!UtilitiesGame.FloatApprox(realTimeScale, goalTimeScale))
        {
            //print(realTimeScale + " - " + goalTimeScale);
            realTimeScale = Mathf.Lerp(realTimeScale, goalTimeScale, Time.unscaledDeltaTime * ev.Rate);
            Time.timeScale = realTimeScale;
            yield return new WaitForEndOfFrame();
        }

        //Wait for the specified duration
        float elapsedWaitTime = 0.0f;
        while (elapsedWaitTime < ev.DurationInSeconds)
        {
            elapsedWaitTime += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Return to previous Time Scales (all of them)
        float prevTimeScale = 1.0f;
        if(ev.ReturnsToDefault)
        {
            prevTimeScales.Clear();
            prevTimeScales.Push(prevTimeScale);

            while (!UtilitiesGame.FloatApprox(realTimeScale, prevTimeScale))
            {
                
                realTimeScale = Mathf.Lerp(realTimeScale, prevTimeScale, Time.unscaledDeltaTime * ev.RestorationRate);
                Time.timeScale = realTimeScale;
                yield return new WaitForEndOfFrame();
            }

            PopTimeScale();
        }
        else
        {
            while (prevTimeScales.Count > 0)
            {
                if (prevTimeScales.TryPop(out prevTimeScale))
                {
                    while (!UtilitiesGame.FloatApprox(realTimeScale, prevTimeScale))
                    {
                        realTimeScale = Mathf.Lerp(realTimeScale, prevTimeScale, Time.unscaledDeltaTime * ev.RestorationRate);
                        Time.timeScale = realTimeScale;
                        yield return new WaitForEndOfFrame();
                    }

                    PopTimeScale();
                }
            }
        }
        
        
        yield return null;
    }

    void PopTimeScale()
    {
        prevTimeScales.Pop();
    }
}

public class LerpToTimeScaleEvent : GameEvent
{
    public float Rate = 1.0f;
    public float ScaleGoal = 1.0f;
    public float DurationInSeconds = 0.0f;
    public float RestorationRate = 1.0f;
    public bool ReturnsToDefault = true;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lerpId"></param>
    /// <param name="rate"></param>
    /// <param name="goal"></param>
    /// <param name="durationInSeconds">In seconds.</param>
    /// <param name="restorationRate"></param>
    public LerpToTimeScaleEvent(float rate, float goal, float durationInSeconds, float restorationRate, bool returnsToDefault = true)
    {
        Rate = rate;
        ScaleGoal = goal;
        RestorationRate = restorationRate;
        DurationInSeconds = durationInSeconds;
        ReturnsToDefault = returnsToDefault;
    }
}

public class StopLerpToTimeScaleEvent : GameEvent
{

}