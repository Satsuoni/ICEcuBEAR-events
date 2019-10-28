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

public class HideUI : MonoBehaviour
{

    public GameObject[] hidden=null;
    bool[] wasHidden=null;
    public SubstTrans[] moved;
    bool _hidden = false;
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
        foreach(SubstTrans s in moved)
        {
     Vector4 landscapeAnchor = s.core.landscapeAnchor;
     Vector4 portraitAnchor = s.core.portraitAnchor;
     Vector4 landscapeOffset = s.core.landscapeOffset;
     Vector4 portraitOffset = s.core.portraitOffset;
     Vector2 landscapePivot = s.core.landscapePivot;
     Vector2 portraitPivot = s.core.landscapePivot;
            s.core.landscapeAnchor = s.landscapeAnchor;
            s.core.portraitAnchor = s.portraitAnchor;
            s.core.landscapeOffset = s.portraitOffset;
            s.core.portraitOffset = s.portraitOffset;
            s.core.landscapePivot = s.landscapePivot;
            s.core.portraitPivot = s.portraitPivot;
            s.landscapeAnchor = landscapeAnchor;
            s.portraitAnchor = portraitAnchor;
            s.landscapeOffset = landscapeOffset;
            s.portraitOffset = portraitOffset;
            s.landscapePivot = landscapePivot;
            s.portraitPivot = portraitPivot;
            s.core.Align();
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
