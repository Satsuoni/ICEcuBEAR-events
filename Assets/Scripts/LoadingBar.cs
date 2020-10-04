using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoadingBar : MonoBehaviour
{

    public Image innerCircle;
    public Image outerCircle;
    public TMP_Text title;
    public TMP_Text center;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetTitle(string s)
    {
        title.text = s;
    }
    public void SetCenter(string s)
    {
        center.text = s;
    }
    public void SetCenter(int x,int lim)
    {
        center.text = string.Format("{0}/{1}",x,lim);
    }
    public void SetInner(float percent)
    {
        innerCircle.fillAmount = percent;

    }
    public void SetOuter(float percent)
    {
        outerCircle.fillAmount = percent;

    }
}
