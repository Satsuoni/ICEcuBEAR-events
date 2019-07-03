using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modalyesno : MonoBehaviour
{
    // Start is called before the first frame update
    public delegate void cback();
    public cback yes;
    public cback no;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnYes()
    {
        Destroy(gameObject);
        yes?.Invoke();

    }
    public void OnNo()
    {
        Destroy(gameObject);
        no?.Invoke();

    }
}
