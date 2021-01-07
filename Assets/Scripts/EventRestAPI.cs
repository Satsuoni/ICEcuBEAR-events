using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using MessagePack;
using System.IO;
using MessagePack.Resolvers;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using evId = System.Collections.Generic.KeyValuePair<long,long>;
using SharpCompress.Writers;
using SharpCompress.Common;
using SharpCompress.Archives.GZip;
using SharpCompress.Compressors.Deflate;
using Unity.Jobs;
using Unity.Collections;
using System.Threading;

public delegate void EventListUpdatedHandler();
public delegate void CurrentEventChangedHandler();
public class Utilz
{
    public static event EventListUpdatedHandler eventListUpdated;
    public static event CurrentEventChangedHandler currentEventUpdated;
    public static void UpdateCurEvent()
    {
        if(currentEventUpdated!=null)
        currentEventUpdated();
    }
    public static void UpdateEventList()
    {
        if(eventListUpdated!=null)
        eventListUpdated();
    }
    public static string GenerateSHA256(byte[] bytes)
    {



        byte[] fileBytes = bytes;
        StringBuilder sb = new StringBuilder();

        using (SHA256Managed sha256 = new SHA256Managed())
        {
            byte[] hash = sha256.ComputeHash(fileBytes);
            foreach (Byte b in hash)
                sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
      
    }
}
[Serializable,MessagePackObject]
public class eventDesc :IEquatable<eventDesc>
{
    [Key(0)]
    public Int64 run;
    [Key(1)]
    public Int64 evn;
    [Key(2)]
    public string baseDesc;
    [Key(3)]
    public double energy;
    [Key(4)]
    public string eventDate;
    [Key(5)]
    public string humName = null;
    [Key(6)]
    public trackData track = null;
    [Key(7)]
    public string comment = null;

    public bool Equals(eventDesc other)
    {
        if (other == null) return false;
        if (run != other.run) return false;
        if (evn != other.evn) return false;
        if (baseDesc != other.baseDesc) return false;
        if (energy != other.energy) return false;
        if (eventDate != other.eventDate) return false;
        if (humName != other.humName) return false;
        if (comment != other.comment) return false;
        if (track == null && other.track != null) return false;
        if (track != null && other.track == null) return false;
        if(track!=null&&other.track!=null)
        {
            if (!track.Equals(other.track)) return false;
        }
        return true;
    }
    public bool tryLoadFromNode(JSONNode node)
    {
        if (node.IsArray)
            return this.tryLoadFromArray(node);
        if (node.IsObject)
            return this.tryLoadingFromObject(node);
        return false;
    }
    
    public bool tryLoadFromArray(JSONNode arr)
    {
        if (arr.Count < 3) return false;
        try
        {
            baseDesc = (string)arr[2];
            evn = (Int64)arr[1];
            run = (Int64)arr[0];

            if (arr.Count > 3)
            {
                energy = arr[3];
                eventDate = arr[4];
            }
            if (arr.Count > 5)
            {
                humName = arr[5];
            }
            if(arr.Count>6)
            {
                if (arr[6].IsArray)
                {
                    track = trackData.getFromArray(arr[6]);
                }
                else if (arr[6].IsObject)
                {

                    track = trackData.getFromObject(arr[6]);
                }
            }
            if (arr.Count > 7)
            {
                comment = arr[7];
            }
        }
        catch(Exception )
        {

            return false;
        }
        return true;
    }
    public bool tryLoadingFromObject(JSONNode obj)
    {

        if (obj.HasKey("run"))
            run = (int)obj["run"];
        else return false;
        if (obj.HasKey("event"))
            evn = (int)obj["event"];
        else return false;
        if (obj.HasKey("alert_type"))
            baseDesc = (string)obj["alert_type"];
        if (obj.HasKey("e_nu"))
            energy = (double)obj["e_nu"];
        if (obj.HasKey("event_time"))
            eventDate = (string)obj["event_time"];
        if (obj.HasKey("nickname"))
            humName = (string)obj["nickname"];
        else humName = null;
        if (obj.HasKey("comment"))
            comment = (string)obj["comment"];
        else comment = null;

        if (obj.HasKey("track"))
        {
            if (obj["track"].IsObject)
                track = trackData.getFromObject(obj["track"]);
            else
                if (obj["track"].IsArray)
            {
                track = trackData.getFromArray(obj["track"]);
            }
            else
                track = null;
        }
        else
        {
            if(obj.HasKey("ra_rad"))
            {
                track = trackData.getFromObject(obj);
            }
        }
                  
            
        return true; 
    }
    public static Regex getProposedCSVMatch()
    {
        return new Regex("(\\d+?)_(\\d+?).csv");
    }
    public static eventDesc getFromCsvName(Match m)
    {
        eventDesc n = new eventDesc();
        n.run = Int64.Parse(m.Groups[0].Value);
        n.evn = Int64.Parse(m.Groups[1].Value);
        n.baseDesc = "Recovered";
        n.energy = 1;
        n.eventDate = "unknown";

        return n;
    }
    public string csvName()
    {
        return string.Format("{0}_{1}.csv",run,evn);
    }
    public Int32 Compare(eventDesc y)
    {
        if (run < y.run) return -1;
        if (run > y.run) return 1;
        if (evn > y.run) return 1;
        if (evn < y.run) return -1;
        return 0;
    }
}

public class ThreadedJob
{
     private bool m_IsDone = false;
private object m_Handle = new object();
    protected static object m_Lock = new object();
   // private System.Threading.Thread m_Thread = null;
public bool IsDone
{
    get
    {
        bool tmp;
        lock (m_Handle)
        {
            tmp = m_IsDone;
        }
        return tmp;
    }
    set
    {
        lock (m_Handle)
        {
            m_IsDone = value;
        }
    }
}

public virtual void Start()
{
        // m_Thread = new System.Threading.Thread(Run);
        // m_Thread.Start();
        ThreadPool.QueueUserWorkItem(Run);
}
public virtual void Abort()
{
    //m_Thread.Abort();
}

protected virtual void ThreadFunction() { }

protected virtual void OnFinished() { }

public virtual bool Update()
{
    if (IsDone)
    {
        OnFinished();
        return true;
    }
    return false;
}
public IEnumerator WaitFor()
{
    while (!Update())
    {
        yield return null;
    }
}
private void Run(object _meh)
{
    ThreadFunction();
    IsDone = true;
}
 }

public class AwaitedAction
{
    private static readonly object locker = new object();
    private static Dictionary<evId, HashSet<Type>> runningTasks = new Dictionary<evId, HashSet<Type>>();
    ManualResetEvent ev =new ManualResetEvent(false);
    evId myId;
    public evId Id
    {
        get { return myId; }
    }
    public AwaitedAction(evId id)
    {
        
        myId = id;
    }
    public bool IsDone()
    {
        return ev.WaitOne(0);
    }
    protected virtual void ThreadFunction() { }
    private void Run(object state)
    {
        try
        {
            ThreadFunction();
        }
        finally
        {
            ev.Set();
        }
    }
    public virtual void Start()
    {
        ThreadPool.QueueUserWorkItem(Run);
    }

}

public class LoadMeshfile : ThreadedJob
{
    public string fname;
    public byte[] loaded;
    public MultimeshObject OutData = null;
    public bool outSuccess = false;

    protected override void ThreadFunction()
    {
      
        if (fname!=null&&loaded==null)
        {
            if (File.Exists(fname))
            {
                Debug.LogFormat("Read ing meshfile {0}", fname);
                loaded= File.ReadAllBytes(fname);
                Debug.LogFormat("Read {0} bytes from meshfile", loaded.Length);

            }
            else
            {
                outSuccess = false;
                return;
            }
        }
        if(loaded==null)
        {
            outSuccess = false;
            return;
        }
        OutData= new MultimeshObject();
        if (!OutData.LoadFromZippedBytes(loaded))
        {
            if(fname!=null)
            {
                Debug.LogFormat("Removing corrupt meshfile: {0}", fname);
                File.Delete(fname);
                outSuccess = false;
                return;
            }
        }  
            Debug.LogFormat("Processed meshfile {0}", OutData.meshes.Count);
        outSuccess = true;

    }
}
public enum Wactions
{
    Update,
    Load,

}
public class ReentranceGuard : IDisposable
{
    static object locked = new object();
    static HashSet<string> _labels=new HashSet<string>();
    string mylabel;
    bool _failedMiserably = false;
    public static implicit operator bool(ReentranceGuard obj)
    {
        return !obj._failedMiserably;
    }
    public ReentranceGuard(string label)
    {
        if (_labels.Contains(label))
        {
            //   throw new ArgumentException("Reentry", label);
            mylabel = label;
            _failedMiserably = true;
        }
        lock (locked)
        {
            mylabel = label;
            _labels.Add(mylabel);
        }
    }
    void IDisposable.Dispose() {
        lock (locked)
        {
            if(!_failedMiserably)
            _labels.Remove(mylabel);
        }

        }
}
public class ReActionGuard : IDisposable
{
    static object locked = new object();
    static Dictionary<evId, HashSet<string>> _actions = new Dictionary<evId, HashSet<string>>();
    string mylabel;
    evId myId;
    bool _failedMiserably = false;
    public static implicit operator bool(ReActionGuard obj)
    {
        return !obj._failedMiserably;
    }
    public ReActionGuard(evId id, string label)
    {
        HashSet<string> outp;
        if (_actions.TryGetValue(id,out outp))
        {
            if (outp.Contains(label))
            {
                _failedMiserably = true;
            }
        }
        lock (locked)
        {
            mylabel = label;
            myId = id;
            if (_actions.TryGetValue(id, out outp))
            {
                outp.Add(label);
            }
            else
            {
                _actions[myId] = new HashSet<string>();
                _actions[myId].Add(label);
            }
        }
    }
    void IDisposable.Dispose()
    {
        lock (locked)
        {
            if (!_failedMiserably)
            {
                _actions[myId].Remove(mylabel);
            }
        }

    }
}

public class SimulateMillipede : ThreadedJob
{
    public string persistPath;
    public String InData;  // arbitary job data -millipede csv
    public MultimeshObject OutData = null;
    public eventDesc eDesc;
    public trackData track;
    public bool saveMeshfile=false;
    public struct burstEntry
    {
        public double energy;
        public Vector3 coords;
        public int index;
        public double time;
    }

