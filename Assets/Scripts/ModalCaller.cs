using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalCaller : MonoBehaviour
{
    public Modalyesno prefab;
    Canvas root;
    // Start is called before the first frame update
    void Start()
    {
        Canvas cnv = gameObject.GetComponentInParent<Canvas>();
        if(cnv!=null)
        {
            root = cnv.rootCanvas;
            if (root == null)
                root = cnv;
        }
    }
    public void Call(Modalyesno.cback yes, Modalyesno.cback no)
    {
        if(root!=null)
        {
            Modalyesno pr = Instantiate<Modalyesno>(prefab, root.transform, false);
            pr.yes = yes;
            pr.no = no;
        }
    }
    // Update is called once per frame
   
}
