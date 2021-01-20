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
using AudioSynthesis.Bank;
using AudioSynthesis.Bank.Patches;
using SimpleJSON;

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
[System.Serializable]
public class trackData :IEquatable<trackData>
{
    [Key(0)]
    public float azi_rad; //local azimuth
    [Key(1)]
    public float dec_rad; // star
    [Key(2)]
    public double mjd; //julian
    [Key(3)]
    public double ra_rad; //star
    [Key(4)]
    public float rec_t0; //initial point
    [Key(5)]
    public float rec_x;
    [Key(6)]
    public float rec_y;
    [Key(7)]
    public float rec_z;
    [Key(8)]
    public float zen_rad; //local 
    [Key(9)]
    public float angle_err_50;
    [Key(10)]
    public float angle_err_90;


    public bool Equals(trackData other)
    {
        if (this == null && other == null) return true;
        if (other == null) return false;
        if (azi_rad != other.azi_rad) return false;
        if (dec_rad != other.dec_rad) return false;
        if (mjd != other.mjd) return false;
        if (ra_rad != other.ra_rad) return false;
        if (rec_t0 != other.rec_t0) return false;
        if (rec_x != other.rec_x) return false;
        if (rec_y != other.rec_y) return false;
        if (rec_z != other.rec_z) return false;
        if (zen_rad != other.zen_rad) return false;
        if (angle_err_50 != other.angle_err_50) return false;
        if (angle_err_90 != other.angle_err_90) return false;

        return true;
    }
    public trackData()
    {
        angle_err_50 = -1;
        angle_err_90 = -1;

    }
    public static trackData getFromArray(JSONNode arr)
    {
        trackData ret = new trackData();
        if (arr.Count < 9) return null;
        ret.azi_rad = (float)arr[0];
        ret.dec_rad = (float)arr[1];
        ret.mjd = (double)arr[2];
        ret.ra_rad = (double)arr[3];
        ret.rec_t0 = (float)arr[4];
        ret.rec_x = (float)arr[5];
        ret.rec_y = (float)arr[6];
        ret.rec_z = (float)arr[7];
        ret.zen_rad = (float)arr[8];
        if(arr.Count>=11)
        {
            ret.angle_err_50 = (float)arr[9];
            ret.angle_err_90 = (float)arr[10];
        }
        return ret;   
    }
    public static trackData getFromObject(JSONNode obj)
        {
        trackData ret = new trackData();
        if (obj.HasKey("azi_rad"))
            ret.azi_rad = (float)obj["azi_rad"];
        if (obj.HasKey("dec_rad"))
            ret.dec_rad = (float)obj["dec_rad"];

        if (obj.HasKey("mjd"))
            ret.mjd = (double)obj["mjd"];
        if (obj.HasKey("ra_rad"))
            ret.ra_rad = (double)obj["ra_rad"];
        if (obj.HasKey("rec_t0"))
            ret.rec_t0 = (float)obj["rec_t0"];
        if (obj.HasKey("rec_x"))
            ret.rec_x = (float)obj["rec_x"];
        if (obj.HasKey("rec_y"))
            ret.rec_y = (float)obj["rec_y"];
        if (obj.HasKey("rec_z"))
            ret.rec_z = (float)obj["rec_z"];
        if (obj.HasKey("zen_rad"))
            ret.zen_rad = (float)obj["zen_rad"];
        if (obj.HasKey("angle_err_50"))
            ret.angle_err_50 = (float)obj["angle_err_50"];
        if (obj.HasKey("angle_err_90"))
            ret.angle_err_90 = (float)obj["angle_err_90"];

        return ret;
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
    [Key(6)]
    public trackData track;
}

[Serializable]
public class forceTrack
{
    public KeyValuePair<int, int> eid;
    public trackData frc;
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
    TimeSlider playbackController;
    Slider zoomControl;
    Slider hairControl;
    GameObject hairCover=null;
    Toggle towerControl;
    Toggle czillaControl;
    const int timeSpans = 2500;
    public Transform footPoint;
    public Transform footAlign;
    List<eventData> curEvent = new List<eventData>();
    [SerializeField] TextAssetStream bankSource;
    [SerializeField]
    public singleBallList[] ballArray = null;//new int[5][];  
    [SerializeField]
    public SingleString[] stringArray = null;
    // Dictionary<int, Dictionary<int, ballIntegratedData>> affBalls = new Dictionary<int, Dictionary<int, ballIntegratedData>>();
    //List<ballIntegratedData> collectedBalls = new List<ballIntegratedData>();
    PatchBank bank;
    public float ni = 1.33f;
    public forceTrack ftrack = new forceTrack();
    public RefPoint ref1;
    public RefPoint ref2;
    public RefPoint ref3;
    public RefPoint ref4;
    public GameObject cone;
    public GameObject pole; //length:10*z, point: 000
    public GameObject earth;
    public GameObject epole;
    public GameObject indicator;
    public GameObject fluxball;
    public GameObject tower;
    public GameObject czilla;
    public TrackMeshMaker trackmesh;
   
