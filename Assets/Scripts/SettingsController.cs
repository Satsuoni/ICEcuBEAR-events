using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{

    public Slider csvVal;
    public Slider sdtVal;
    public Slider animVal;
    public Slider powerVal;
    public Slider mulVal;
    public GameObject settingsOverlay;
    public RectTransform settingsPanel;
    public float animSpeed = 3f;
    bool ready = false;
    Vector2 presetMx, presetMn;
    // Start is called before the first frame update
    void Start()
    {
        settingsOverlay.SetActive(false);
        ready = false;
        presetMn = settingsPanel.anchorMin;
        presetMx = settingsPanel.anchorMax;

        settingsPanel.anchorMin = new Vector2(presetMn.x,presetMn.y-presetMx.y);
        settingsPanel.anchorMax = new Vector2(presetMx.x,0);

        settingsPanel.offsetMax = Vector2.zero;
        settingsPanel.offsetMin = Vector2.zero;
        settingsPanel.gameObject.SetActive(false);
        StartCoroutine(waitandSet());
        //EventRestAPI.isDropdownReady
    }
    IEnumerator waitandSet()
    {
        while(EventRestAPI.Instance==null)
        {
            yield return null;
        }
        while(!EventRestAPI.isDropdownReady)
        {
            yield return null;
        }
        //settings should be Ok here
        csvVal.value = EventRestAPI.settings.numberKeptAsCsv;
        sdtVal.value = EventRestAPI.settings.numberIntegrated;
        animVal.value = EventRestAPI.settings.animationSpeed;
        powerVal.value = EventRestAPI.settings.scalePower;
        mulVal.value = EventRestAPI.settings.scaleMul;
        ready = true;
    }
    bool isAnimating = false;
    bool isOpening = false;
    bool open = false;
    float cp =0;

    public void clickedButton()
    {
        if(!ready)
        {
            Debug.Log("waiting for settings");
            return;
        }
        if(!isAnimating)
        {
            isAnimating = true;
            if(open)
            {
                isOpening = false;
                cp = 1.0f;
            }
            else
            {
                isOpening = true;
                cp = 0.0f;
            }
            settingsOverlay.SetActive(true);
            settingsPanel.gameObject.SetActive(true);
        }
        else
        {
            isOpening = !isOpening;
        }

    }
    void setCP(float cp)
    {
        if(cp==1.0f)
        {
            settingsPanel.anchorMin = presetMn;
            settingsPanel.anchorMax = presetMx;
            settingsPanel.offsetMax = Vector2.zero;
            settingsPanel.offsetMin = Vector2.zero;
            return;
        }
       float dlt = presetMn.y - presetMx.y;
        settingsPanel.anchorMin = new Vector2(presetMn.x, presetMn.y - presetMx.y-cp*dlt);
        settingsPanel.anchorMax = new Vector2(presetMx.x, -cp*dlt);

        settingsPanel.offsetMax = Vector2.zero;
        settingsPanel.offsetMin = Vector2.zero;
    }
    // Update is called once per frame
    void Update()
    {
        if(isAnimating)
        {
            float dt = Time.deltaTime;
            if(isOpening)
            {
                cp += dt * animSpeed;
                if(cp>=1)
                {
                    cp = 1.0f;
                    isAnimating = false;
                    open = true;
                    setCP(cp);
                    return;
                }
            }
            else
            {
                cp-= dt * animSpeed;
                if (cp <=0)
                {
                    cp = 0.0f;
                    isAnimating = false;
                    open = false;
                    setCP(cp);
                    settingsOverlay.SetActive(false);
                    settingsPanel.gameObject.SetActive(false);
                    return;
                }
            }
            setCP(cp);

        }
    }
}
