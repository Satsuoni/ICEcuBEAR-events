using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slidervalue : MonoBehaviour
{

    public Slider slider;
    public Text valueField;
    public Text label;
    public string tformat = null;
    private float _value=0;
    public float Value {
        get { return _value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(slider==null)
        {
            Destroy(this);
            return;
        }
        slider.onValueChanged.AddListener(changeValue);
        changeValue(slider.value);
    }
    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(changeValue);
    }
    void changeValue(float nv)
    {
        _value = nv;
       if(valueField != null)
        {
        if(tformat!=null)
            {
                valueField.text = string.Format(tformat, nv);
            }
        else
            {
                valueField.text = string.Format("{0}", nv);
            }
        }
    }
    // Update is called once per frame
 
}