    protected override void ThreadFunction()
    {
        if (eDesc == null)
        {
            eDesc = new eventDesc();
            eDesc.baseDesc = "placeholder";
            eDesc.run = 100;
            eDesc.evn = 100;
            eDesc.energy = 100;
        }
        burstEntry[] bursts = null;
        string[] fLines = null;
        if (InData.Contains("\r\n"))
        {
            fLines = Regex.Split(InData, "\r\n");
        }
        else
        {
            fLines = Regex.Split(InData, "\n|\r");

        }
        double energyMin = -1f, energyMax = -1f;
        bool skipHeader = true;
        List<burstEntry> lst = new List<burstEntry>();
        foreach (var line in fLines)
        {
            if (skipHeader)
            {
                skipHeader = false;
                continue;
            }
            try
            {
                string[] fields = line.Split(',');
                burstEntry dat = new burstEntry();
                dat.index = int.Parse(fields[0]);
                dat.energy = double.Parse(fields[1]);
                dat.time = double.Parse(fields[2]);
                dat.coords.x = float.Parse(fields[3]);
                dat.coords.y = float.Parse(fields[4]);
                dat.coords.z = float.Parse(fields[5]);
                lst.Add(dat);
                if (energyMax == -1 || energyMax < dat.energy) energyMax = dat.energy;
                if (energyMin == -1 || energyMin > dat.energy)
                    if(dat.energy != 0) energyMin = dat.energy;
            }
            catch (Exception)
            {
                //Debug.Log(e);
                Debug.Log(line);

                //Debug.Log(line.Split(','));
            }
        }
        bursts = lst.ToArray();
        energyMax = Math.Sqrt(energyMax-energyMin);
        //energyMin = 0;
        if(track==null)
        {
            //create fake track XD
            track = new trackData();
            track.azi_rad = 0;
            track.zen_rad = 0;
            if(bursts.Length>0)
            {
                track.rec_x = bursts[0].coords.x;
                track.rec_y = bursts[0].coords.y;
                track.rec_z = bursts[0].coords.z;
                track.rec_t0= (float)bursts[0].time;
            }
        }
        Vector3 tdir = new Vector3(Mathf.Sin(track.zen_rad) * Mathf.Cos(track.azi_rad), Mathf.Sin(track.zen_rad) * Mathf.Sin(track.azi_rad), Mathf.Cos(track.zen_rad));
        float vel = 0.299792458f;/// ni;
        // float dt = atime - track.rec_t0;
        //Vector3 ice_offs = -tdir * vel * dt;
        //  Vector3 ice_pos = new Vector3(track.rec_x, track.rec_y, track.rec_z) + ice_offs;
        Vector3 initialPos = new Vector3(track.rec_x, track.rec_y, track.rec_z);
        Vector3 heading = -tdir / tdir.magnitude;
        float duration=5000; float emrate;
        float itime = track.rec_t0;
        int cntnz = 0;
        double accum = 0;
        for (int i = 0; i < bursts.Length; i++)
        {
            if (bursts[i].energy <= 0.00001) continue;
            cntnz++;
            accum += Math.Sqrt(bursts[i].energy - energyMin);
        }
        if (accum < 1) accum = 1;
        CascadeData[] cdat = new CascadeData[cntnz];
        int j = 0;
        // float maxEnergy = 0;
        int minPhotons = 25;
        int totalExtraPhotons = cntnz * minPhotons * 2;
        if (totalExtraPhotons > 3000)
        {
            totalExtraPhotons = 3000;
            minPhotons = totalExtraPhotons / (2 * cntnz);
        }
        Debug.LogFormat("Total photons: {0} {1}", totalExtraPhotons*1.5, cntnz);
        for (int i = 0; i < bursts.Length; i++)
        {
            if (bursts[i].energy <= 0.00001) continue;
            double encf = Math.Sqrt(bursts[i].energy - energyMin)* totalExtraPhotons / accum + minPhotons;
            int en = (int)(encf);
           // if (en > 5000) en = 5000;
            cdat[j] = new CascadeData(heading, 1.0f, (int)en, (float)bursts[i].time - itime);
            cdat[j].location = bursts[i].coords;
            j++;
        }
        //new CascadeData(heading, 1.0f, 9500);
        //6000 / maxEnergy

        MuonTrack tr = new MuonTrack(initialPos, heading, duration, 0.299792458f, 1, cdat);//emrate
        OutData = tr.Simulate();
        string fname = String.Format("{0}_{1}_trail.gz", eDesc.run,eDesc.evn);
        Debug.LogFormat("Simulated, saving..: {0} ", fname);
        if (saveMeshfile)
           OutData.SaveToFile(persistPath + "/" +fname);
    }
}
    public class ProcessEventCsv : ThreadedJob
{
    public int timeSpans = 2500;
    public string persistPath;
    public bool saveIntegrated;
    public bool saveCsv;
    public String InData;  // arbitary job data
    public fullEventData OutData=null; // arbitary job data
    public eventDesc eDesc;
    public string hashName;
    public string csvName;
    public string csvHash;
    public string eMessage = null;


    List<eventData> curEvent = new List<eventData>();
    Dictionary<int, Dictionary<int, ballIntegratedData>> affBalls = new Dictionary<int, Dictionary<int, ballIntegratedData>>();
    List<ballIntegratedData> collectedBalls = new List<ballIntegratedData>();
   

    protected override void ThreadFunction()
    {
        if(eDesc==null)
        {
            eDesc = new eventDesc();
            eDesc.baseDesc = "placeholder";
            eDesc.run = 100;
            eDesc.evn = 100;
            eDesc.energy = 100;
        }
        // Do your threaded task. DON'T use the Unity API here
        if (saveCsv)
        {
            lock (m_Lock)
            {
                csvName = eDesc.csvName();
                File.WriteAllText(persistPath + "/" + csvName, InData);
                csvHash = Utilz.GenerateSHA256(Encoding.UTF8.GetBytes(InData));
            }
        }
        string[] fLines = null;
        if (InData.Contains("\r\n"))
        {
            fLines = System.Text.RegularExpressions.Regex.Split(InData, "\r\n");
        }
        else
        {
            fLines = System.Text.RegularExpressions.Regex.Split(InData, "\n|\r");

        }
        float timeMin = -1f, timeMax = -1f;
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
               // dat.time_period = float.Parse(fields[4]);
                curEvent.Add(dat);
                if (timeMin == -1 || dat.time < timeMin) { timeMin = dat.time; }
                if (timeMax == -1 || dat.time > timeMax) { timeMax = dat.time; }
                if (signalMin == -1 || dat.signal < signalMin) { signalMin = dat.signal; }
                if (signalMax == -1 || dat.signal > signalMax) { signalMax = dat.signal; }
            }
            catch (System.Exception )
            {
                //Debug.Log(e);
                //Debug.Log(line);

                //Debug.Log(line.Split(','));
            }
        }
        if (timeSpans > curEvent.Count/2)
        {
            timeSpans = (int)(curEvent.Count / 2);
        }
        if (timeSpans < 2) timeSpans = 2;
        float dt = timeMax - timeMin + 0.001f;
        double ds = signalMax - signalMin + 0.001;
        curEvent.Sort((x, y) => x.time.CompareTo(y.time));
        foreach (eventData ev in curEvent)
        {
            float tScale = (ev.time - timeMin) / (dt);
            int tspn = (int)(Mathf.RoundToInt(tScale * timeSpans));
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
     
        }
        collectedBalls.Clear();
        
        foreach (KeyValuePair<int, Dictionary<int, ballIntegratedData>> kp in affBalls)
        {
            foreach (KeyValuePair<int, ballIntegratedData> bd in kp.Value)
            {
                collectedBalls.Add(bd.Value);
            }
        }
  
        fullEventData ed = new fullEventData();
        ed.minPureTime = timeMin;
        ed.maxPureTime = timeMax;
        ed.description = eDesc;

        if(eDesc.humName!=null)
            ed.eventName = eDesc.humName;
           else
        ed.eventName = eDesc.baseDesc;
        ed.ballData = collectedBalls;
        ed.isteps = timeSpans;
        try
        {
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
        }
        catch (Exception )
        {

        }
        if (saveIntegrated)
        {
            var bytes = MessagePackSerializer.Serialize(ed);
            hashName = string.Format("{0}.sdt", Utilz.GenerateSHA256(bytes));
            lock (m_Lock)
            {
                string filename = string.Format("{0}/{1}", persistPath, hashName);
                if (File.Exists(filename))
                {
                    eMessage = "Event file already exists... Overwriting";
                }
                FileStream file = File.Create(filename);
                file.Write(bytes, 0, bytes.Length);
                file.Close();
            }
        }
        OutData = ed;
    }
    protected override void OnFinished()
    {
        // This is executed by the Unity main thread when the job is finished
      
    }
}

