using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MessagePack;
using System.IO;
using MessagePack.Resolvers;
using System.Text;
using System.Security.Cryptography;

[Serializable]
public class singleBallList
{
    public singleBallList(int lst)
    {
        balls = new singleBall[lst];
    }
    public singleBall[] balls;
}

[Serializable]
public struct eventData
{
    public float time;
    public double signal;
    public int dom;
    public int str;
    public float time_period;
}

[Serializable]
public struct ballDataPoint
{
    public float timeScale;
    public int stId;
    public int domId;
    public float size;
    public Color color;
}

[Serializable,MessagePackObject]
public class ballIntegratedData
{
    [Key(0)]
    public bool affected;
    [Key(1)]
    public float mintime;
    [Key(2)]
    public float maxtime;
    [Key(3)]
    public double[] icharge; //1000 pts? *not* in affected area?
    [Key(4)]
    public int stId;
    [Key(5)]
    public int domId;
    public bool ballAffectedInTs(float start, float end)
    {
       // acc = 0;
        if (start > maxtime) return false;
        if (end < mintime) return false;
        int pt1 = (int)(Mathf.RoundToInt(start * icharge.Length));
        int pt2 = (int)(Mathf.RoundToInt(end * icharge.Length));
        if (pt1 == 0 && pt2 == 0) return false; //too narrow...
        if (pt2 >= icharge.Length) pt2 = icharge.Length - 1;
        if (pt1 >= pt2) pt1 = pt2-1;
        double diff = icharge[pt2] - icharge[pt1];
        if (diff < 0.00001f) return false;
        //acc = diff;
        return true;
    }
  
   public double getInSpan(float start, float end,float span,float tim,ref float tscale)
    {

        int pt1 = (int)(Mathf.RoundToInt(start * icharge.Length));
        int pt2 = (int)(Mathf.RoundToInt(end * icharge.Length));
        int delta= (int)(Mathf.RoundToInt(span * icharge.Length));
        int endtime = (int)(Mathf.RoundToInt(tim * icharge.Length));
        if (endtime >= icharge.Length) endtime = icharge.Length-1;
        if (pt1 == 0 && pt2 == 0) return 0.0; //too narrow...
        if (pt2 >= icharge.Length) pt2 = icharge.Length - 1;
        if (pt1 >= pt2) pt1 = pt2 - 1;
        if (delta <= 0) delta = 1;
       // if(delta >= pt2-pt1) { tscale = 0; return icharge[pt2] - icharge[pt1]; }
        int tmax = pt1;
        double mx = -1;
        int imx = -1;
        int ep = Math.Min(pt2, endtime);
        for (int i = pt1+1; i < ep; i += 1)
        {
            /* int np = i + delta;
             if (np > pt2) np = pt2;
             double df= icharge[np] - icharge[i];
             if(df>mx)
             {
                 mx = df;
                 imx = i; 
             }*/
            int np = i - delta;
            if ( icharge[i] - icharge[i-1]>0)
            {
                imx = i;
                break;
            }
        }
        if (imx == -1)
        {
            tscale = -1;
            return -1;
        }
        int sfile = endtime - delta;
        if(sfile<0)
        {
            mx = icharge[endtime];
        }
        else
        {
            mx = icharge[endtime]-icharge[sfile];
        }

    
        tscale = (float)(imx-pt1) / (float)(pt2-pt1);
       // Debug.Log(tscale);
        return mx;
    }
}
[MessagePackObject]
public class fullEventData
{
    [Key(0)]
    public String eventName;
    [Key(1)]
    public List<ballIntegratedData> ballData;
    [Key(2)]
    public int isteps;
    [Key(3)]
    public eventDesc description;
    [Key(4)]
    public float minPureTime;
    [Key(5)]
    public float maxPureTime;

}
public class DOMController : MonoBehaviour
{
    //time, signal, om, string, time_period
    // Start is called before the first frame update
    public float delay = 1f;
    public float tgap = 0.1f;
    float baseScale = 1.2f;
   // double power = 0.15;
    //double _sscale = 5.0;


    RangeSlider timeSpan;
    Slider deltaController;
    Slider timeController;
    Slider zoomControl;
    const int timeSpans = 2500;
    public Transform footPoint;
    public Transform footAlign;
    List<eventData> curEvent = new List<eventData>();
    [SerializeField]
    public singleBallList[] ballArray = null;//new int[5][];  
   // Dictionary<int, Dictionary<int, ballIntegratedData>> affBalls = new Dictionary<int, Dictionary<int, ballIntegratedData>>();
    //List<ballIntegratedData> collectedBalls = new List<ballIntegratedData>();
  
