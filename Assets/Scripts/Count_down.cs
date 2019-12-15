using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Count down and info text UI script
/// </summary>

public class Count_down : MonoBehaviour
{
    public Text_Shadow timertext;
    bool active, alarm = false;
    public string startText = "Press 'start' to play";
    float timeLeft;

    public bool IsActive { get => active; }

    [Header("Blink Warning Settings:")]
    public float blinkStartTime = 10f;
    public float blinkInterval = .5f;
    public Color blinkColor = Color.red;
    private void Start()
    {
        timertext.SetText = startText;
    }

    public void Set (int seconds)
    {
        if(seconds > 0)
        {
            active = true;
            timeLeft = (float)seconds;
            timertext.SetText = TimeStamp(timeLeft);
        }
    }

    public void Stop ()
    {
        active = false;
        timeLeft = 0;
        timertext.SetText = TimeStamp(timeLeft);
        if (timertext.IsBlinking)
            timertext.StopBlinking();
    }
    
    public bool Alarm
    {
       get
        {
            bool _alarm = alarm;
            alarm = false;
            return _alarm;
        }
    }

    private void Update()
    {
        if(active)
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0f)
            {
                RingAlarm();
            }
            if (Mathf.Ceil(timeLeft) <= blinkStartTime && !timertext.IsBlinking)
            {
                timertext.StartBlinking(blinkColor, blinkInterval);
            }
            timertext.SetText = TimeStamp(timeLeft);
        }
    }

    void RingAlarm ()
    {
        alarm = true;
        active = false;
        timeLeft = 0;
        timertext.SetText = TimeStamp(timeLeft);
    }

    public string TimeStamp (float seconds)
    {
        int min = 0;
        int sec = (int)Mathf.Ceil(seconds);
        if (sec >= 60) {
            while (sec >= 60)
            {
                sec -= 60;
                min++;
            }
        }

        return min.ToString() + ":" +(sec > 9 ? sec.ToString() : "0" + sec.ToString());
    } 
}
