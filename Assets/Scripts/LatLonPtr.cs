using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LatLonPtr : MonoBehaviour
{
    public Vector2 latlon;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        float dsin = Mathf.Sin(latlon.y * Mathf.PI / 180);
        float dcos = Mathf.Cos(latlon.y * Mathf.PI / 180);
        float rsin = Mathf.Sin(latlon.x * Mathf.PI / 180);
        float rcos = Mathf.Cos(latlon.x * Mathf.PI / 180);

        Vector3 forw = new Vector3(dcos * rsin,  dsin,- dcos * rcos);
        gameObject.transform.localRotation = Quaternion.LookRotation(forw);
    }
}
