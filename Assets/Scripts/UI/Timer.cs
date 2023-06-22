using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Component")]
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    public float currentTime;
    public bool countDown;
    public bool isActive;

    [Header("Limit Settings")]
    public bool hasLimit;
    public float timerLimit;

    [Header("Format Settings")]
    public bool hasFormat;
    public TimerFormats format;
    private Dictionary<TimerFormats, string> timeFormats = new Dictionary<TimerFormats, string>();

    // Start is called before the first frame update
    void Start()
    {
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.Tenth, "0.0");
        timeFormats.Add(TimerFormats.Hundredth, "0.00");
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            currentTime = countDown ? currentTime -= Time.deltaTime : currentTime += Time.deltaTime;

            if (hasLimit && ((countDown && currentTime <= timerLimit) || (!countDown &&  currentTime >= timerLimit)))
            {
                currentTime = timerLimit;
                //if you wanted to change the color when it hits a limit, can do timerText.color = Color.red;
                enabled = false;
            }

            SetTimerText();       
        }      
    }

    private void SetTimerText()
    {
        timerText.text = hasFormat ? currentTime.ToString(timeFormats[format]) : currentTime.ToString();
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        SetTimerText();
        isActive = false;
    }

    public void StartTimer()
    {
        isActive = true;
    }
    
    public void StopTimer()
    {
        SetTimerText();
        isActive = false;        
    }

    public enum TimerFormats
    {
        Whole,
        Tenth,
        Hundredth
    }
}
