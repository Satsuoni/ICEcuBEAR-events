using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericFader : MonoBehaviour
{
    public float refDuration = 2.0f;
    public float fadeDuration = 1.0f;
    CanvasGroup group;
    // Start is called before the first frame update
    void Start()
    {
        if (group == null)
            group = gameObject.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    float _alpha = 0;
    float _refractory = 0;
    public void Show()
    {
        _refractory = refDuration;
        _alpha = 1;
        if (group == null)
            group = gameObject.GetComponent<CanvasGroup>();
        group.alpha = _alpha;

    }
 
    void Update()
    {
        float dt = Time.deltaTime;
        if (_refractory > 0)
        {
            _refractory -= dt;
            if (_refractory < 0)
            {
                dt = -_refractory;
                _refractory = 0;
            }
        }
        if (_refractory <= 0 && _alpha > 0)
        {
            if (fadeDuration <= 0)
            {
                fadeDuration = 0.01f;
            }
            _alpha -= dt / fadeDuration;
            if (_alpha < 0) _alpha = 0;

        }
        group.alpha = _alpha;
    }
}
