using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    public static float playbackSpeed = 0.1f;
    Slider main;
    public Button playButton;
    bool isPlaying = false;
    public Sprite play;
    public Sprite pause;
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
            float dv = dt * playbackSpeed;
            playValue += dv;
            if(playValue>=1.0f)
            {
                playValue = 0.0f;

                Pause();
            }
            main.value = playValue;
        }
    }
    void Pause()
    {
        if (!isPlaying) return;
        playButton.GetComponent<Image>().sprite = play;
        isPlaying = false;

    }
    void Play()
    {
        if (isPlaying) return;
        playValue = main.value;
        playButton.GetComponent<Image>().sprite = pause;
        isPlaying = true;
        playValue = main.value;
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
