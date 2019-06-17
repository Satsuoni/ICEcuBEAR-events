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
   public double getInSpan(float start, float end,float span,ref float tscale)
    {
        int pt1 = (int)(Mathf.RoundToInt(start * icharge.Length));
        int pt2 = (int)(Mathf.RoundToInt(end * icharge.Length));
        int delta= (int)(Mathf.RoundToInt(span * icharge.Length));
        if (pt1 == 0 && pt2 == 0) return 0.0; //too narrow...
        if (pt2 >= icharge.Length) pt2 = icharge.Length - 1;
        if (pt1 >= pt2) pt1 = pt2 - 1;
        if (delta <= 0) delta = 1;
        if(delta >= pt2-pt1) { tscale = 0; return icharge[pt2] - icharge[pt1]; }
        int tmax = pt1;
        double mx = 0;
        int imx = pt1;
        for(int i=pt1;i<pt2;i+=1)
        {
            int np = i + delta;
            if (np > pt2) np = pt2;
            double df= icharge[np] - icharge[i];
            if(df>mx)
            {
                mx = df;
                imx = i; 
            }
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


}
public class DOMController : MonoBehaviour
{
    //time, signal, om, string, time_period
    // Start is called before the first frame update
    public float delay = 0.01f;
    public float tgap = 0.1f;
    float baseScale = 1.2f;
    double power = 0.15;
    double sscale = 5.0;
    Slider slid;
    Slider gapControl;
    const int timeSpans = 2500;
    List<eventData> curEvent = new List<eventData>();
    [SerializeField]
    public singleBallList[] ballArray = null;//new int[5][];  
    Dictionary<int, Dictionary<int, ballIntegratedData>> affBalls = new Dictionary<int, Dictionary<int, ballIntegratedData>>();
    List<ballIntegratedData> collectedBalls = new List<ballIntegratedData>();
  
    void Start()
    {
        curEvent = new List<eventData>();
        GameObject timeCon = GameObject.Find("timeControl");
        if (timeCon!=null)
        {
            slid = timeCon.GetComponent<Slider>();
        }
        GameObject gapCon = GameObject.Find("gapControl");
        if (gapCon != null)
        {
            gapControl = gapCon.GetComponent<Slider>();
        }
        Debug.Log(string.Format("Lengthh {0}", ballArray.Length));
        TextAsset eventl = Resources.Load("Data/Hydrangea") as TextAsset;
        string fs = eventl.text;
        string[] fLines = null;
        if (fs.Contains("\r\n"))
        {
            fLines = System.Text.RegularExpressions.Regex.Split(fs, "\r\n");
        }
        else
        {
            fLines = System.Text.RegularExpressions.Regex.Split(fs, "\n|\r");

        }
        float timeMin=-1f, timeMax=-1f;
        double signalMin = -1f, signalMax = -1f;
        foreach (var line in fLines)
        {
            try
            {
                string[] fields = line.Split(',');
                eventData dat = new eventData();
                dat.time = float.Parse(fields[0]);
                dat.signal = double.Parse(fields[1]);
                dat.dom = int.Parse(fields[2]);
                dat.str = int.Parse(fields[3]);
                dat.time_period = float.Parse(fields[4]);
                curEvent.Add(dat);
                if (timeMin == -1 || dat.time < timeMin) { timeMin = dat.time; }
                if (timeMax == -1 || dat.time > timeMax) { timeMax = dat.time; }
                if (signalMin == -1 || dat.signal < signalMin) { signalMin = dat.signal; }
                if (signalMax == -1 || dat.signal > signalMax) { signalMax = dat.signal; }
            }
            catch (System.Exception e)
            {
                 Debug.Log(e);
                Debug.Log(line);

               Debug.Log(line.Split(','));
            }
        }
        float dt = timeMax - timeMin + 0.001f; 
        double ds = signalMax - signalMin + 0.001;
        curEvent.Sort((x, y) => x.time.CompareTo(y.time));
        foreach (eventData ev in curEvent)
        {
            float tScale = (ev.time - timeMin) / (dt);
            int tspn= (int)(Mathf.RoundToInt(tScale * timeSpans));
            if (tspn >= timeSpans) tspn = timeSpans - 1;
            float sScale = (float)((ev.signal - signalMin) / (ds));
            if (!affBalls.ContainsKey(ev.str)) affBalls[ev.str] = new Dictionary<int, ballIntegratedData>();
            if (!affBalls[ev.str].ContainsKey(ev.dom))
            {
                ballIntegratedData nball = new ballIntegratedData();
                nball.affected = true;
                nball.stId = ev.str;
                nball.domId = ev.dom;
                nball.icharge = new double[timeSpans];
                for (int i = 0; i < timeSpans; i++)
                {
                    if (i < tspn)
                        nball.icharge[i] = 0;
                    else
                        nball.icharge[i] = (double)ev.signal;
                }
                nball.mintime = tScale;
                nball.maxtime = tScale;
                affBalls[ev.str][ev.dom] = nball;
            }
            else
            {
                ballIntegratedData nball = affBalls[ev.str][ev.dom];
                nball.maxtime = tScale;
                double incr = nball.icharge[tspn] + (double)ev.signal;
                for (int i = tspn; i < timeSpans; i++)
                {
                    nball.icharge[i] = incr;
                }
            }
          //  ballArray[ev.str].balls[ev.dom].setScale(sScale*20.0f+1.0f);
          //  Color clr = Color.Lerp(Color.red, Color.green,tScale); 
          //  ballArray[ev.str].balls[ev.dom].setColor(clr);
        }
        collectedBalls.Clear();
        foreach (KeyValuePair<int,Dictionary<int,ballIntegratedData>> kp in affBalls)
        {
            foreach(KeyValuePair<int,ballIntegratedData> bd in kp.Value)
            {
                collectedBalls.Add(bd.Value);
            }
        }
        foreach(singleBallList lst in ballArray)
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
        fullEventData ed = new fullEventData();
        ed.eventName = "Kloppo";
        ed.ballData = collectedBalls;
        ed.isteps = timeSpans;
        CompositeResolver.RegisterAndSetAsDefault(
            GeneratedResolver.Instance,
            StandardResolverAllowPrivate.Instance,
MessagePack.Unity.UnityResolver.Instance,
BuiltinResolver.Instance,
AttributeFormatterResolver.Instance,
// replace enum resolver
DynamicEnumAsStringResolver.Instance,
DynamicGenericResolver.Instance,
PrimitiveObjectResolver.Instance,
// final fallback(last priority)
StandardResolver.Instance);
        var bytes = MessagePackSerializer.Serialize(ed);
        FileStream file = File.Create(Application.persistentDataPath + "/hydra.sdt");
        file.Write(bytes,0,bytes.Length);
        file.Close();
        // size = scale * ( 0.2 * accum ) ** power
        updateToSet();
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
        if(t<=0.5f) return Color.Lerp(Color.blue, Color.green, t*2.0f);
        return Color.Lerp(Color.green, Color.red, (t-0.5f) * 2.0f);
    }
    float prevSld = 0.0f;
    List<ballIntegratedData> beforeBalls = null;
    public void updateToSet()
    {
        float nsld = 0.0f;
        if (slid != null)
        {
            nsld = slid.value;
        }
        if (nsld == prevSld &&beforeBalls!=null) return;
        if(beforeBalls!=null)
        {
            foreach(ballIntegratedData id in beforeBalls)
            {
                ballArray[id.stId].balls[id.domId].setScale(baseScale);
                ballArray[id.stId].balls[id.domId].setColor(new Color(0.5f, 0.5f, 0.5f));

            }
        }
         
        List<ballIntegratedData> afterBalls = new List<ballIntegratedData>();
        float start = nsld;
        float end = nsld + tgap;
        if (end > 1.0f) end = 1.0f;
        foreach(ballIntegratedData ball in collectedBalls)
        {
           if(ball.ballAffectedInTs(start,end))
            {
                afterBalls.Add(ball);
                float tscl = 0.0f;
                double sig = ball.getInSpan(start, end, delay, ref tscl);
              
                double scale = (double) baseScale + (sscale * Math.Pow((0.2 * sig), power));
                ballArray[ball.stId].balls[ball.domId].setScale((float)scale);
                ballArray[ball.stId].balls[ball.domId].setColor(colorFromBasicMap(tscl));
            }
        }
        // public float delay = 0.01f;
        //public float tgap = 0.1f;
        beforeBalls = afterBalls;
}

    public void setBall(int sid, int did,singleBall sball)
    {
        registerDomId(sid, did);
        ballArray[sid].balls[did] = sball;
    }
    // Update is called once per frame
    void Update()
    {
        if(slid!=null)
        {
            updateToSet();
        }
        if(gapControl!=null&&gapControl.value!=tgap)
        {
            tgap = gapControl.value;
        }
    }
}
