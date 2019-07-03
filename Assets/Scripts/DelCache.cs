using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelCache : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void No()
    {

    }
    public void Yes()
    {
        if (EventRestAPI.Instance!=null)
        EventRestAPI.Instance.deleteEventsData();
    }
    public void OnClick()
    {
        ModalCaller cl = gameObject.GetComponent<ModalCaller>();
        if(cl!=null)
        {
            cl.Call(Yes,No);
        }


    }
}
