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


public delegate void EventListUpdatedHandler();
public delegate void CurrentEventChangedHandler();
public class Utilz
{
    public static event EventListUpdatedHandler eventListUpdated;
    public static event CurrentEventChangedHandler currentEventUpdated;
    public static void UpdateCurEvent()
    {
        currentEventUpdated();
    }
    public static void UpdateEventList()
    {
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
public class eventDesc
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
        }
        catch(Exception )
        {

            return false;
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
    private System.Threading.Thread m_Thread = null;
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
    m_Thread = new System.Threading.Thread(Run);
    m_Thread.Start();
}
public virtual void Abort()
{
    m_Thread.Abort();
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
private void Run()
{
    ThreadFunction();
    IsDone = true;
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
        ed.minPureTime = timeMax;
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
        if (animationSpeed < 0.1f) animationSpeed = 0.2f;
        if (animationSpeed > 1f) animationSpeed = 0.2f;
        if (scalePower < 0.04f) scalePower = 0.15f;
        if (scalePower > 0.3f) scalePower = 0.15f;
        if (scaleMul < 1) scaleMul = 5f;
        if (scaleMul > 20) scaleMul = 5f;
        return true;
    }
    public float animationSpeed;
    public float scalePower;
    public float scaleMul;
}


[Serializable]
public class eventList
{
    public List<List<string>> events;
}

public class PrimCache<id,t>
{
    UInt32 maxItems = 20;
    public void setMaxItems(UInt32 mi)
    {
        maxItems = mi;
    }
    Dictionary<id, t> index = new Dictionary<id, t>();
    Queue<id> order = new Queue<id>();
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
    public static volatile bool isDropdownReady = false;
    public static List<eventDesc> notificationsWoken = new List<eventDesc>();
    const string saveFileName = "savedData.json";
    evId expectedNextEvent = new evId(-1,-1);
    bool _lifecycleReady = false;
    public bool lifecycleReady { get { return _lifecycleReady; } }
    // Start is called before the first frame update
    void Awake()
    {
        if (_Instance != null) Destroy(gameObject);
        else
            _Instance = this;
    }
    void OnDestroy()
    {
        _Instance = null;
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
        if (settings.animationSpeed < 0.05f) settings.animationSpeed = 0.05f;
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
                s.status = "Csv recovered";

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
    public static string mainURL = "https://ar.obolus.com";
    public static string eventCounter = "nevents";
    public static string lastEvents = "lastevents";
    public static string lastEventsBefore = "lasteventsbeforeid";
    public static string efile = "eventfile";

    fullEventData _currentEvent =null;
    public fullEventData currentEvent
    {
        get { return _currentEvent; }
    }
    //primitive lock
    HashSet<evId> __locks = new HashSet<evId>();
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
    }
    //preferably, only process event here to avoid races, etc;
    IEnumerator loadAndSaveSingleEvent(eventDesc ev,bool saveInt=true,bool saveCSV=true)
    {
        if (ev == null) yield break;
        string dir = Application.persistentDataPath + "/";
        evId evid = new evId(ev.run,ev.evn);
        if(cache.checkCache(evid))
        {
            //already have it, break;
            yield break;
        }
        while(!_lock(evid))
        {
            yield return null;
        }
        SavedEventData sdat = null;
        if (savedIndex.ContainsKey(evid))
        {
            sdat = savedIndex[evid];
            if(sdat.hashname!=null)
            {
                string fhash = dir + sdat.hashname;
                if(File.Exists(fhash))
                {
                    try
                    {
                        byte[] dat = File.ReadAllBytes(fhash);
                        fullEventData fl = MessagePackSerializer.Deserialize<fullEventData>(dat);
                        if(fl.description.evn==ev.evn&& fl.description.run == ev.run)
                        {
                            //load complete
                            cache.pushItem(evid, fl);
                            _unlock(evid);//rAII is pain
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
            if(sdat.csvName!=null)
            {
                string fcsv = dir + sdat.csvName;
                if (File.Exists(fcsv))
                {
                    string csvText = null;
                    try
                    {
                        byte[] csvBytes = File.ReadAllBytes(fcsv);
                        string hash = Utilz.GenerateSHA256(csvBytes);
                        if(sdat.csvHash==null || hash!=sdat.csvHash)
                        { Debug.Log("Invalid csv hash"); csvText = null; }
                        else
                        csvText = System.Text.Encoding.UTF8.GetString(csvBytes);
                    }
                    catch (Exception e)
                    {
                        Debug.LogFormat("Corrupt csv? {0} {1}", sdat.csvName, e);
                    }
                    if (csvText != null)
                    {
                        ProcessEventCsv job = new ProcessEventCsv();
                        job.eDesc = ev;
                        job.eMessage = null;
                        job.InData = csvText;
                        job.saveIntegrated = saveInt;
                        job.saveCsv = false;
                        job.persistPath = Application.persistentDataPath;
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
                        }
                        else
                        {
                            cache.pushItem(evid, job.OutData);
                            sdat.hashname = job.hashName;
                            sdat.integrationSteps = job.timeSpans;
                            _unlock(evid);
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
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {

                Debug.Log("Error getting  " + url + " :" + webRequest.error);
                sdat.status = "Failed download";
                if (!savedIndex.ContainsKey(evid))
                {
                    savedIndex[evid] = sdat;
                    settings.eventData.Add(sdat);
                    saveSettings();
                }
                _unlock(evid);
                yield break;
            }
            else
            {
                ProcessEventCsv job = new ProcessEventCsv();
                job.eDesc = ev;
                job.saveCsv = saveCSV;
                job.saveIntegrated = saveInt;
                job.eMessage = null;
                job.InData = webRequest.downloadHandler.text;
                job.persistPath = Application.persistentDataPath;

                job.Start();
                yield return StartCoroutine(job.WaitFor());
                if (job.eMessage != null) { Debug.Log(job.eMessage); }
                if (job.OutData==null)
                {
                    sdat.status = "Failed processing";
                    if(!savedIndex.ContainsKey(evid))
                    {
                        savedIndex[evid] = sdat;
                        settings.eventData.Add(sdat);
                        saveSettings();
                    }
                    _unlock(evid);
                    yield break;
                }
                cache.pushItem(evid, job.OutData);
                sdat.hashname = job.hashName;
                sdat.csvHash = job.csvHash;
                sdat.csvName = job.csvName;
                sdat.integrationSteps = job.timeSpans;
                if (!savedIndex.ContainsKey(evid))
                {
                    savedIndex[evid] = sdat;
                    settings.eventData.Add(sdat);
                    saveSettings();
                }
            }
        }
        _unlock(evid);
    }
    IEnumerator loadNotifiedEvents()
    {
         while( notificationsWoken.Count>0) //amount can change while loading...
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
            webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
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
            webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
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
                        if (ed.tryLoadFromArray(eda.Value))
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
    public IEnumerator OptimisticUpdate()
    {
        int oldCount = settings.eventData.Count;
        gotEventNumber = false;
        yield return StartCoroutine(GetEventNumber());
        if (!gotEventNumber)
        {
            yield break;
        }
        int oldEventNum = settings.eventData.Count;
        if (nevents == settings.eventData.Count)
        {
            Debug.Log(string.Format("No new events detected, {0} currently", nevents));
            yield break;
        }
        if (nevents < oldEventNum)
        {
            Debug.Log(string.Format("Events are shrinking?, {0} currently, {1} before", nevents, oldEventNum));
            yield break;
        }
        Int64 delta = nevents - oldEventNum;
        gotLastEvents = false;
        yield return StartCoroutine(GetLastEvents(delta));
        if (!gotLastEvents)
        {
            yield break;
        }
        if (lastEventList.Count == 0)
        {
            yield break;
        }
        if (lastEventList.Count != delta)
        {
            Debug.Log(string.Format("Got wrong number of events? Ah well. {0} expected, {1} got", delta, lastEventList.Count));

        }
        foreach (eventDesc dsc in lastEventList)
        {
            yield return StartCoroutine(loadAndSaveSingleEvent(dsc));
            _unlock(new evId(dsc.run,dsc.evn)); //just in case
        }
        lastEventList.Clear();
        saveSettings();
        if (oldCount != settings.eventData.Count)
            Utilz.UpdateEventList();
        Debug.Log(string.Format("Done updating, cur events {0}", settings.eventData.Count));
      
    }
    bool gotLastBefore = false;
    IEnumerator GetLastEventsBefore(Int64 delta, Int64 runId, Int64 evd)
    {
        //inclusive
        gotLastBefore = false;
        lastEventList = new List<eventDesc>();
        string url = String.Format("{0}/{1}/{2}/{3}/{4}", mainURL, lastEventsBefore, delta, runId, evd);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
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
                        if (ed.tryLoadFromArray(eda.Value))
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
        if (!tryLoadingSave(fullSaveName))
        {
            Debug.Log("Could not read settings, repairing");
            yield return StartCoroutine(repairSave());
            if (!tryLoadingSave(fullSaveName))
            {
                Debug.Log("Could not repair settings!");
                yield break;
            }
        }
        if (settings == null)
        {
            Debug.Log("Could not recover settings!");
            yield break; //may crash after, but at that point, something is seriously off?
        }
        foreach (SavedEventData evd in settings.eventData) //reindex
        {
            savedIndex[new evId(evd.description.run, evd.description.evn)] = evd;
        }
        //3. Scan folder for files not mentioned in saved data. Remove them.
        HashSet<string> mentioned = new HashSet<string>();
        Debug.Log(settings.eventData.Count);
        foreach (SavedEventData evd in settings.eventData)
        {
            if (evd.csvName != null) mentioned.Add(evd.csvName);
            if (evd.hashname != null) mentioned.Add(evd.hashname);
            //Debug.LogFormat("Mentioned {0}", evd.csvName);
        }
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
        //5. Run network update for eventnum we don't seem to have. Save up to set number of files from new ones.
        yield return StartCoroutine(OptimisticUpdate());
        pruneFiles();
        cache.Prune();
        //6. Run a full network update (with paging) as coroutine to fill up gaps if any
        yield return StartCoroutine(runFullUpdate());
        //7.Download files and process files to fill up settings quota.
        _lifecycleReady = true;
        yield return StartCoroutine(fillOutFiles());
        //8. Prune excess files (save json after every step)  -done in above
    }
    public IEnumerator runFullUpdate()
    {
        gotLastEvents = false;
        int oldCount = settings.eventData.Count;
        yield return StartCoroutine(GetLastEvents(1));
        if (gotLastEvents && lastEventList.Count >= 1)
        {
            long page = 10;
            HashSet<evId> updCheck = new HashSet<evId>();
            bool left = true;
            while (left)
            {
                left = false;
                lastEventList.Sort((x, y) => { return x.Compare(y); });
                bool upd = false;
                foreach (eventDesc ev in lastEventList)
                {
                    
                    evId eid = new evId(ev.run, ev.evn);
                    if (!updCheck.Contains(eid))
                    {
                        updCheck.Add(eid);
                        left = true;
                    }
                    if(!savedIndex.ContainsKey(eid))
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
                }
                if(upd)
                {
                    settings.eventData.Sort((x, y) => { return x.description.Compare(y.description); });
                    saveSettings();
                }
                eventDesc curLastEv = lastEventList[0];
                yield return StartCoroutine(GetLastEventsBefore(page, curLastEv.run, curLastEv.evn));
                if (!gotLastBefore) left = false;
            }
            pruneFiles();
            settings.eventData.Sort((x, y) => { return x.description.Compare(y.description); });
            saveSettings();
        }
        if (oldCount != settings.eventData.Count)
            Utilz.UpdateEventList();
    }

    void assignExpected()
    {
        Debug.Log("Assigning expected");
        if (expectedNextEvent.Key == -1) return;
        if (!cache.checkCache(expectedNextEvent)) return;
        fullEventData dat = cache.getItem(expectedNextEvent);
        if (dat == null) return;
        Debug.Log("Got cached");
        if (dat.description==null)
        {
            Debug.Log("Corrupt DATA");
            return;
        }
        if(_currentEvent==null)
        {
            _currentEvent = dat;
            Utilz.UpdateCurEvent();
            return;
        }
        if(_currentEvent.description.run!= dat.description.run|| _currentEvent.description.evn != dat.description.evn)
        {
            _currentEvent = dat;
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
        yield return StartCoroutine(loadAndSaveSingleEvent(dsc));

        Debug.Log("oop");
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