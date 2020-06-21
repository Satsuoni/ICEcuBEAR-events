using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubstTrans
{
    public Vector4 landscapeAnchor = Landscaped.inv4;
    public Vector4 portraitAnchor = Landscaped.inv4;
    public Vector4 landscapeOffset = Landscaped.inv4;
    public Vector4 portraitOffset = Landscaped.inv4;
    public Vector2 landscapePivot = Landscaped.inv2;
    public Vector2 portraitPivot = Landscaped.inv2;
    public Landscaped core;
}
[System.Serializable]
public class Toggleable
{
    public GameObject togglecontainer;
    public string nameOn="on";
    public string nameOff="off";
    ToggleContraint[] constraints=null;
   public ToggleContraint [] getToggles()
    {
        if (togglecontainer == null)
            return new ToggleContraint[0];

        if(constraints==null)
        {
            constraints = togglecontainer.GetComponents<ToggleContraint>();
        }
        return constraints;
    }
    public void RefreshToggles()
    {
        if (togglecontainer == null)
            return;
        constraints = togglecontainer.GetComponents<ToggleContraint>();
    }
}


public class HideUI : MonoBehaviour
{

    public GameObject[] hidden=null;
    bool[] wasHidden=null;
    public Toggleable[] moved;
    bool _hidden = false;
    void OnValidate()
    {
        foreach (Toggleable s in moved)
        {
            s.RefreshToggles();
   }
        return;
    }
        public void Toggle()
    {
        if(_hidden)
        { //unhide
            for(int i=0;i<hidden.Length;i++)
            {
                GameObject g = hidden[i];
                g.SetActive(wasHidden[i]);
            }
    }
        else
        {
                for(int i=0;i<hidden.Length;i++)
            {
                GameObject g = hidden[i];
                wasHidden[i] = g.activeSelf;
               g.SetActive(false);
            }
        }
        foreach(Toggleable s in moved)
        {
            ToggleContraint[] cns = s.getToggles();
            if(cns!=null)
            foreach (ToggleContraint cn in cns)
            {
                    if (_hidden)
                        cn.ToggleByName(s.nameOn);
                    else
                        cn.ToggleByName(s.nameOff);

                }
        }
        _hidden = !_hidden;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(hidden!=null&&hidden.Length>0)
        wasHidden = new bool[hidden.Length];

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