    void Start()
    {
        ftrack.eid = new KeyValuePair<int, int>(132974, 67924813);
        ftrack.frc = new trackData();
        ftrack.frc.azi_rad = 3.00769365471949f;
        ftrack.frc.dec_rad = 0.016708671281222f;
        ftrack.frc.mjd = 58714.7322249641;
        ftrack.frc.ra_rad = 2.59716876080088;
        ftrack.frc.rec_t0 = 10983.2956250753f;
        ftrack.frc.rec_x = -83.0575967774025f;
        ftrack.frc.rec_y = 93.7020767723102f;
        ftrack.frc.rec_z = -46.6655356299309f;
        ftrack.frc.zen_rad = 1.5858330048395f;


                                                
curEvent = new List<eventData>();
        GameObject timeCon = GameObject.Find("timeController");
        if (timeCon!=null)
        {
            timeController = timeCon.GetComponent<Slider>();
            playbackController = timeCon.GetComponent<TimeSlider>();
            if(playbackController!=null)
            {
                playbackController.OnStartPlaying.AddListener(delegate { StartedPlaying(); });
                playbackController.OnStopPlaying.AddListener(delegate { StoppedPlaying(); });
            }
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
        GameObject hCon = GameObject.Find("hairController");
        if (hCon != null)
        {
            hairControl = hCon.GetComponent<Slider>();
            hairCover = hCon.transform.parent.gameObject;
            if (hairControl != null)
            {
                hairControl.onValueChanged.AddListener(delegate { hairChange(); });
                hairChange();
            }
        }

        GameObject tCon = GameObject.Find("towerToggle");
        if (tCon != null)
        {
            towerControl = tCon.GetComponent<Toggle>();
            if (towerControl != null)
            {
                towerControl.onValueChanged.AddListener(delegate { towerChange(); });
            }
        }
        GameObject czCon = GameObject.Find("zillaToggle");
        if (czCon != null)
        {
            czillaControl = czCon.GetComponent<Toggle>();
            if (czillaControl != null)
            {
                czillaControl.onValueChanged.AddListener(delegate { czillaChange(); });
            }
        }
        foreach (singleBallList lst in ballArray)
        {
            foreach(singleBall bl in lst.balls)
            {
                if (bl != null)
                {
                    bl.setColor(new Color(0.9f, 0.9f, 0.9f));
                    bl.setScale(baseScale);
                }
            }
        }

        bank = new PatchBank(bankSource);
        if(bank==null)
        {
            Debug.LogFormat("Couldn't load music bank: {0}", bankSource);
        }
        else
        {
            int maxPatch = 0;
            Patch[] arr = bank.GetBank(0);
            while (maxPatch<arr.Length)
            {
                if (arr[maxPatch] == null) break;
                maxPatch++;
            }
            for(int i=0;i<stringArray.Length;i++)
            {
                if(stringArray[i]!=null)
                {
                    //Debug.Log(ballArray[i].balls.Length);
                    stringArray[i].SetupWithBank(bank, i, maxPatch, 20, 85, ballArray[i+1].balls.Length, stringArray.Length);
                }
            }
        }
        // size = scale * ( 0.2 * accum ) ** power
        updateToSet(true);
        updateTrackRot();
    }
    void Awake()
    {
        Utilz.currentEventUpdated += updateForce;
        PlaceMultipleObjectsOnPlane.FingerSliding += SlideRotate;
        Debug.Log("Nyanya");
       
    }
    public void  SlideRotate(Vector2 vct)
    {
        if(fluxball!=null)
        {
            float scrn = Mathf.Min(Screen.width, Screen.height);
            vct /= scrn;
            Vector3 angles = fluxball.transform.localEulerAngles;
            if (Mathf.Abs(vct.x)>0.01f)
            {
               
                angles.y+=120 * vct.x;
            }
             if(Mathf.Abs(vct.y)>0.01f)
            {
               
                angles.z += 120 * vct.y;
            }
            fluxball.transform.localEulerAngles = angles;
        }
    }
    void StartedPlaying()
    {

    }
    void StoppedPlaying()
    {
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (stringArray[i] != null)
            {
                stringArray[i].StopPlaying();
            }
        }
    }
    void OnDestroy()
    {
        Utilz.currentEventUpdated -= updateForce;
        PlaceMultipleObjectsOnPlane.FingerSliding -= SlideRotate;
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
    void updateTrackRot()
    {
        if (EventRestAPI.Instance == null) return;
        if (EventRestAPI.Instance.currentEvent == null) return;
        if (EventRestAPI.Instance.currentEvent.track==null&& EventRestAPI.Instance.currentEvent.description.track!=null)
        {
            EventRestAPI.Instance.currentEvent.track = EventRestAPI.Instance.currentEvent.description.track;
        }
        if (EventRestAPI.Instance.currentEvent.track != null)
        {
            trackData dat = EventRestAPI.Instance.currentEvent.track;

            float dsin = Mathf.Sin(dat.zen_rad);
            float dcos = Mathf.Cos(dat.zen_rad);
            float rsin = Mathf.Sin(-dat.azi_rad);
            float rcos = Mathf.Cos(-dat.azi_rad);
            //Debug.LogFormat("ds:{0} dc:{1} rs:{2} rc:{3}",dsin,dcos,rsin,rcos);
            //////////////////////////////x/////////z//////////y/////
            Vector3 forw = new Vector3(dcos * rsin, -dcos * rcos, dsin);
            //earth-relative direction, i think ^^

            //south pole: 0 -2; 0;
            /// celestial sphere: radius 5 local
            float erad = 2.0f;
            float crad = 1.0f / earth.transform.localScale.x; //7.0f;
           // Debug.LogFormat("LocalScale: {0} ",crad);
            Vector3 spole = new Vector3(0, -erad, 0);
            float a = forw.sqrMagnitude;
            float b = 2 * (spole.x * forw.x + spole.y * forw.y + spole.z * forw.z);
            float c = spole.sqrMagnitude - crad * crad;
            float det = Mathf.Sqrt(b * b - 4 * a * c);
            float sol = (-b + det) / (2 * a);
            Vector3 from = spole + forw * sol;

            float cdsin = Mathf.Sin(dat.dec_rad);
            float cdcos = Mathf.Cos(dat.dec_rad);
            float crsin = Mathf.Sin((float)dat.ra_rad);
            float crcos = Mathf.Cos((float)dat.ra_rad);
            Vector3 ft = new Vector3(from.x, from.z, from.y);
            //dcos*rcos,dcos*rsin,dsin);
            Vector3 to = (new Vector3(cdcos * crcos, cdcos * crsin, cdsin)).normalized * crad;
            {
                // earth.gameObject.transform.localRotation = Quaternion.FromToRotation(from.normalized, to.normalized);
                Quaternion q;
                Vector3 a1 = Vector3.Cross(to.normalized, from.normalized);
                q.x = a1.x;
                q.y = a1.y;
                q.z = a1.z;
                q.w = 1 + Vector3.Dot(to.normalized, from.normalized);

            }
            epole.transform.localPosition = spole;
            epole.transform.localRotation = Quaternion.LookRotation(forw);
            indicator.transform.localRotation = Quaternion.LookRotation(from.normalized);
            indicator.transform.localScale = new Vector3(0.05f, 0.05f, from.magnitude / 10f);
            epole.transform.localScale = new Vector3(0.05f, 0.05f, sol / 10f);
            // Debug.LogFormat("Sol: {0}",sol);
            // Debug.LogFormat("From: {0}", from);
            // Debug.LogFormat("res: {0}", a *sol*sol+b*sol+c);
            // Debug.Log(forw);
            // Debug.Log(to);
            RotateAnim an = earth.GetComponent<RotateAnim>();
            Quaternion erot = Quaternion.FromToRotation(from.normalized, to.normalized);
            if (an != null)
            {
                an.SetRotation(erot);
            }
            else
            {
                earth.gameObject.transform.localRotation = erot;
            }
            // Debug.Log("^^^^^^");
        }
        else
        {
            if (epole != null)
            {
                epole.transform.localPosition = Vector3.zero;
                epole.transform.localRotation = Quaternion.identity;
                epole.transform.localScale = new Vector3(0.05f, 0.05f, 0.01f);
            }
            RotateAnim an = earth.GetComponent<RotateAnim>();
            Quaternion erot = Quaternion.identity;
            if (an != null)
            {
                an.SetRotation(erot);
            }
            else
            {
                earth.gameObject.transform.localRotation = erot;
            }
        }
    }
    public void updateForce() //event was updated... 
    {
        KeyValuePair<float, float> rng = getDefaultRange();
        Debug.LogFormat("Got range {0} {1}",rng.Key,rng.Value);
        timeSpan.SetValueWithoutNotify(rng.Key);
        timeSpan.SetValue2WithoutNotify(rng.Value);

        //if (timeController.value < rng.Key) timeController.SetValueWithoutNotify(rng.Key);
        //if (timeController.value > rng.Value) timeController.SetValueWithoutNotify(rng.Value);
        timeController.SetValueWithoutNotify(rng.Key);
        updateToSet(true);
        updateTrackRot();


    }
    public static KeyValuePair<float, float> getDefaultRange()
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
                ballArray[id.stId].balls[id.domId].setColor(new Color(0.9f, 0.9f, 0.9f));
                if (force) ballArray[id.stId].balls[id.domId].resetSig();

            }
        }
         
