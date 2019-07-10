using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    // public static float playbackSpeed = 0.1f;
    Slider main;
    public Button playButton;
    bool isPlaying = false;
    public Sprite play;
    public Sprite pause;
    public Toggle lockToggle;
    public RangeSlider timeVl;
    public bool IsPlaying
        {
        get { return isPlaying;  }
        }
    public UnityEvent OnStartPlaying;
    public UnityEvent OnStopPlaying;


    float upperLimit
    {
        get
        {
            if (lockToggle == null) return 1.0f;
            if(timeVl==null) return 1.0f;
            if (!lockToggle.isOn) return 1.0f;
            return timeVl.value2;
        }
    }
    float lowerLimit
    {
        get
        {
            if (lockToggle == null) return 0.0f;
            if (timeVl == null) return 0.0f;
            if (!lockToggle.isOn) return 0.0f;
            return timeVl.value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        main = gameObject.GetComponent<Slider>();
       // Button.I
    }
    float playValue = 0;
    // Update is called once per frame
    void Update()
    {
        if(isPlaying)
        {
           float  dt = Time.deltaTime;
            float dv = dt * EventRestAPI.settings.animationSpeed*(upperLimit-lowerLimit);
            playValue += dv;
            if(playValue>=upperLimit||upperLimit-lowerLimit<=0)
            {
                //playValue = lowerLimit;

                Pause();
            }
            main.value = playValue;
        }
    }
    void Pause()
    {
        if (!isPlaying) return;
        OnStopPlaying?.Invoke();
        playButton.GetComponent<Image>().sprite = play;
        isPlaying = false;

    }
    void Play()
    {
        if (isPlaying) return;
        OnStartPlaying?.Invoke();
        if (lockToggle.isOn)
        {
            if (main.value < lowerLimit) main.value = lowerLimit;
            if (main.value > upperLimit) main.value = upperLimit;
            if(main.value==upperLimit)
            {
                main.value = lowerLimit;
            }
        }
        else
        {
            if (main.value >= main.maxValue) main.value = main.minValue;
        }

        playValue = main.value;
        playButton.GetComponent<Image>().sprite = pause;
        isPlaying = true;
        playValue = main.value;
    }
    public void onToggle()
    {
        if(lockToggle.isOn)
        {
            if (main.value < lowerLimit) main.value = lowerLimit;
            if (main.value > upperLimit) main.value = upperLimit;

        }
    }
    public void valueChanged()
    {
       // if (main.value != playValue && isPlaying)
      //  { Pause(); }
    }
    public void OnButtonClicked()
    {
        if (isPlaying) Pause();
        else Play();
    }
 
}
