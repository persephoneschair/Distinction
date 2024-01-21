using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{    
    private bool questionClockRunning;
    private bool resettingClock;
    private bool roundActive;

    [Header("Scene Objects")]
    public TextMeshPro[] timerMeshes;
    public Animator[] timerWobble;
    public Renderer[] clockBackings;
    public Material[] clockMaterials;

    [Header("Times")]
    [ShowOnly] public float elapsedTime;
    [ShowOnly] public float totalTime;
    public float testTime;

    [Button]
    public void TestTimer()
    {
        StartTheClock(testTime);
    }

    public void StartTheClock(float startTime)
    {
        elapsedTime = startTime;
        totalTime = startTime;
        roundActive = true;
        resettingClock = true;
    }

    private void InvokeClock()
    {
        questionClockRunning = true;
    }

    private void Update()
    {
        if (questionClockRunning)
            QuestionTimer();
        else if (resettingClock)
            ResetTimer();
    }

    void ResetTimer()
    {
        //Increment timer
        int previousTime = (int)elapsedTime;
        elapsedTime -= (20f * Time.deltaTime);

        if ((int)elapsedTime != previousTime)
            AudioManager.Get.Play(AudioManager.OneShotClip.Tick);

        //Set clock backing
        float percentage = (elapsedTime / totalTime) * 100f;
        foreach (Renderer r in clockBackings)
        {
            if (percentage < 50f)
                r.material = clockMaterials[0];
            else if (percentage < 90f)
                r.material = clockMaterials[1];
            else
                r.material = clockMaterials[2];
        }

        //Update meshes
        foreach (TextMeshPro tx in timerMeshes)
            tx.text = (totalTime - elapsedTime).ToString("#0");

        //End reset
        if (elapsedTime <= 0)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.Correct);
            resettingClock = false;
            elapsedTime = 0;
            Invoke("InvokeClock", 1f);
        }
    }

    void QuestionTimer()
    {
        //Increment timer
        int previousTime = (int)elapsedTime;
        float percentage = (elapsedTime / totalTime) * 100f;
        elapsedTime += (1f * Time.deltaTime);

        //Invoke text wobble/tick
        if ((int)elapsedTime != previousTime && percentage < 98f)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.Tick, 0.5f);
            foreach (Animator an in timerWobble)
                an.SetTrigger("tick");
        }
            

        //Set clock backing
        foreach (Renderer r in clockBackings)
        {
            if (percentage < 50f)
                r.material = clockMaterials[0];
            else if (percentage < 90f)
                r.material = clockMaterials[1];
            else
                r.material = clockMaterials[2];
        }            

        //Update meshes
        foreach(TextMeshPro tx in timerMeshes)
            tx.text = (totalTime - elapsedTime).ToString("#0");

        //End timer
        if (elapsedTime > totalTime)
        {
            questionClockRunning = false;
            roundActive = false;
            GameplayManager.Get.OnTimeUp();
        }            
    }

    public int GetIntTimeRemaining()
    {
        return (int)(totalTime - elapsedTime);
    }

    public float GetRawTimestamp()
    {
        return elapsedTime;
    }

    public string GetFormattedTimestamp()
    {
        return elapsedTime.ToString("#0.00");
    }

    public bool RoundActive()
    {
        return roundActive;
    }
}
