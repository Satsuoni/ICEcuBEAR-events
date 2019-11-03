using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAnim : MonoBehaviour
{
    public GameObject[] set_active=null;
    public float duration = 0.5f;
    Quaternion origin;
    Quaternion goal;
    float stpos = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.localRotation;
        goal = origin;
    }

    public void SetRotation(Quaternion newrot)
    {
        origin = transform.localRotation;
        goal = newrot;
        if (set_active!=null)
        foreach(var go in set_active)
        {
                go.SetActive(false);
        }
        stpos = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if(stpos<1)
        {
            float dt = Time.deltaTime;
            float dst = dt / duration;
            stpos += dst;
            if (stpos > 1) stpos = 1;
            transform.localRotation = Quaternion.Slerp(origin,goal,stpos);
            if(stpos>=1)
            {
                if (set_active != null)
                    foreach (var go in set_active)
                    {
                        go.SetActive(true);
                    }
            }
        }
    }
}
