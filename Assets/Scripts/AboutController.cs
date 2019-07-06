using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutController : MonoBehaviour
{

    public GameObject aboutOverlay;
    public RectTransform aboutPanel;
    Landscaped sizecontrol = null;
    public float animSpeed = 3f;
  
    Vector2 presetMx, presetMn;
    // Start is called before the first frame update
    void Start()
    {
        aboutOverlay.SetActive(false);
        
        presetMn = aboutPanel.anchorMin;
        presetMx = aboutPanel.anchorMax;
        sizecontrol = aboutPanel.gameObject.GetComponent<Landscaped>();

        aboutPanel.anchorMin = new Vector2(presetMn.x, presetMn.y - presetMx.y);
        aboutPanel.anchorMax = new Vector2(presetMx.x, 0);

        aboutPanel.offsetMax = Vector2.zero;
        aboutPanel.offsetMin = Vector2.zero;
        aboutPanel.gameObject.SetActive(false);
    }
    bool isAnimating = false;
    bool isOpening = false;
    bool open = false;
    float cp = 0;
    public void clickedCLose()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            isOpening = false;
            //cp = 1.0f;
           
            aboutOverlay.SetActive(true);
            aboutPanel.gameObject.SetActive(true);
        }
        else
        {
            isOpening = false;
        }
    }
    public void clickedButton()
    {

        if (!isAnimating)
        {
            isAnimating = true;
            if (open)
            {
                isOpening = false;
                cp = 1.0f;
            }
            else
            {
                isOpening = true;
                cp = 0.0f;
            }
            aboutOverlay.SetActive(true);
            aboutPanel.gameObject.SetActive(true);
        }
        else
        {
            isOpening = !isOpening;
        }

    }
    public void onObservatoryClick()
    {
        Application.OpenURL("https://icecube.wisc.edu/"); 
    }
    public void onLearnClick()
    {
        Application.OpenURL("https://gcn.gsfc.nasa.gov/amon.html");
    }
    void setCP(float cp)
    {
        if (sizecontrol != null)
        {
            if (sizecontrol.isPortrait)
            {
                presetMn = new Vector2(sizecontrol.portraitAnchor.x, sizecontrol.portraitAnchor.y);
                presetMx = new Vector2(sizecontrol.portraitAnchor.z, sizecontrol.portraitAnchor.w);

            }
            else
            {
                presetMn = new Vector2(sizecontrol.landscapeAnchor.x, sizecontrol.landscapeAnchor.y);
                presetMx = new Vector2(sizecontrol.landscapeAnchor.z, sizecontrol.landscapeAnchor.w);

            }
        }
        if (cp == 1.0f)
        {
            aboutPanel.anchorMin = presetMn;
            aboutPanel.anchorMax = presetMx;
            aboutPanel.offsetMax = Vector2.zero;
            aboutPanel.offsetMin = Vector2.zero;
            return;
        }
        float dlt = presetMn.y - presetMx.y;
        aboutPanel.anchorMin = new Vector2(presetMn.x, presetMn.y - presetMx.y - cp * dlt);
        aboutPanel.anchorMax = new Vector2(presetMx.x, -cp * dlt);

        aboutPanel.offsetMax = Vector2.zero;
        aboutPanel.offsetMin = Vector2.zero;
    }
    // Update is called once per frame
    void Update()
    {
        if (isAnimating)
        {
            float dt = Time.deltaTime;
            if (isOpening)
            {
                cp += dt * animSpeed;
                if (cp >= 1)
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
                cp -= dt * animSpeed;
                if (cp <= 0)
                {
                    cp = 0.0f;
                    isAnimating = false;
                    open = false;
                    setCP(cp);
                    aboutOverlay.SetActive(false);
                    aboutPanel.gameObject.SetActive(false);
                    return;
                }
            }
            setCP(cp);

        }
    }
}