    void Start()
    {
        curEvent = new List<eventData>();
        GameObject timeCon = GameObject.Find("timeController");
        if (timeCon!=null)
        {
            timeController = timeCon.GetComponent<Slider>();
        }
        GameObject gapCon = GameObject.Find("deltaController");
        if (gapCon != null)
        {
            deltaController = gapCon.GetComponent<Slider>();
        }
        if(timeController!=null)
        {
            timeController.onValueChanged.AddListener(delegate { ValueChange(); });
        }
        if (deltaController != null)
        {
            deltaController.onValueChanged.AddListener(delegate { ValueChange(); });
        }
        GameObject tspn = GameObject.Find("timeSpan");
        if(tspn!=null)
        {
            timeSpan = tspn.GetComponent<RangeSlider>();
            if(timeSpan!=null)
            {
                timeSpan.onValueChanged.AddListener(delegate { ValueChange(); });
            }
        }
        GameObject zCon = GameObject.Find("zoomController");
        if (zCon != null)
        {
            zoomControl = zCon.GetComponent<Slider>();
            if(zoomControl!=null)
            {
                zoomControl.onValueChanged.AddListener(delegate { zoomChange(); });
            }
        }
        foreach (singleBallList lst in ballArray)
        {
            foreach(singleBall bl in lst.balls)
            {
                if (bl != null)
                {
                    bl.setColor(new Color(0.5f, 0.5f, 0.5f));
                    bl.setScale(baseScale);
                }
            }
        }
        

        // size = scale * ( 0.2 * accum ) ** power
        updateToSet();
    }
    void Awake()
    {
        Utilz.currentEventUpdated += updateForce;
        Debug.Log("Nyanya");
    }
    void OnDestroy()
    {
        Utilz.currentEventUpdated -= updateForce;
    }
    public void registerStringId(int id)
    {
        if (id < 0) return;
        if (ballArray == null)
        {
            ballArray = new singleBallList[id + 1];
        }
        else
        {
            if (id < ballArray.Length) return;
            Array.Resize(ref ballArray, id+1);
        }
    }
    public void registerDomId(int sid,int did)
    {
        if (sid < 0 || did<0) return;
        if (sid >= ballArray.Length) registerStringId(sid);
        if (ballArray[sid] == null)
        {
            ballArray[sid] = new singleBallList(did + 1);
        }
        else
        {
            if (did < ballArray[sid].balls.Length) return;
            Array.Resize(ref ballArray[sid].balls, did + 1);
        }
    }
 
