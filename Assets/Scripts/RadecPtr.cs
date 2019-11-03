using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RadecPtr : MonoBehaviour
{
    public Vector2 radec;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float dsin = Mathf.Sin(radec.y * Mathf.PI / 180);
        float dcos = Mathf.Cos(radec.y * Mathf.PI / 180);
        float rsin = Mathf.Sin(radec.x * Mathf.PI / 180);
        float rcos = Mathf.Cos(radec.x * Mathf.PI / 180);

        Vector3 forw =new Vector3(dcos*rcos,dcos*rsin,dsin);
        gameObject.transform.localRotation = Quaternion.LookRotation(forw);
    }
}
