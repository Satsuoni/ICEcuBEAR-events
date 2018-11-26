using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShaderProp : MonoBehaviour
{
    private float last_time;
    public float curr_time = 0;
    public float time_mod = 0.005f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        curr_time += Time.deltaTime * time_mod;
        if ((curr_time - last_time) / time_mod > 0.02)
        {
            foreach (Transform child in transform)
            {
                var material = ((Renderer)child.GetComponent("Renderer")).material;
                //Debug.Log(curr_time.ToString());
                material.SetFloat("mytime", curr_time);
            }
            last_time = curr_time;
        }

        // for looping enable below (20s)
        //if (curr_time / time_mod > 20)
        //{
        //    resetTime();
        //}
    }
    public void resetTime()
    {
        curr_time = 0;
        last_time = 0;
    }

    public void OnInputClicked()
    {
        resetTime();
    }
}