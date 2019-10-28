using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Landscaped : MonoBehaviour
{
    public static Vector2 inv2 = new Vector2(-1e5f, -2e5f);
    public static Vector4 inv4 = new Vector4(-1e5f, -2e5f,-2.5e6f,-1);
    
    public bool isPortrait = false;
    public Vector4 landscapeAnchor=inv4;
    public Vector4 portraitAnchor = inv4;
    public Vector4 landscapeOffset = inv4;
    public Vector4 portraitOffset = inv4;
    public Vector2 landscapePivot = inv2;
    public Vector2 portraitPivot = inv2;

    RectTransform self;
    // Start is called before the first frame update
    void Start()
    {
        self = gameObject.GetComponent<RectTransform>();
        if (self == null) return;
        if(isPortrait)
        {
            if (portraitAnchor==inv4)
            {
                portraitAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
                portraitOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
                portraitPivot = self.pivot;
            }
        }
        else
        {
            if(landscapeAnchor==inv4)
            {
                landscapeAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
                landscapeOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
                landscapePivot = self.pivot;
            }
        }
    }
    public void Align()
    {
        if(isPortrait)
        {
            self.anchorMax = new Vector2(portraitAnchor.z, portraitAnchor.w);
            self.anchorMin = new Vector2(portraitAnchor.x, portraitAnchor.y);
            self.offsetMin = new Vector2(portraitOffset.x, portraitOffset.y);
            self.offsetMax = new Vector2(portraitOffset.z, portraitOffset.w);
            self.pivot = portraitPivot;
        }
        else
        {
            self.anchorMax = new Vector2(landscapeAnchor.z, landscapeAnchor.w);
            self.anchorMin = new Vector2(landscapeAnchor.x, landscapeAnchor.y);
            self.offsetMin = new Vector2(landscapeOffset.x, landscapeOffset.y);
            self.offsetMax = new Vector2(landscapeOffset.z, landscapeOffset.w);
        }
    }
    public void Switch(bool isPor)
    {
        if (self == null)
        {
            self = gameObject.GetComponent<RectTransform>();
            if(self==null)
            return;

        }
        
        if (portraitAnchor==inv4)
        {
            portraitAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
            portraitOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
            portraitPivot = self.pivot;
        }
        if(landscapeAnchor==inv4)
        {
            landscapeAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
            landscapeOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
            landscapePivot = self.pivot;
        }
        if(isPor!=isPortrait)
        {
            isPortrait = isPor;
            if(isPortrait)
            {
                landscapeAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
                landscapeOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
                landscapePivot = self.pivot;
                self.anchorMax = new Vector2(portraitAnchor.z, portraitAnchor.w);
                self.anchorMin = new Vector2(portraitAnchor.x, portraitAnchor.y);
                self.offsetMin = new Vector2(portraitOffset.x, portraitOffset.y);
                self.offsetMax = new Vector2(portraitOffset.z, portraitOffset.w);
                self.pivot = portraitPivot;
            }
            else
            {
                portraitAnchor = new Vector4(self.anchorMin.x, self.anchorMin.y, self.anchorMax.x, self.anchorMax.y);
                portraitOffset = new Vector4(self.offsetMin.x, self.offsetMin.y, self.offsetMax.x, self.offsetMax.y);
                portraitPivot = self.pivot;
                self.anchorMax = new Vector2(landscapeAnchor.z, landscapeAnchor.w);
                self.anchorMin = new Vector2(landscapeAnchor.x, landscapeAnchor.y);
                self.offsetMin = new Vector2(landscapeOffset.x, landscapeOffset.y);
                self.offsetMax = new Vector2(landscapeOffset.z, landscapeOffset.w);
                self.pivot = landscapePivot;
            }
        }
        if (gameObject.name=="Image")
        {
            Debug.LogFormat("Tmr* {0} {1} {2} {3}", Time.frameCount, Time.time, self.anchorMin,self.anchorMax);

        }
    }

    
}
