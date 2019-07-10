using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class singleBall : MonoBehaviour
{

    MeshRenderer rnd;
    MaterialPropertyBlock props = null;
    // Start is called before the first frame update
    void Start()
    {
        
            rnd = gameObject.GetComponent<MeshRenderer>();
        if(props==null) props= new MaterialPropertyBlock();
        /*float r = Random.Range(0.0f, 1.0f);
        float g = Random.Range(0.0f, 1.0f);
        float b = Random.Range(0.0f, 1.0f);
        props.SetColor("_color", new Color(r, g, b));
        rnd.SetPropertyBlock(props);*/
    }
    float _sig = -1;
    public void resetSig()
    {
        _sig = -1;
    }
    public float setAndCompareSig(float sig)
    {
        if (_sig > 0) return 0;//for now, only react on first hit... 
        if(sig!=_sig)
        {
            _sig = sig;
            return sig;

        }
        return 0;
    }
    public void setScale(float scale)
    {
        if (props == null) props = new MaterialPropertyBlock();
        if (rnd == null) rnd = gameObject.GetComponent<MeshRenderer>();
        transform.localScale = new Vector3(scale,scale,scale);
    }
    public void setColor(Color clr)
    {
        if (props == null) props = new MaterialPropertyBlock();
        if(rnd==null) rnd = gameObject.GetComponent<MeshRenderer>();
        props.SetColor("_color", clr);
        rnd.SetPropertyBlock(props);
    }
    // Update is called once per frame
    void Update()
    {

       
    }
}
