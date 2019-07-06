using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class Landscaper : MonoBehaviour
{
    public Landscaped[] landscaped;
    // Start is called before the first frame update
    RectTransform mine;
    [SerializeField]
    bool _isPortrait = false;
    [SerializeField]
    public bool isPortrait

    {
        get { return _isPortrait; }
    }
    void Start()
    {
        mine = gameObject.GetComponent<RectTransform>();
    }
    public void OnEnabled()
    {
        landscaped = gameObject.GetComponentsInChildren<Landscaped>(true);
        mine = gameObject.GetComponent<RectTransform>();
        if(mine!=null)
        {
            Vector2 wh = mine.offsetMax - mine.offsetMin;
            if (wh.x > wh.y)
            {
                _isPortrait = false;
            }
            else
            {
                _isPortrait = true;
            }

            foreach (Landscaped l in landscaped)
            {
                l.Switch(_isPortrait);
            }
        }
    }
    void OnRectTransformDimensionsChange()
    {
        landscaped = gameObject.GetComponentsInChildren<Landscaped>(true);
        mine = gameObject.GetComponent<RectTransform>();
       // Debug.LogFormat("Changed {0} {1}",mine.offsetMin,mine.offsetMax);
        Vector2 wh = mine.offsetMax - mine.offsetMin;
        bool sw = false;
        if (wh.x > wh.y)
        {
            //Debug.Log("Landscape");
            if (_isPortrait) sw = true;
            _isPortrait = false;
        }
        else
        {
            if (!_isPortrait) sw = true;
            _isPortrait = true;
        }
        if(sw)
        {
            foreach(Landscaped l in landscaped)
            {
                
                l.Switch(_isPortrait);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