    public Color colorFromBasicMap(float t)
    {
        if (t < 0) t = 0;
        if (t > 1) t = 1;
        if (timeSpan!=null&&timeSpan.betweenImage!=null)
        {
           
            /// need read/write enabled on texture.
            Color c= timeSpan.betweenImage.texture.GetPixelBilinear(t, 0.5f);
            if(c!=null)
            {
                return c;
            }
            else
            {
                Debug.Log("Between texture needs fixing");
            }
        }
       
        if(t<=0.5f) return Color.Lerp(Color.red, Color.green, t*2.0f);
        return Color.Lerp(Color.green, Color.blue, (t-0.5f) * 2.0f);
    }
    float prevSld = 0.0f;
    List<ballIntegratedData> beforeBalls = null;
    public void updateForce()
    {
        KeyValuePair<float, float> rng = getDefaultRange();
        timeSpan.SetValueWithoutNotify(rng.Key);
        timeSpan.SetValue2WithoutNotify(rng.Value);
       
        if (timeController.value < rng.Key) timeController.SetValueWithoutNotify(rng.Key);
        if (timeController.value > rng.Value) timeController.SetValueWithoutNotify(rng.Value);

        updateToSet(true);
    }
    public KeyValuePair<float, float> getDefaultRange()
    {
        if(EventRestAPI.Instance==null)
        {
            return new KeyValuePair<float, float>(0, 1);
        }
        fullEventData evv = EventRestAPI.Instance.currentEvent;
        List<ballIntegratedData> collectedBalls = EventRestAPI.Instance.currentEvent.ballData;
        if(collectedBalls==null)
        {
            return new KeyValuePair<float, float>(0, 1);
        }
        double mxcharge=0;
        ballIntegratedData mball=null;
        foreach (ballIntegratedData ball in collectedBalls)
        {
            if(ball.icharge[ball.icharge.Length-1]>mxcharge)
            {
                mball = ball;
                mxcharge = ball.icharge[ball.icharge.Length - 1];
            }
        }
       // Debug.LogFormat("Mball{0} {1} {2}",mball,mball.mintime,mball.maxtime);
        if (mball==null)
        {
            return new KeyValuePair<float, float>(0, 1);
        }
        int pos = 0;
        for(pos=0;pos<mball.icharge.Length;pos++)
        {
            if (mball.icharge[pos] > 0) break;
        }
        if(Mathf.Approximately(mball.maxtime - mball.mintime,0.0f))
            return new KeyValuePair<float, float>(0, 1);

        if (evv.maxPureTime == 0 && evv.minPureTime >= evv.maxPureTime)
        {
            float ptime = (((float)pos) / ((float)(mball.icharge.Length))) * (mball.maxtime - mball.mintime);
        
            float ltime0 = ptime * 0.9f;
            float ltime1 = ptime * 1.5f;
            float cntime0 = ltime0 / (mball.maxtime - mball.mintime);
            float cntime1 = ltime1 / (mball.maxtime - mball.mintime);
            if (cntime0 < 0) cntime0 = 0;
            if (cntime1 > 1) cntime1 = 1;
            if (cntime0 > cntime1)
            {
                return new KeyValuePair<float, float>(0, 1);
            }
            return new KeyValuePair<float, float>(cntime0, cntime1);
        }
        else
        {
            float ptime = (((float)pos) / ((float)(mball.icharge.Length))) * (evv.maxPureTime - evv.minPureTime);
            float scl = 1f;
            float ltime0 = ptime - scl * 1000;
            float ltime1 = ptime + scl * 5000;
            float cntime0 = ltime0 / (evv.maxPureTime - evv.minPureTime);
            float cntime1 = ltime1 / (evv.maxPureTime - evv.minPureTime);
            if (cntime0 < 0) cntime0 = 0;
            if (cntime1 > 1) cntime1 = 1;
            if (cntime0 > cntime1)
            {
                return new KeyValuePair<float, float>(0, 1);
            }
            return new KeyValuePair<float, float>(cntime0, cntime1);
        }
    }
    public void updateToSet(bool force=false)
    {
        if(EventRestAPI.Instance==null)
        {
            Debug.Log("Nyanya: no instance");
            return;
        }
        if (EventRestAPI.Instance.currentEvent == null)
        {
        //    Debug.Log("Nyanya: noevent");
            return;
        }
        float nsld = 0.0f;
       
        if (timeController != null)
        {
            nsld = timeController.value;
        }

        if (deltaController != null)
        {
            tgap = deltaController.value;
        }
        if (nsld == prevSld &&beforeBalls!=null&&!force) return;
        if(beforeBalls!=null)
        {
            foreach(ballIntegratedData id in beforeBalls)
            {
                ballArray[id.stId].balls[id.domId].setScale(baseScale);
                ballArray[id.stId].balls[id.domId].setColor(new Color(0.5f, 0.5f, 0.5f));

            }
        }
         
        List<ballIntegratedData> afterBalls = new List<ballIntegratedData>();
        float start = timeSpan.value;// nsld; TODO
        float end = timeSpan.value2;//nsld + tgap;
        if (end > 1.0f) end = 1.0f;
        List<ballIntegratedData> collectedBalls = EventRestAPI.Instance.currentEvent.ballData;
        foreach (ballIntegratedData ball in collectedBalls)
        {
           if(ball.ballAffectedInTs(start,end))
            {
                afterBalls.Add(ball);
                float tscl = 0.0f;
                double sig = ball.getInSpan(start, end, tgap, nsld,ref tscl);
                if (sig != -1)
                {
                    double scale = (double)baseScale + (EventRestAPI.settings.scaleMul * Math.Pow((0.2 * sig), EventRestAPI.settings.scalePower));
                    ballArray[ball.stId].balls[ball.domId].setScale((float)scale);
                    ballArray[ball.stId].balls[ball.domId].setColor(colorFromBasicMap(tscl));
                }
                else
                {
                    ballArray[ball.stId].balls[ball.domId].setScale(baseScale);
                    ballArray[ball.stId].balls[ball.domId].setColor(new Color(0.5f, 0.5f, 0.5f));
                }
            }
        }
        // public float delay = 0.01f;
        //public float tgap = 0.1f;
        beforeBalls = afterBalls;
}
    public void ValueChange()
    {
        updateToSet();
    }
    public void zoomChange()
    {
        if (zoomControl == null) return;
        float nzoom = zoomControl.value;
        if (nzoom < 0.001f) nzoom = 0.001f;
        if (nzoom > 0.015f) nzoom = 0.015f;
        transform.localScale = new Vector3(nzoom,nzoom,nzoom);
        if(footPoint!=null && footAlign!=null)
        {
            Vector3 v1 = footPoint.position - footAlign.position;
            transform.position = transform.position + v1;
        }
    }
        public void setBall(int sid, int did,singleBall sball)
    {
        registerDomId(sid, did);
        ballArray[sid].balls[did] = sball;
    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