        List<ballIntegratedData> afterBalls = new List<ballIntegratedData>();
        float start = timeSpan.value;//  
        float end = timeSpan.value2;//
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
                    float dsig=ballArray[ball.stId].balls[ball.domId].setAndCompareSig((float)sig);

                    if (dsig>0&&playbackController!=null&&playbackController.IsPlaying)
                    {
                        //NoteSelector inst = NoteSelector.GetMainInstance();
                        // Debug.LogFormat("Sig :{0}", sig);
                        //if(inst)
                        //{
                        //   inst.PlayNote(ballArray[ball.stId].balls[ball.domId].transform.localPosition,(float) sig, 0.12f);
                        // }
                        int str = (int)(sig * 40);
                        if (str < 20) str = 20;
                        if (str > 70) str = 70;

                        stringArray[ball.stId-1].HitBall(ball.domId, str);
                    }
                    double scale = (double)baseScale + (EventRestAPI.settings.scaleMul * Math.Pow((0.2 * sig), EventRestAPI.settings.scalePower));
                    ballArray[ball.stId].balls[ball.domId].setScale((float)scale);
                    ballArray[ball.stId].balls[ball.domId].setColor(colorFromBasicMap(tscl));
                }
                else
                {
                    ballArray[ball.stId].balls[ball.domId].resetSig();
                    ballArray[ball.stId].balls[ball.domId].setScale(baseScale);
                    ballArray[ball.stId].balls[ball.domId].setColor(new Color(0.9f, 0.9f, 0.9f));
                }
            }
        }
        // public float delay = 0.01f;
        //public float tgap = 0.1f;
        beforeBalls = afterBalls;
        //track
        trackData track = null;
        if (EventRestAPI.Instance.currentEvent.track == null)
        {
            EventRestAPI.Instance.currentEvent.track = EventRestAPI.Instance.currentEvent.description.track;
        }
        if (EventRestAPI.Instance.currentEvent.track!=null)
        {
            track = EventRestAPI.Instance.currentEvent.track;

        }
        else
        {
       

            if (ftrack!=null&&EventRestAPI.Instance.currentEvent.description.run==ftrack.eid.Key&&
                EventRestAPI.Instance.currentEvent.description.evn == ftrack.eid.Value)
            {
                track = ftrack.frc;
                EventRestAPI.Instance.currentEvent.track=track;
                //EventRestAPI.Instance.currentEvent.sav
                //   Debug.Log("Forcing track ");
            }
        }
        if (force)
        {
            Debug.Log("Force updating");
        }
        float t1 = EventRestAPI.Instance.currentEvent.maxPureTime;
        float t0 = EventRestAPI.Instance.currentEvent.minPureTime;
        if (t0 > t1)
        {
            float tt = t0;
            t0 = t1; t1 = tt;
        }
        float atime = nsld * (t1 - t0) + t0;
        var cdesc = EventRestAPI.Instance.currentEvent.description;
        var ceid = new KeyValuePair<long, long>(cdesc.run, cdesc.evn);
        if (track==null||track.rec_t0>atime+0.001f || EventRestAPI.Instance.eventTrackIsSuppressed(ceid))
        {
            pole.SetActive(false);
            cone.SetActive(false);
        }
        else
        {
            pole.SetActive(true);
            cone.SetActive(true);
        }
        if (track!=null)
        {
            if(ref1==null||ref2==null||ref3==null||ref4==null)
            {
                Debug.Log("Need reference points");
                return;
            }
            if(cone==null||pole==null)
            {
                Debug.Log("Need cone and pole");
                return;
            }
            if(trackmesh!=null)
            {
                if(EventRestAPI.Instance.currentEvent!=null)
                {
                    var desc = EventRestAPI.Instance.currentEvent.description;
                    var eid = new KeyValuePair<long, long>(desc.run,desc.evn);
                    if(EventRestAPI.Instance.eventHasSim(eid))
                    {
                        if (hairControl != null)
                        {
                            hairCover?.SetActive(true);
                        }
                    }
                    else
                    {
                        if (hairControl != null)
                        {
                            hairCover?.SetActive(false);
                        }
                    }
                    

                }
            }
           // Debug.Log(atime);
            Debug.LogFormat("Trackt0 {0} atime: {1}",track.rec_t0,atime);
            if (track.rec_t0 > atime)
            {
                trackmesh?.Inactivate();
                return;
            }
            Vector3 tdir = new Vector3(Mathf.Sin(track.zen_rad)*Mathf.Cos(track.azi_rad), Mathf.Sin(track.zen_rad) * Mathf.Sin(track.azi_rad), Mathf.Cos(track.zen_rad));
            float vel = 0.299792458f;/// ni;
            float dt = atime - track.rec_t0;
            Vector3 ice_offs= -tdir * vel * dt;
            Vector3 ice_pos = new Vector3(track.rec_x, track.rec_y, track.rec_z)+ice_offs;
            if(trackmesh!=null)
            {
                var desc = EventRestAPI.Instance.currentEvent.description;
                MultimeshObject mesh = EventRestAPI.Instance.DemandMMesh(desc);
                Debug.LogFormat("DMesh {0}", mesh);
                if (mesh!=null)
                {
                    trackmesh.UseMesh((int)desc.run, (int)desc.evn,mesh);
                  
                }
                else
                {
                    trackmesh.Inactivate();
                    
                }
               //trackmesh.MaybeRecreateMesh((int)desc.run, (int)desc.evn, new Vector3(track.rec_x, track.rec_y, track.rec_z), -tdir/tdir.magnitude, 5000, 10);
               //trackmesh.MaybeRecreateProperMesh((int)desc.run, (int)desc.evn, new Vector3(track.rec_x, track.rec_y, track.rec_z), -tdir / tdir.magnitude, 5000, 10, track.rec_t0);
                Debug.LogFormat("Tdir {0}",tdir);
            }
            //ref1.gameObject.transform.localPosition;
            Matrix4x4 trans=new Matrix4x4();
            Vector3 c1 = ref1.gameObject.transform.localPosition;
            Vector3 c2 = ref2.gameObject.transform.localPosition;
            Vector3 c3 = ref3.gameObject.transform.localPosition;
            Vector3 c4 = ref4.gameObject.transform.localPosition;
            trans.SetColumn(0, new Vector4(c1.x, c1.y, c1.z, 1));
            trans.SetColumn(1, new Vector4(c2.x, c2.y, c2.z, 1));
            trans.SetColumn(2, new Vector4(c3.x, c3.y, c3.z, 1));
            trans.SetColumn(3, new Vector4(c4.x, c4.y, c4.z, 1));
            Matrix4x4 ice = new Matrix4x4();

            Vector3 i1 = ref1.icecube_pt;
            Vector3 i2 = ref2.icecube_pt;
            Vector3 i3 = ref3.icecube_pt;
            Vector3 i4 = ref4.icecube_pt;
            ice.SetColumn(0, new Vector4(i1.x, i1.y, i1.z, 1));
            ice.SetColumn(1, new Vector4(i2.x, i2.y, i2.z, 1));
            ice.SetColumn(2, new Vector4(i3.x, i3.y, i3.z, 1));
            ice.SetColumn(3, new Vector4(i4.x, i4.y, i4.z, 1));

            Matrix4x4 inv = trans*ice.inverse;
            Vector3 un_pos0=inv.MultiplyPoint3x4(new Vector3(track.rec_x, track.rec_y, track.rec_z));
            if(trackmesh!=null)
            {
                trackmesh.gameObject.transform.localPosition = un_pos0;
                float stime = start * (t1 - t0) + t0;
                float etime = end * (t1 - t0) + t0;
                trackmesh.SetTime(atime- track.rec_t0);
                trackmesh.SetTimeScale((etime - track.rec_t0));
                // Vector3 un = inv.MultiplyPoint3x4(new Vector3(track.rec_x, track.rec_y, track.rec_z-1)); lhs = m_Matrix.GetColumn(2),
               /* var lhs = inv.GetColumn(2);
                var rhs = inv.GetColumn(1);
                Quaternion rtt = Quaternion.identity;
                if (lhs == Vector4.zero && rhs == Vector4.zero)
                    rtt= Quaternion.identity;
                else
                    rtt= Quaternion.LookRotation(lhs, rhs);
                trackmesh.gameObject.transform.localRotation = rtt;// Quaternion.LookRotation(un_pos0 - un); 
                trackmesh.gameObject.transform.localScale = new Vector3(0.1f,0.1f,0.1f);*/
               // Debug.LogFormat("Setting meshtimescale: {0} {1}", inv, (t1 - track.rec_t0));
            }
            Vector3 un_pos1 = inv.MultiplyPoint3x4(ice_pos);
            Quaternion rot=Quaternion.LookRotation(un_pos0-un_pos1);
            pole.transform.localRotation = rot;
            cone.transform.localRotation = rot;
            float sclen = (un_pos0 - un_pos1).magnitude;
            pole.transform.localScale = new Vector3(1, 1, sclen / 10);
            pole.transform.localPosition =  un_pos1;
            cone.transform.localPosition = un_pos1;

        }
        else
        {
            if (hairControl != null)
            {
                hairCover?.SetActive(false);
                hairCover?.SetActive(false);
            }
            if (trackmesh != null)
            {
                trackmesh.Inactivate();
            }
        }
    }
    public void ValueChange()
    {
        updateToSet();
    }
    public void towerChange()
    {
        if (towerControl!=null&&tower!=null)
        {
            tower.SetActive(towerControl.isOn);
        }
    }
    public void czillaChange()
    {
        if (czillaControl != null && czilla != null)
        {
            czilla.SetActive(czillaControl.isOn);
        }
    }
    public void hairChange()
    {
        if (trackmesh!=null&&hairControl!=null&&trackmesh.HasMesh())
        {
            trackmesh.SetTimeWidth(hairControl.value); 
        }
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