[Serializable,MessagePackObject(keyAsPropertyName:true)]
public class SavedEventData
{
    public eventDesc description;
    public string hashname;
    public string csvName;
    public string csvHash; //hash of csv file 
    public int integrationSteps;
    public string status;// maybe enum is better but a hassle
    public string comment;
    public bool Corrupt()
    {
    
        if (description == null) return true;
    
        if (description.evn < 0) return true;
      
        if (description.run < 0) return true;
        if (integrationSteps < 2) return true;
        if (csvName!=null && !csvName.EndsWith("csv")) return true;
        if (hashname!=null && !hashname.EndsWith("sdt")) return true;
        if (description.energy < 0) return true;
        if (csvName != null && csvHash == null) return true; //needs to be hashed
        if (csvHash != null && csvName == null) return true;//shouldn't be
        return false;
        
    }
}
[Serializable,MessagePackObject(keyAsPropertyName:true)]
public class SavedEventsSettings
{
    public UInt32 numberIntegrated =20; //number of preintegrated files to keep
    public UInt32 numberKeptAsCsv = 60; //number of csv files to keep
    public List<SavedEventData> eventData=new List<SavedEventData>();
    public bool CheckForCorrectness() //very basic check
    {
        if (numberIntegrated < 1 || numberIntegrated > 200) return false;
        if (numberKeptAsCsv < 1 || numberKeptAsCsv > 500) return false;
        if (eventData == null) return false;
        // just remove corrupted savefile nodes
        List<SavedEventData> todel = new List<SavedEventData>();
        foreach(SavedEventData dat in eventData)
        {
            if(dat.Corrupt())
            {
                todel.Add(dat);
            }
        }
        foreach (SavedEventData dat in todel)
        {
            eventData.Remove(dat);
        }
        if (animationSpeed < 0.01f) animationSpeed = 0.01f;
        if (animationSpeed > 1f) animationSpeed = 0.2f;
        if (scalePower < 0.04f) scalePower = 0.15f;
        if (scalePower > 0.3f) scalePower = 0.15f;
        if (scaleMul < 1) scaleMul = 2f;
        if (scaleMul > 20) scaleMul = 5f;
        return true;
    }
    public float animationSpeed=0.1f;
    public float scalePower=0.15f;
    public float scaleMul=2.0f;
    public void deleteCache()
    {
        string pth = Application.persistentDataPath+"/";
        if (eventData == null) return;
        foreach(SavedEventData sd in eventData)
        {
           if(sd.csvName!=null)
            {
                try
                {
                    File.Delete(pth + sd.csvName);
                    sd.csvName = null;
                    sd.csvHash = null;
                }
                catch
                {

                }
            }
            if (sd.hashname != null)
            {
                try
                {
                    File.Delete(pth + sd.hashname);
                    sd.hashname = null;
                }
                catch
                {

                }
            }
        }
        //if they are deleted, no point in displaying them?
        eventData.Clear();
    }
}


[Serializable]
public class eventList
{
    public List<List<string>> events;
}

public class PrimCache<id,t>
{
    UInt32 maxItems = 50;
    public void setMaxItems(UInt32 mi)
    {
        maxItems = mi;
    }
    Dictionary<id, t> index = new Dictionary<id, t>();
    Queue<id> order = new Queue<id>();
    public t GetFirstData()
    {
        if (order == null || order.Count == 0) return default(t);
        return index[order.Peek()];

    }
    public bool checkCache(id idn)
    {
        return index.ContainsKey(idn);
    }
    public bool pushItem(id idt, t itm)
    {
        if(index.ContainsKey(idt)&&index[idt]!=null)
        {
            return order.Count > maxItems;
        }
        index[idt] = itm;
        order.Enqueue(idt);
        return order.Count > maxItems;
    }
    public void Prune()
    {
        while(order.Count>maxItems)
        {
            id idt = order.Dequeue();
            index.Remove(idt);
        }
    }
    public void Clear()
    {
        index.Clear();
        order.Clear();
    }
    public t getItem(id idt)
    {
        t ret;
        if(!index.TryGetValue(idt,out ret))
        {
            return default(t);
        }
         return ret;
    }
}

public class LifecycleState
{
  public bool mainLifecycle;
  public string curStatus;
  public string task;
  public int counter;
  public int primaryCount;
  public int secondaryCount;
    public LifecycleState()
    {
        mainLifecycle = false;
        curStatus = "Waiting";
        task = "";
        counter = 0;
        primaryCount = 0;
        secondaryCount = 0;
    }
}
public class ExtraSimData
{
    public MultimeshObject mesh;
}


public class EventRestAPI : MonoBehaviour
{

    static EventRestAPI _Instance=null;
    public static EventRestAPI Instance
    {
        get { return _Instance; }
    }
    public static SavedEventsSettings settings = null;
    Dictionary<evId,SavedEventData> savedIndex = new Dictionary<evId, SavedEventData>();
    PrimCache<evId, fullEventData> cache = new PrimCache<evId, fullEventData>();
    PrimCache<evId, ExtraSimData> simcache = new PrimCache<evId, ExtraSimData>();
    public static volatile bool isDropdownReady = false;
    public static List<eventDesc> notificationsWoken = new List<eventDesc>();
    const string saveFileName = "savedData.json";
    evId expectedNextEvent = new evId(-1,-1);
    bool _lifecycleReady = false;
    LifecycleState loaderData = new LifecycleState();
    public bool lifecycleReady { get { return _lifecycleReady; } }
    // Start is called before the first frame update
    void Awake()
    {
        if (_Instance != null) Destroy(gameObject);
        else
            _Instance = this;
        simcache= new PrimCache<evId, ExtraSimData>();
        simcache.setMaxItems(10);
        // string fullName = Application.persistentDataPath + "/" + "test.gz";
        //  string fsb = Application.persistentDataPath + "/" + "savedData.json";
        //   string lsb = Application.persistentDataPath + "/" + "sssavedData.json";
        /*  using (FileStream originalFileStream = File.OpenRead(fsb))
          {
              using (Stream stream = File.OpenWrite(fullName))
              using (var writer = new GZipStream(stream, SharpCompress.Compressors.CompressionMode.Compress, SharpCompress.Compressors.Deflate.CompressionLevel.BestCompression))
              {
                  byte[] bt = Encoding.ASCII.GetBytes("hello there");
                  // writer.Write(bt, 0, bt.Length);
                  originalFileStream.CopyTo(writer);
                  // bt.CopyTo(writer);
              }
          }
          using (Stream stream = File.OpenRead(fullName))
          {
              using (var reader = new GZipStream(stream, SharpCompress.Compressors.CompressionMode.Decompress))
              {
                  using (Stream outf = File.OpenWrite(lsb))
                  { reader.CopyTo(outf); }

              }
              }*/

    }
    void OnDestroy()
    {
        _Instance = null;
    }
    public void deleteEventsData()
    {
        settings.deleteCache();
        savedIndex.Clear();
        saveSettings();
        Utilz.UpdateEventList();
    }
    public void UpdateLoading()
    {
        if (loaderData.primaryCount > loaderData.counter)
        {
            loaderData.primaryCount = loaderData.counter;

        }
        if (loaderData.secondaryCount > loaderData.counter)
        {
            loaderData.secondaryCount = loaderData.counter;

        }
        if (loading!=null)
        {
            if (loaderData.task==""||(loaderData.primaryCount==loaderData.counter&& loaderData.secondaryCount == loaderData.counter))
            {
                loading.gameObject.SetActive(false);
                return;
            }
            else
            {
                loading.gameObject.SetActive(true);
            }
            if (loaderData.counter == 0)
            {
                loading.SetOuter(1);
                loading.SetInner(1);
            }
            else
            {
                loading.SetOuter((float)loaderData.primaryCount / (float)loaderData.counter);
                loading.SetInner((float)loaderData.secondaryCount / (float)loaderData.counter);
            }
            loading.SetCenter(loaderData.primaryCount, loaderData.counter);
            if (loaderData.mainLifecycle)
            {
                loading.SetTitle(string.Format("Initializing: {0}\n{1}", loaderData.curStatus, loaderData.task));
            }
            else
            {
                loading.SetTitle(string.Format("{0}\n{1}", loaderData.curStatus,loaderData.task));
            }
        }
    }
    void Start()
    {
        
        //  string strng= "{\"events\": [[132431, 65009599, \"gfu - gold\"]]}";
        // var el = JSON.Parse(strng);
        try
        {
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
        }
        catch (Exception)
        {

        }

        savedIndex = new Dictionary<evId, SavedEventData>();
        isDropdownReady = false;
        _lifecycleReady = false;
        loaderData = new LifecycleState();
        StartCoroutine(Lifecycle());
       
    }
    bool savingDelaySetting=false;
    IEnumerator saveWithDelay()
    {
        if (savingDelaySetting) yield break;
        savingDelaySetting = true;
        yield return new WaitForSeconds(3.0f);
        saveSettings();
        savingDelaySetting = false;
    }
    public void updatedCsvNum(float num)
    {
        settings.numberKeptAsCsv =(UInt32) Mathf.RoundToInt(num);
        if (settings.numberKeptAsCsv < 10) settings.numberKeptAsCsv = 10;
        StartCoroutine(saveWithDelay());
    }
    public void updatedSDTNum(float num)
    {
        settings.numberIntegrated = (UInt32)Mathf.RoundToInt(num);
        if (settings.numberIntegrated < 1) settings.numberIntegrated = 1;
        StartCoroutine(saveWithDelay());
    }
    public void updatedScalePower(float num)
    {
        settings.scalePower = num;
        StartCoroutine(saveWithDelay());
    }

