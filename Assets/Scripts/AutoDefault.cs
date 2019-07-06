using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoDefault : MonoBehaviour
{
    RangeSlider timeSpan;
    // Start is called before the first frame update
    void Start()
    {
        timeSpan = gameObject.GetComponent<RangeSlider>();
    }

    // Update is called once per frame
   
    public void updateRange()

    {
        if (timeSpan == null) return;
        KeyValuePair<float, float> rng = DOMController.getDefaultRange();
        Debug.LogFormat("Got def range {0} {1}", rng.Key, rng.Value);
        timeSpan.SetValueWithoutNotify(rng.Key);
        timeSpan.SetValue2WithoutNotify(rng.Value);
    }
    void Awake()
    {
        Utilz.currentEventUpdated += updateRange;
        Debug.Log("defualt");
    }
    void OnDestroy()
    {
        Utilz.currentEventUpdated -= updateRange;
    }
}