    public void updatedAnim(float num)
    {
        settings.animationSpeed = num;
        if (settings.animationSpeed < 0.01f) settings.animationSpeed = 0.01f;
        StartCoroutine(saveWithDelay());
    }

    public void updatedScaleMul(float num)
    {
        settings.scaleMul = num;
        StartCoroutine(saveWithDelay());
    }

    void saveSettings()
    {
        if (settings == null) return;
        string savetxt = MessagePackSerializer.ToJson<SavedEventsSettings>(settings);// JsonUtility.ToJson(settings);
        string fullSaveName = Application.persistentDataPath + "/" + saveFileName;
        try
        {
            File.WriteAllText(fullSaveName, savetxt);
        }
        catch (Exception e)
        {
            Debug.LogFormat("Couldn't save settings: {0}",e);

        }
        

    }
    IEnumerator repairSave() //more like remake really
    {
        settings = new SavedEventsSettings();
        settings.eventData = new List<SavedEventData>();
        string sourceDir = Application.persistentDataPath;
        Regex csvform = eventDesc.getProposedCSVMatch();
        foreach (string currentFile in Directory.EnumerateFiles(sourceDir, "*.csv"))
        {
            string fileName = currentFile.Substring(sourceDir.Length + 1);
            Match m = csvform.Match(fileName);
            if (!m.Success)
            {
                Debug.Log(string.Format("Removing extraneous csv file {0}", fileName));
                File.Delete(currentFile);
                continue;
            }
            try
            {
                SavedEventData s = new SavedEventData();
                s.description = eventDesc.getFromCsvName(m);
                s.status = "Datafile recovered";

                s.csvName = fileName;
                byte[] csv = File.ReadAllBytes(currentFile);
                s.csvHash = Utilz.GenerateSHA256(csv);
                s.hashname = null;
                settings.eventData.Add(s);
                savedIndex[new KeyValuePair<long, long>(s.description.run, s.description.evn)] = s;
            }
            catch (Exception)
            {
                continue;
            }
            yield return null; //pause
        }


        foreach (string currentFile in Directory.EnumerateFiles(sourceDir, "*.sdt"))
        {
            string fileName = currentFile.Substring(sourceDir.Length + 1);
            if (fileName.Length != 68)
            {
                Debug.LogFormat("Filename {0} is of invalid length {1}, PURGE", fileName, fileName.Length);
                File.Delete(currentFile);
                continue;
            }
            byte[] dat = null;
            try
            {
                dat = File.ReadAllBytes(currentFile);
                fullEventData fl = MessagePackSerializer.Deserialize<fullEventData>(dat);
                if(fl.description==null)
                {
                    continue;
                }

                KeyValuePair<long, long> ptr = new KeyValuePair<long, long>(fl.description.run, fl.description.evn);
                SavedEventData sdat = null;
                if(savedIndex.ContainsKey(ptr))
                {
                    sdat = savedIndex[ptr];
                }
                else
                {
                    sdat = new SavedEventData();
                    sdat.csvHash = null;
                    sdat.csvName = null;
                    settings.eventData.Add(sdat);
                    savedIndex[ptr] = sdat;
                }
                sdat.status = "SDT recovered";
                sdat.description = fl.description;
                sdat.hashname = fileName;

            }
            catch(Exception e)
            {
                Debug.LogFormat("Error reading/parsing {0} :{1}",currentFile,e);
            }
            yield return null; //we are not in a hurry, I hope.  
        }
        saveSettings();
    }
    bool tryLoadingSave(string fullSaveName)
    {
        
        if (!File.Exists(fullSaveName)) return false;
        string filedata = null;
        try
        {
            filedata= File.ReadAllText(fullSaveName, Encoding.UTF8);
            Debug.Log(filedata);
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Error reading {0}: {1}",fullSaveName,e));
            return false;
        }
        if (filedata == null) return false;
        if (filedata.Length <= 2) return false;
        try
        {
            settings=MessagePackSerializer.Deserialize< SavedEventsSettings >( MessagePackSerializer.FromJson(filedata));
         }
        catch (Exception e)
        {
            Debug.Log(string.Format("Error parsing settings:  {0}", e));
            return false;
        }
        if (settings == null) return false;
        if (!settings.CheckForCorrectness()) return false;


        return true;
    }
    public static string mainURL = "https://ar.obolus.net";
    public static string eventCounter = "nevents";
    public static string lastEvents = "lasteventswithtracks";
    public static string lastEventsBefore = "lasteventsbeforeidwithtracks";
    public static string efile = "eventfile";
    public static string commentsUrl = "comment";
    public LoadingBar loading;
    string _curComment=null;
    public string currentComment
    {
        get { return _curComment; }
    }
    fullEventData _currentEvent =null;
    public fullEventData currentEvent
    {
        get { return _currentEvent; }
    }
    //primitive lock
    /*HashSet<evId> __locks = new HashSet<evId>();
    bool _lock(evId id)
    {
        if(__locks==null) __locks = new HashSet<evId>();
        if (__locks.Contains(id)) return false;
        __locks.Add(id);
        return true;
    }
    void _unlock(evId id)
    {
        if (__locks == null) __locks = new HashSet<evId>();
        if (__locks.Contains(id)) __locks.Remove(id);
    }*/
    public MultimeshObject DemandMMesh(eventDesc ev )
    {
        evId evid = new evId(ev.run, ev.evn);
        if (!eventHasSim(evid)) return null;
        if (simcache.checkCache(evid))
        {
            ExtraSimData s = simcache.getItem(evid);
            return s.mesh;
        }
        Debug.LogFormat("Event {0} {1} not found in simcache!",evid.Key,evid.Value);
        StartCoroutine(simulateSingleEvent(ev));
        return null;
    }
    public bool eventHasSim(evId ev)
    {
        HardcodedEventData dat = HardcodedEvents.instance.GetHardcoded(ev);
        if (dat == null) return false;
        if (dat.meshFile == null && dat.millipedeFile == null) return false;
        return true;
    }
    IEnumerator simulateSingleEvent(eventDesc ev, bool saveMeshfile = true)
    {
        if (ev == null) yield break;
        string dir = Application.persistentDataPath + "/";
        evId evid = new evId(ev.run, ev.evn);
        using (ReActionGuard r = new ReActionGuard(evid,"resimulate"))
        {
            if (!r)
            {
                yield return StartCoroutine(AwaitUnlock(evid, "resimulate"));
                yield break;
            }
            if (!eventHasSim(evid)) yield break;

            if (simcache.checkCache(evid))
            {
                ExtraSimData s = simcache.getItem(evid);
                if (s.mesh != null)
                { yield break; }

            }
            // currently, only built-in Millipede! TODO...
            HardcodedEventData dat = HardcodedEvents.instance.GetHardcoded(evid);
            if (dat == null)
            {

                yield break;
            }


            if (dat.meshFile != null)
            {
                ExtraSimData s = new ExtraSimData();
                LoadMeshfile job = new LoadMeshfile();
                job.fname = null;
                job.loaded = dat.meshFile.bytes;
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                if (!job.outSuccess)
                {
                    Debug.Log("Invalid hardcoded file!");

                    yield break;
                }
                s.mesh = job.OutData;
                simcache.pushItem(evid, s);

                yield break;
            }

            if (!savedIndex.ContainsKey(evid))
                yield return StartCoroutine(loadAndSaveSingleEvent(ev, false, true));
            string fname = Application.persistentDataPath + '/' + String.Format("{0}_{1}_trail.gz", ev.run, ev.evn);
            if (File.Exists(fname))
            {
                Debug.LogFormat("Reading meshfile {0}", fname);
                LoadMeshfile job = new LoadMeshfile();
                job.fname = fname;
                job.loaded = null;
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                if (!job.outSuccess)
                {

                   // yield return StartCoroutine(simulateSingleEvent(ev, saveMeshfile));
                    yield break;
                }
                ExtraSimData s = new ExtraSimData();
                s.mesh = job.OutData;
                simcache.pushItem(evid, s);
            }
            else
            {
                SimulateMillipede job = new SimulateMillipede();
                job.eDesc = ev;
                job.saveMeshfile = saveMeshfile;
                job.InData = dat.millipedeFile.text;
                job.persistPath = Application.persistentDataPath;
                job.OutData = null;
                job.track = ev.track;
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                ExtraSimData s = new ExtraSimData();
                s.mesh = job.OutData;
                simcache.pushItem(evid, s);
            }
        }
    }
    IEnumerator AwaitUnlock(evId evid,string label)
    {
        using (ReActionGuard r = new ReActionGuard(evid, label))
        {
            if (!r) yield return null;
        }
        yield break;
    }
    IEnumerator AwaitUnlock(string label)
    {
        using (ReentranceGuard r = new ReentranceGuard(label))
        {
            if (!r) yield return null;
        }
        yield break;
    }
    //preferably, only process event here to avoid races, etc;
    IEnumerator loadAndSaveSingleEvent(eventDesc ev, bool saveInt = true, bool saveCSV = true)
    {
        yield return null;
        if (ev == null) yield break;
        string dir = Application.persistentDataPath + "/";
        evId evid = new evId(ev.run, ev.evn);
        using (ReActionGuard r = new ReActionGuard(evid, "loadAndSaveSingleEvent"))
        {
            if(!r)
            {
                yield return StartCoroutine(AwaitUnlock(evid, "loadAndSaveSingleEvent"));
                yield break;
            }
            if (!loaderData.mainLifecycle)
            {
                loaderData.counter = 1;
            }
            if (cache.checkCache(evid))
            {
                loaderData.secondaryCount += 1;
                if (!loaderData.mainLifecycle)
                {
                    loaderData.primaryCount = 1;
                }
                //already have it, break;
                yield break;
            }
           
            SavedEventData sdat = null;
            loaderData.task = "Get data";
            UpdateLoading();
            HardcodedEventData hdat = HardcodedEvents.instance.GetHardcoded(evid);
            if (savedIndex.ContainsKey(evid))
            {
                loaderData.task = "Reload data";
                UpdateLoading();
                sdat = savedIndex[evid];
                if (sdat.hashname != null)
                {
                    string fhash = dir + sdat.hashname;
                    if (File.Exists(fhash))
                    {
                        try
                        {
                            byte[] dat = File.ReadAllBytes(fhash);
                            fullEventData fl = MessagePackSerializer.Deserialize<fullEventData>(dat);
                            if (fl.description.evn == ev.evn && fl.description.run == ev.run)
                            {
                                //load complete
                                cache.pushItem(evid, fl);
                                loaderData.secondaryCount += 1;
                                if (!loaderData.mainLifecycle)
                                {
                                    loaderData.primaryCount = 1;
                                    UpdateLoading();
                                }
                               
                                yield break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogFormat("Corrupt file? {0} {1}", sdat.hashname, e);
                        }
                        //couldn't load
                        File.Delete(fhash);

                    }
                    sdat.hashname = null;
                }
                if (sdat.csvName != null)
                {
                    string fcsv = dir + sdat.csvName;

                    if (File.Exists(fcsv) || (hdat != null && hdat.csvFile != null))
                    {
                        string csvText = null;
                        try
                        {
                            byte[] csvBytes;
                            if (hdat != null && hdat.csvFile != null)
                                csvBytes = hdat.csvFile.bytes;
                            else
                                csvBytes = File.ReadAllBytes(fcsv);
                            string hash = Utilz.GenerateSHA256(csvBytes);
                            if (sdat.csvHash == null || hash != sdat.csvHash)
                            { Debug.Log("Invalid csv hash"); csvText = null; }
                            else
                                csvText = System.Text.Encoding.UTF8.GetString(csvBytes);
                            loaderData.task = "Read datafile";
                            UpdateLoading();
                        }
                        catch (Exception e)
                        {
                            Debug.LogFormat("Corrupt csv? {0} {1}", sdat.csvName, e);
                            csvText = null;
                        }
                        if (csvText != null)
                        {
                            loaderData.task = "Process data";
                            ProcessEventCsv job = new ProcessEventCsv();
                            job.eDesc = ev;
                            job.eMessage = null;
                            job.InData = csvText;
                            job.saveIntegrated = saveInt;
                            job.saveCsv = false;
                            job.persistPath = Application.persistentDataPath;
                            UpdateLoading();
                            job.Start();
                            sdat.status = "Processing";
                            yield return StartCoroutine(job.WaitFor());
                            if (job.eMessage != null)
                            {
                                Debug.Log(job.eMessage);
                            }
                            if (job.OutData == null)
                            {
                                Debug.Log("Failed processing");
                                loaderData.task = "Failed processing";
                                UpdateLoading();
                            }
                            else
                            {
                                cache.pushItem(evid, job.OutData);
                                sdat.integrationSteps = job.timeSpans;
                              
                                loaderData.secondaryCount += 1;
                                if (!loaderData.mainLifecycle)
                                {
                                    loaderData.primaryCount = 1;
                                }
                                UpdateLoading();
                                yield break;
                            }
                        }
                    }
                    sdat.csvName = null;
                }
            }
            else
            {
                sdat = new SavedEventData();
                sdat.description = ev;
                sdat.csvName = null;
                sdat.csvHash = null;
                sdat.hashname = null;

            }
            string url = String.Format("{0}/{1}/{2}/{3}", mainURL, efile, ev.run, ev.evn);
            string csvdata = null;
            if (hdat != null && hdat.csvFile != null)
            {
                csvdata = hdat.csvFile.text;
            }
            else
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    // Request and wait for the desired page.
                    //webRequest.chunkedTransfer = false;
                    loaderData.task = "Load datafile";
                    UpdateLoading();
                    yield return webRequest.SendWebRequest();
                    if (webRequest.isNetworkError || webRequest.isHttpError)
                    {

                        Debug.Log("Error getting  " + url + " :" + webRequest.error);
                        sdat.status = "Failed download";
                        if (!savedIndex.ContainsKey(evid))
                        {
                            savedIndex[evid] = sdat;
                            settings.eventData.Add(sdat);
                            saveSettings();
                        }
                     
                        loaderData.task = "CSV: Network failure";
                        UpdateLoading();
                        yield break;
                    }
                    else
                    {
                        csvdata = webRequest.downloadHandler.text;
                    }
                }
            }
            if (csvdata == null)
            {
                loaderData.task = "CSV: Network failure";
                UpdateLoading();
                yield break;

            }
            loaderData.task = "Process datafile";
            loaderData.secondaryCount += 1;
            UpdateLoading();
            ProcessEventCsv hjob = new ProcessEventCsv();
            hjob.eDesc = ev;
            hjob.saveCsv = saveCSV;
            hjob.saveIntegrated = saveInt;
            hjob.eMessage = null;
            hjob.InData = csvdata;
            hjob.persistPath = Application.persistentDataPath;

            hjob.Start();
            yield return StartCoroutine(hjob.WaitFor());
            if (hjob.eMessage != null) { Debug.Log(hjob.eMessage); }
            if (hjob.OutData == null)
            {
                loaderData.task = "Failed processing";
                sdat.status = "Failed processing";
                if (!savedIndex.ContainsKey(evid))
                {
                    savedIndex[evid] = sdat;
                    settings.eventData.Add(sdat);
                    saveSettings();
                }
               
                UpdateLoading();
                yield break;
            }
            cache.pushItem(evid, hjob.OutData);
            sdat.hashname = hjob.hashName;
            sdat.csvHash = hjob.csvHash;
            sdat.csvName = hjob.csvName;
            sdat.integrationSteps = hjob.timeSpans;
            if (!savedIndex.ContainsKey(evid))
            {
                savedIndex[evid] = sdat;
                settings.eventData.Add(sdat);
                saveSettings();
            }
            if (!loaderData.mainLifecycle)
            {
                loaderData.primaryCount = 1;
                UpdateLoading();
            }
           
        }

    }
    IEnumerator loadNotifiedEvents()
    {
        loaderData.counter = notificationsWoken.Count;
        loaderData.primaryCount = 0;
        loaderData.secondaryCount = 0;
        loaderData.curStatus = "Notification";
        loaderData.task = "Load Event";
        UpdateLoading();
        yield return null;
        while ( notificationsWoken.Count>0) //amount can change while loading...
        {
            eventDesc toget = notificationsWoken[0];
            notificationsWoken.RemoveAt(0);
            yield return StartCoroutine(loadAndSaveSingleEvent(toget));
            expectedNextEvent = new evId(toget.run,toget.evn);
        }
        assignExpected();
    }
    public void reloadNotified()
    {
        StartCoroutine(loadNotifiedEvents());
    }
    IEnumerator GetEventNumber()
    {
        string url = String.Format("{0}/{1}", mainURL, eventCounter);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            //webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                gotEventNumber = false;
                Debug.Log("Error getting  " + url + " :" + webRequest.error + " --> " + webRequest.responseCode.ToString() + webRequest.downloadHandler.text);
                yield break;
            }
            else
            {
                JSONNode el = null;
                try
                {
                    el = JSON.Parse(webRequest.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    gotEventNumber = false;
                    yield break;
                }
                if (el["nevents"] == null)
                {
                    gotEventNumber = false;
                    yield break;
                }
                try
                {

                    nevents = (Int64)el["nevents"];
                    gotEventNumber = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    gotEventNumber = false;
                }

            }
        }
    }
    List<eventDesc>  lastEventList = new List<eventDesc>();
    IEnumerator GetLastEvents(Int64 delta)
    {
        lastEventList = new List<eventDesc>();
        string url = String.Format("{0}/{1}/{2}", mainURL, lastEvents, delta);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
           // webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                gotLastEvents = false;
                Debug.Log("Error getting  " + url + " :" + webRequest.error);
                yield break;
            }
            else
            {
                JSONNode el = null;
                try
                {
                    el = JSON.Parse(webRequest.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    gotLastEvents = false;
                    yield break;
                }
                if (el["events"] == null)
                {
                    gotLastEvents = false;
                    yield break;
                }
                try
                {
                    foreach (var eda in el["events"])
                    {
                        eventDesc ed = new eventDesc();
                       // Debug.Log(eda.Value);
                        if (ed.tryLoadFromNode(eda.Value))
                        {
                            lastEventList.Add(ed);
                        }
                      

                    }

                    gotLastEvents = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    gotLastEvents = false;

                }

            }
        }
    }
    bool gotEventNumber = false;
    Int64 nevents;
    bool gotLastEvents = false;

    public class crWrapper
    {
        int maxStarted;
        int finishedCount = 0;
        int startedCount = 0;
        List<IEnumerator> queued;
        public crWrapper(int mx)
        {
            maxStarted = mx;
            queued = new List<IEnumerator>();
        }
        public void Enqueue(IEnumerator e)
        {
            queued.Add(e);
        }
        IEnumerator crWrap(IEnumerator e, Action onDone) { yield return e; onDone(); }
        public IEnumerator Await(EventRestAPI outer)
        {
            outer.loaderData.counter = (int)queued.Count;
            int cnt = queued.Count;
            Action onDone = new Action(() => { finishedCount++; });
            while ( finishedCount < cnt)
            {
                while(startedCount<cnt&&startedCount-finishedCount<maxStarted)
                {
                    outer.StartCoroutine(crWrap(queued[startedCount], onDone));
                    startedCount++;
                }
                outer.loaderData.primaryCount = finishedCount;
                outer.loaderData.secondaryCount = startedCount;
                outer.UpdateLoading();
                yield return null;
            }
        }
    }
    IEnumerator crWrap(IEnumerator e, Action onStart,Action onDone) { onStart(); yield return e; onDone(); }
    public IEnumerator HardcodedUpdate()
    {
        using (ReentranceGuard r = new ReentranceGuard("hard_update"))
        {
            if (!r)
            {
                yield return StartCoroutine(AwaitUnlock("hard_update"));
                yield break;
            }
            loaderData.counter = (int)HardcodedEvents.instance.events.Length;
            loaderData.primaryCount = 0;
            loaderData.secondaryCount = 0;
            loaderData.curStatus = "Hardcoded events Update";
            loaderData.task = "Processing...";
            crWrapper wrap = new crWrapper(5);
            foreach (HardcodedEventData dat in HardcodedEvents.instance.events)
            {
                if (dat.csvFile != null)
                {
                    eventDesc dsc = new eventDesc();
                    dsc.baseDesc = dat.desc.baseDesc;
                    dsc.comment = dat.desc.comment;
                    dsc.energy = dat.desc.energy;
                    dsc.eventDate = dat.desc.eventDate;
                    dsc.evn = dat.evId;
                    dsc.humName = dat.desc.humName;
                    dsc.run = dat.runId;
                    wrap.Enqueue(loadAndSaveSingleEvent(dsc));
                }
            }
            yield return wrap.Await(this);
        }
    }
    public IEnumerator HardcodedSimulate()
    {
        loaderData.counter =1;
        loaderData.primaryCount = 0;
        loaderData.secondaryCount = 0;
        loaderData.curStatus = "Hardcoded events";
        loaderData.task = "Simulating";
        int simCnt = 0;
        foreach (HardcodedEventData dat in HardcodedEvents.instance.events)
        {
            if (dat.millipedeFile != null || dat.meshFile != null)
                simCnt++;
            
        }
        loaderData.counter = simCnt;
        Debug.LogFormat("Simulateling: {0}",simCnt);
        UpdateLoading();
        int ccnt = 0;
        crWrapper wrap = new crWrapper(2);
        foreach (HardcodedEventData dat in HardcodedEvents.instance.events)
        {
            if (dat.millipedeFile != null || dat.meshFile != null)

            {
                Debug.LogFormat("Simulstart: {0} {1}", dat.runId, dat.evId);
                ccnt++;
                loaderData.counter = simCnt;
                loaderData.primaryCount = ccnt-1;
                loaderData.secondaryCount = ccnt;
                loaderData.curStatus = "Hardcoded events";
                loaderData.task = "Simulating";
                UpdateLoading();
                evId evid = new evId(dat.runId,dat.evId);

                SavedEventData sd = null;// savedIndex[evid];
                eventDesc dsc = null;
                if(!savedIndex.TryGetValue(evid,out sd))
                {
                    dsc = new eventDesc();
                    dsc.baseDesc = dat.desc.baseDesc;
                    dsc.comment = dat.desc.comment;
                    dsc.energy = dat.desc.energy;
                    dsc.eventDate = dat.desc.eventDate;
                    dsc.evn = dat.evId;
                    dsc.humName = dat.desc.humName;
                    dsc.run = dat.runId;

                }
                else
                {
                    dsc = sd.description;
                  //  dsc.track = null;
                }
                wrap.Enqueue(simulateSingleEvent(dsc));
              //  Debug.LogFormat("Simulated: {0} {1}", dat.runId,dat.evId);
                loaderData.counter = simCnt;
              //  loaderData.primaryCount = ccnt;
               // loaderData.secondaryCount = ccnt;
                loaderData.curStatus = "Hardcoded events";
                loaderData.task = "Simulating";  
                UpdateLoading();
            }

        }

        yield return wrap.Await(this) ;
        

    }
    public IEnumerator OptimisticUpdate()
    {
        using (ReentranceGuard r = new ReentranceGuard("update"))
        {
            if(!r)
            {
                yield return StartCoroutine(AwaitUnlock("update"));
                yield break;
            }
            loaderData.counter = settings.eventData.Count > 0 ? settings.eventData.Count : 1;
            loaderData.primaryCount = 0;
            loaderData.secondaryCount = 0;
            loaderData.curStatus = "Optimistic Update";
            loaderData.task = "Update event count";
            UpdateLoading();

            int oldCount = settings.eventData.Count;
            gotEventNumber = false;
            yield return StartCoroutine(GetEventNumber());
            if (!gotEventNumber)
            {
                loaderData.curStatus = "Optimistic Update failed";
                yield break;
            }
            Debug.LogFormat("Have loaded events: {0}", nevents);
            int oldEventNum = settings.eventData.Count;
            if (nevents == settings.eventData.Count)
            {
                Debug.Log(string.Format("No new events detected, {0} currently", nevents));
                loaderData.primaryCount = settings.eventData.Count;
                loaderData.secondaryCount = settings.eventData.Count;
                UpdateLoading();
                yield break;
            }
            if (nevents < oldEventNum)
            {
                Debug.Log(string.Format("Events are shrinking?, {0} currently, {1} before", nevents, oldEventNum));
                loaderData.counter = 0;
                loaderData.primaryCount = 0;
                loaderData.secondaryCount = 0;
                UpdateLoading();
                yield break;
            }
            Int64 delta = nevents - oldEventNum;
            loaderData.counter = (int)delta;
            loaderData.primaryCount = 0;
            loaderData.secondaryCount = 0;
            loaderData.curStatus = "Optimistic Update";
            loaderData.task = "Get latest events";
            UpdateLoading();
            gotLastEvents = false;
            yield return StartCoroutine(GetLastEvents(delta));
            if (!gotLastEvents)
            {
                loaderData.task = "Failed getting events";
                UpdateLoading();
                yield break;
            }
            if (lastEventList.Count == 0)
            {
                loaderData.task = "No latest events";
                UpdateLoading();
                yield break;
            }
            Debug.LogFormat("Have loaded last events: {0} of {1}", lastEventList.Count, delta);
            if (lastEventList.Count != delta)
            {
                Debug.Log(string.Format("Got wrong number of events? Ah well. {0} expected, {1} got", delta, lastEventList.Count));
                loaderData.counter = (int)lastEventList.Count;
            }
            loaderData.curStatus = "Loading events";
            loaderData.task = "Get data";
            UpdateLoading();
            crWrapper wrap = new crWrapper(5);
            foreach (eventDesc dsc in lastEventList)
            {
                wrap.Enqueue(loadAndSaveSingleEvent(dsc));
               // _unlock(new evId(dsc.run, dsc.evn)); //just in case
            }
            yield return wrap.Await(this);
            loaderData.primaryCount = loaderData.counter;
            loaderData.secondaryCount = loaderData.counter;
            UpdateLoading();
            lastEventList.Clear();
            saveSettings();
            if (oldCount != settings.eventData.Count)
                Utilz.UpdateEventList();
            Debug.Log(string.Format("Done updating, cur events {0}", settings.eventData.Count));
            StartCoroutine(updateComments());
        }
    }
    bool gotLastBefore = false;
    IEnumerator GetLastEventsBefore(Int64 delta, Int64 runId, Int64 evd)
    {
        //inclusive
        gotLastBefore = false;
        lastEventList = new List<eventDesc>();
       // Debug.LogFormat("Getting last event before {0}/{1}/{2}/{3}", lastEventsBefore, delta, runId, evd);
        string url = String.Format("{0}/{1}/{2}/{3}/{4}", mainURL, lastEventsBefore, delta, runId, evd);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
           // webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                gotLastBefore = false;
                Debug.Log("Error getting  " + url + " :" + webRequest.error);
                yield break;
            }
            else
            {
                JSONNode el = null;
                try
                {
                    el = JSON.Parse(webRequest.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    gotLastBefore = false;
                    yield break;
                }
                if (el["events"] == null)
                {
                    gotLastBefore = false;
                    yield break;
                }
                try
                {
                    foreach (var eda in el["events"])
                    {
                        eventDesc ed = new eventDesc();


                        if (ed.tryLoadFromNode(eda.Value))
                        {
                            lastEventList.Add(ed);
                        }
                       

                    }
                    Debug.LogFormat("Got events... {0} {1}", el["events"].Count,lastEventList.Count);
                    gotLastBefore = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    gotLastBefore = false;

                }

            }
        }
    }
    void pruneFiles()
    {
        //prune sdt
        string pth = Application.persistentDataPath+"/";
        if (settings.eventData.Count < settings.numberIntegrated && settings.eventData.Count < settings.numberKeptAsCsv) return;
        if(settings.eventData.Count > settings.numberIntegrated)
        {
            int curcnt = 0;
            for(int i= settings.eventData.Count-1;i>=0;i--)
            {
            if(settings.eventData[i].hashname!=null)
                {
                    if(File.Exists(pth+ settings.eventData[i].hashname))
                    {
                        if(curcnt<settings.numberIntegrated)
                        {
                            curcnt++;
                        }
                        else
                        {
                            try
                            {
                                File.Delete(pth + settings.eventData[i].hashname);
                                settings.eventData[i].hashname = null;
                            }
                            catch(Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                        settings.eventData[i].hashname = null;
                    }
                }
            }
        }

        if (settings.eventData.Count > settings.numberKeptAsCsv)
        {
            int curcnt = 0;
            for (int i = settings.eventData.Count - 1; i >= 0; i--)
            {
                if (settings.eventData[i].hashname != null)
                {
                    if (File.Exists(pth + settings.eventData[i].csvName))
                    {
                        if (curcnt < settings.numberKeptAsCsv)
                        {
                            curcnt++;
                        }
                        else
                        {
                            try
                            {
                                File.Delete(pth + settings.eventData[i].csvName);
                                settings.eventData[i].csvName = null;
                                settings.eventData[i].csvHash = null;
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                        settings.eventData[i].csvName = null;
                        settings.eventData[i].csvHash = null;
                    }
                }
            }
        }
        saveSettings();
    }

    IEnumerator fillOutFiles()
    {
        //prune sdt
        string pth = Application.persistentDataPath + "/";
        int cntCsv = 0;
            int cntInt = 0;
        for (int i = settings.eventData.Count - 1; i >= 0; i--)
        {
            if (cntCsv >= settings.numberKeptAsCsv && cntInt >= settings.numberIntegrated) break;
            bool needProcessing = false;
            bool saveCsv = false;
            bool saveInt = false;
            if (settings.eventData[i].hashname != null)
            {
                if (File.Exists(pth + settings.eventData[i].hashname))
                {
                    cntInt++;
                }
                else
                {
                    settings.eventData[i].hashname = null;
                }
            }
            if (settings.eventData[i].hashname == null && cntInt < settings.numberIntegrated)
            { saveInt = true; needProcessing = true; }

            if (settings.eventData[i].csvName != null)
            {
                if (File.Exists(pth + settings.eventData[i].csvName))
                {

                    cntCsv++;
                }
                else
                {
                    settings.eventData[i].csvName = null;
                    settings.eventData[i].csvHash = null;
                }
            }
            if (settings.eventData[i].csvName == null && cntCsv < settings.numberKeptAsCsv)
            { saveCsv = true; needProcessing = true; }
            if (needProcessing)
            {
                yield return StartCoroutine(loadAndSaveSingleEvent(settings.eventData[i].description, saveInt, saveCsv));
            }

        }
        pruneFiles();
        saveSettings();
    }
    IEnumerator Lifecycle()
    {
        string fullSaveName = Application.persistentDataPath + "/" + saveFileName;
        string sourceDir = Application.persistentDataPath;
        while(HardcodedEvents.instance==null) //wait for hardcoded to load in... 
        {
            yield return null;
        }
        loaderData.mainLifecycle = true;
        loaderData.counter = 1;
        loaderData.curStatus = "";
        loaderData.task = "Load save data";
        UpdateLoading();
        if (!tryLoadingSave(fullSaveName))
        {
            Debug.Log("Could not read settings, repairing");
            loaderData.task = "Repair save data";
            UpdateLoading();
            yield return StartCoroutine(repairSave());
            if (!tryLoadingSave(fullSaveName))
            {
                Debug.Log("Could not repair settings!");
                loaderData.curStatus = "Failed loading settings";
                loaderData.task = "Reinstall";
                UpdateLoading();
                yield break;
            }
        }
        if (settings == null)
        {
            Debug.Log("Could not recover settings!");
            loaderData.curStatus = "Failed loading settings";
            loaderData.task = "Reinstall";
            UpdateLoading();
            yield break; //may crash after, but at that point, something is seriously off?
        }
        loaderData.counter = settings.eventData.Count;
        loaderData.primaryCount = 0;
        loaderData.secondaryCount = 0;
        loaderData.curStatus = "Reindexing";
        loaderData.task = "Process save data";
        UpdateLoading();
        yield return null;
        foreach (SavedEventData evd in settings.eventData) //reindex
        {
            savedIndex[new evId(evd.description.run, evd.description.evn)] = evd;
        }
        //3. Scan folder for files not mentioned in saved data. Remove them.
        HashSet<string> mentioned = new HashSet<string>();
        Debug.LogFormat("Cur events in settings {0}",settings.eventData.Count);
        foreach (SavedEventData evd in settings.eventData)
        {
            if (evd.csvName != null) mentioned.Add(evd.csvName);
            if (evd.hashname != null) mentioned.Add(evd.hashname);
            //Debug.LogFormat("Mentioned {0}", evd.csvName);
        }
        loaderData.curStatus = "Pruning";
        UpdateLoading();
        yield return null;
        foreach (string currentFile in Directory.EnumerateFiles(sourceDir, "*.csv"))
        {
            string fileName = currentFile.Substring(sourceDir.Length + 1);
            if(!mentioned.Contains(fileName))
            {
                Debug.LogFormat("Pruning {0}", fileName);
                File.Delete(currentFile);
            }
        }
        foreach (string currentFile in Directory.EnumerateFiles(sourceDir, "*.sdt"))
        {
            string fileName = currentFile.Substring(sourceDir.Length + 1);
            if (!mentioned.Contains(fileName))
            {
                Debug.LogFormat("Pruning {0}", fileName);
                File.Delete(currentFile);
            }
        }
        isDropdownReady = true;
        Utilz.UpdateEventList();
        //4. If woken up by notification tap - try getting event data if we don't have it in the list as coroutine...
        StartCoroutine(loadNotifiedEvents());
        // Run update on hardcoded events
        yield return StartCoroutine(HardcodedUpdate());
 
        //5. Run network update for eventnum we don't seem to have. Save up to set number of files from new ones.
        yield return StartCoroutine(OptimisticUpdate());
        yield return StartCoroutine(HardcodedSimulate());
        pruneFiles();
        cache.Prune();
        //6. Run a full network update (with paging) as coroutine to fill up gaps if any
        yield return StartCoroutine(runFullUpdate());
       
        //7.Download files and process files to fill up settings quota.
        _lifecycleReady = true;
  
        yield return StartCoroutine(fillOutFiles());
        
        //8. Prune excess files (save json after every step)  -done in above
        Debug.Log("Ending Lifecycle!");
        loaderData.task = "";
        loaderData.curStatus = "Complete";
        loaderData.mainLifecycle = false;
        if (expectedNextEvent.Key == -1)
        {
            fullEventData first = cache.GetFirstData();

            if (first != null)
            {
                expectedNextEvent = new evId(first.description.run, first.description.evn);
                Debug.LogFormat("Assigning first event {0}", first.description.run);
                assignExpected();
            }
            else
            {
                Debug.LogFormat("Not assigning first event");

            }
        }
    }

    public void demandFullUpdate()
    {
        if(isDropdownReady)
        {
            StartCoroutine(runFullUpdate());
        }
    }
    string comment = null;
    bool gotComment = false;
    IEnumerator getComment(evId evid)
    {
        gotComment = false;
        string url = String.Format("{0}/{1}/{2}/{3}", mainURL, commentsUrl,evid.Key,evid.Value);
        //Debug.LogFormat("Comment {0}/{1}/{2}/{3} ", mainURL, commentsUrl, evid.Key, evid.Value);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
           // webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log("Error getting  " + url + " :" + webRequest.error + " --> " + webRequest.responseCode.ToString() + webRequest.downloadHandler.text);
                yield break;
            }
            else
            {
                JSONNode el = null;
                try
                {
                    el = JSON.Parse(webRequest.downloadHandler.text);
                    
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    gotComment = false;
                    yield break;
                }
                
                if (el["comment"] == null)
                {
                    gotComment = true;
                    comment = null;
                    yield break;
                }
                try
                {

                    gotComment = true;
                    comment = el["comment"];
                    //Debug.LogFormat("Comments {0} ", comment);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    gotComment = false;
                }

            }
        }
    }
    public IEnumerator updateComments()
    {
        bool upd = false;
        loaderData.counter = settings.eventData.Count;
        loaderData.primaryCount = 0;
        loaderData.secondaryCount = 0;
        loaderData.curStatus = "Update comments";
        loaderData.task = "Check events";
        UpdateLoading();
        foreach (SavedEventData dat in settings.eventData)
        {
            evId evid = new evId(dat.description.run,dat.description.evn);
            if (dat.description.comment != null)
            {
                dat.comment = dat.description.comment;
            }
            else
            {
                yield return StartCoroutine(getComment(evid));
                if (gotComment == true && comment != null)
                {
                    dat.comment = (string)comment.Clone();
                    upd = true;
                }
            }
            loaderData.primaryCount += 1;
            loaderData.secondaryCount += 1;
            UpdateLoading();
        }
        if (upd) saveSettings();
    }
    public IEnumerator runFullUpdate()
    {
        using (ReentranceGuard r = new ReentranceGuard("update"))
        {
            if (!r)
            {
                yield return StartCoroutine(AwaitUnlock("update"));
                yield break;
            }
            gotLastEvents = false;
            int oldCount = settings.eventData.Count;

            loaderData.counter = 1;
            loaderData.primaryCount = 0;
            loaderData.secondaryCount = 0;
            loaderData.curStatus = "Full update";
            loaderData.task = "Enumerate API";
            UpdateLoading();

            yield return StartCoroutine(GetLastEvents(1));
            if (gotLastEvents && lastEventList.Count >= 1)
            {
                Debug.Log("running full update");
                long page = 10;
                HashSet<evId> updCheck = new HashSet<evId>();
                bool left = true;
                loaderData.counter = lastEventList.Count;
                loaderData.primaryCount = 0;
                loaderData.secondaryCount = 0;
                loaderData.curStatus = "Full update";
                loaderData.task = "Enumerate API";
                UpdateLoading();
                while (left)
                {
                    left = false;
                    if (lastEventList.Count == 0) break;
                    lastEventList.Sort((x, y) => { return x.Compare(y); });
                    bool upd = false;
                    loaderData.task = "Process event page";
                    foreach (eventDesc ev in lastEventList)
                    {
                        //  Debug.Log(ev.track);
                        evId eid = new evId(ev.run, ev.evn);
                        if (!updCheck.Contains(eid))
                        {
                            updCheck.Add(eid);
                            left = true;
                        }
                        if (!savedIndex.ContainsKey(eid))
                        {
                            SavedEventData dat = new SavedEventData();
                            dat.csvHash = null;
                            dat.csvName = null;
                            dat.description = ev;
                            dat.integrationSteps = 2;
                            dat.status = "Not loaded";
                            dat.hashname = null;
                            settings.eventData.Add(dat);
                            savedIndex[eid] = dat;
                            upd = true;
                        }
                        else
                        {
                            if ((ev.humName != null && savedIndex[eid].description.humName == null))
                            {
                                savedIndex[eid].description.humName = ev.humName;
                                upd = true;
                            }
                            if (!ev.Equals(savedIndex[eid].description))
                            {
                                //  Debug.LogFormat("Track: {0}", ev.track);
                                savedIndex[eid].description = ev;
                                upd = true;
                            }
                        }
                        loaderData.primaryCount += 1;
                        UpdateLoading();
                    }
                    if (upd)
                    {
                        settings.eventData.Sort((x, y) => { return x.description.Compare(y.description); });
                        saveSettings();
                    }
                    eventDesc curLastEv = lastEventList[0];
                    loaderData.task = "Read event page";
                    loaderData.primaryCount = 0;
                    loaderData.secondaryCount = 0;
                    loaderData.counter = 1;
                    UpdateLoading();
                    yield return StartCoroutine(GetLastEventsBefore(page, curLastEv.run, curLastEv.evn));
                    Debug.LogFormat("Got last before {0} {1}", gotLastBefore, lastEventList.Count);
                    if (!gotLastBefore) left = false;
                }
                loaderData.task = "Prune files";
                loaderData.primaryCount = 0;
                loaderData.secondaryCount = 0;
                loaderData.counter = 1;
                UpdateLoading();
                pruneFiles();
                settings.eventData.Sort((x, y) => { return x.description.Compare(y.description); });
                saveSettings();
            }
            if (oldCount != settings.eventData.Count)
                Utilz.UpdateEventList();
            StartCoroutine(updateComments());
        }
    }
    public trackData forcedTracks(evId id)
    {
        Debug.Log("Getting forced tracks");
        HardcodedEventData dat = HardcodedEvents.instance.GetHardcoded(id);
        if (dat == null) return null;
        if (dat.tracks.Length > 0)
        {
            Debug.Log("Got forced tracks");
            return dat.tracks[0];
        }
                return null;
  
    }
    void assignExpected()
    {
        Debug.Log("Assigning expected");
        if (expectedNextEvent.Key == -1) return;
        if (!cache.checkCache(expectedNextEvent)) return;
        fullEventData dat = cache.getItem(expectedNextEvent);
        if (dat.description.track!=null&&dat.track==null)
            {
            dat.track = dat.description.track;
        }
        if (dat == null) return;
        if(dat.track==null)
        {
            dat.track = forcedTracks(expectedNextEvent);
        }
        Debug.Log("Got cached");
        if (dat.description==null)
        {
            Debug.Log("Corrupt DATA");
            return;
        }
        if(_currentEvent==null)
        {
            _currentEvent = dat;
            SavedEventData edat;
            if(savedIndex.TryGetValue(expectedNextEvent,out edat))
            {
                _curComment = edat.comment;
                
            }
            Utilz.UpdateCurEvent();
            return;
        }
        if(_currentEvent.description.run!= dat.description.run|| _currentEvent.description.evn != dat.description.evn)
        {
            _currentEvent = dat;
            SavedEventData edat;
            if (savedIndex.TryGetValue(expectedNextEvent, out edat))
            {
                _curComment = edat.comment;
                if(!edat.description.Equals(dat.description))
                {
                    dat.description = edat.description;
                }

            }
            Utilz.UpdateCurEvent();
            return;
        }
    }
    IEnumerator processNewNotification_impl(eventDesc dsc)
    {
        int oldCount = settings.eventData.Count;
        evId evid = new evId(dsc.run, dsc.evn);
        Debug.Log(dsc);
        if(cache.checkCache(evid))
        {
            //duplicate? or preloaded on previous update
              //Debug.LogFormat("Potentially duplicate or preloaded event {0} / {1}",dsc.run,dsc.evn);
            assignExpected();
            yield break;
        }
        loaderData.counter = 1;
        loaderData.primaryCount = 0;
        loaderData.secondaryCount = 0;
        loaderData.curStatus = "Notification";
        yield return StartCoroutine(OptimisticUpdate());
        Debug.Log("oop");
        if (!cache.checkCache(evid))
        { //odd...
            yield return StartCoroutine(loadAndSaveSingleEvent(dsc));
        }
        assignExpected();
        if (oldCount != settings.eventData.Count)
            Utilz.UpdateEventList();
    }
    IEnumerator processSwitch(eventDesc dsc)
    {
        int oldCount = settings.eventData.Count;
        evId evid = new evId(dsc.run, dsc.evn);
        if (cache.checkCache(evid))
        {
            //duplicate? or preloaded on previous update
            //Debug.LogFormat("Potentially duplicate or preloaded event {0} / {1}",dsc.run,dsc.evn);
            assignExpected();
            yield break;
        }
        if (!loaderData.mainLifecycle)
        {
            loaderData.counter = 1;
            loaderData.primaryCount = 0;
            loaderData.secondaryCount = 0;
            loaderData.curStatus = "Switch";
            loaderData.curStatus = "Check";
            UpdateLoading();
        }
        yield return StartCoroutine(loadAndSaveSingleEvent(dsc));

        Debug.Log("Loading and saving complete");
        if (!cache.checkCache(evid))
        { //odd...

            yield return StartCoroutine(OptimisticUpdate());
        }
        assignExpected();
        if (oldCount != settings.eventData.Count)
            Utilz.UpdateEventList();
    }
    public void SwitchToEvent(eventDesc dsc)
    {
        expectedNextEvent = new evId(dsc.run, dsc.evn);
        StartCoroutine(processSwitch(dsc));
    }
    public void processNewNotification(eventDesc dsc,bool tryAssign)
    {
        if(tryAssign)
            expectedNextEvent = new evId(dsc.run, dsc.evn);
        StartCoroutine(processNewNotification_impl(dsc));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
/* 
 * Savedata lifecycle:
 * 1. Load savedData.json
 * 2. If it is missing or corrupt:
 *    scan folder for sdt and csv files
 *    register those into savedData
 *    save json
 * (saved data/event list is accessible for dropdown at this point)   
 * 3. Scan folder for files not mentioned in saved data. Remove them.
 * 4. If woken up by notification tap - try getting event data if we don't have it in the list as coroutine...
 * 5. Run network update for eventnum we don't seem to have. Save up to set number of files from new ones.
 * 6. Run a full network update (with paging) as coroutine to fill up gaps if any
 * 7. Download files and process files to fill up settings quota. 
 * 8. Prune excess files (save json after every step) 
 * 9. On new notification: run network update (parallel should be ok, even with some overlap)
 * 10. On selecting in dropdown: if event integrated data in cache: set that. 
 *     if in file: load file, set, push into cache
 *     if in csv form: load file, process file (no saving if above quota), set, push into cache
 *     if not present on device: try getting from network, then see above.
 * 11. Statuses (per event): 
 *     downloading
 *     processing
 *     loading data
 *     loading saved
 *     failed download
 *     ...etc
 * 
 */