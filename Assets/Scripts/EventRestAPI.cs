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

[Serializable]
public class numEvents
{
    public Int64 nevents;
}

[Serializable]
public class eventDesc
{
    public Int64 run;
    public Int64 evn;
    public string baseDesc;
    public double energy;
    public string eventDate;
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
        catch(System.Exception e)
        {

            return false;
        }
        return true;
    }
}

public class ThreadedJob
{
     private bool m_IsDone = false;
private object m_Handle = new object();
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

    public string eMessage = null;


    List<eventData> curEvent = new List<eventData>();
    Dictionary<int, Dictionary<int, ballIntegratedData>> affBalls = new Dictionary<int, Dictionary<int, ballIntegratedData>>();
    List<ballIntegratedData> collectedBalls = new List<ballIntegratedData>();
    private string GenerateSHA256( byte[] bytes)
    {

       

        byte[] fileBytes = bytes;
        StringBuilder sb = new StringBuilder();

        using (SHA256Managed sha256 = new SHA256Managed())
        {
            byte[] hash = sha256.ComputeHash(fileBytes);
            foreach (Byte b in hash)
                sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
        //   CreateTextFile(_name + "SHA256.txt", filePath, sb.ToString());
    }

    protected override void ThreadFunction()
    {
        // Do your threaded task. DON'T use the Unity API here
       if(saveCsv)
        {

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
                dat.time_period = float.Parse(fields[4]);
                curEvent.Add(dat);
                if (timeMin == -1 || dat.time < timeMin) { timeMin = dat.time; }
                if (timeMax == -1 || dat.time > timeMax) { timeMax = dat.time; }
                if (signalMin == -1 || dat.signal < signalMin) { signalMin = dat.signal; }
                if (signalMax == -1 || dat.signal > signalMax) { signalMax = dat.signal; }
            }
            catch (System.Exception e)
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
        ed.evNum = eDesc.evn;
        ed.runNum = eDesc.run;
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
        catch (Exception e)
        {

        }
        var bytes = MessagePackSerializer.Serialize(ed);
        string filename = string.Format("{0}/{1}.sdt", persistPath, GenerateSHA256(bytes));
        if(File.Exists(filename))
        {
            eMessage = "Event file already exists... Overwriting";
        }
        FileStream file = File.Create(filename);
        file.Write(bytes, 0, bytes.Length);
        file.Close();
        OutData = ed;
    }
    protected override void OnFinished()
    {
        // This is executed by the Unity main thread when the job is finished
      
    }
}

[Serializable]
public class SavedEventData
{
    eventDesc description;
    string hashname;
    string csvName;
    int integrationSteps;
}
[Serializable]
public class SavedEventsSettings
{
    UInt32 numberIntegrated=20; //number of preintegrated files to keep
    UInt32 numberKeptAsCsv = 60; //number of csv files to keep

}
[Serializable]
public class eventList
{
    public List<List<string>> events;
}
public class EventAccessor
{
    public static string mainURL = "https://ar.obolus.com";
    public static string eventCounter = "nevents";
    public static string lastEvents = "lastevents";
    public static string lastEventsBefore = "lasteventsbeforeid";
    public static string efile = "eventfile";
    bool gotEventNumber = false;
    public Int64 nevents=0;
    public Int64 oldEventNum = 0;
    public List<eventDesc> events=new List<eventDesc>();
    HashSet<KeyValuePair<Int64, Int64>> processedEvents=new HashSet<KeyValuePair<long, long>>();
    public List<eventDesc> lastEventList;
    bool gotLastEvents = false;
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
                Debug.Log("Error getting  "+url+" :" + webRequest.error+" --> "+webRequest.responseCode.ToString()+webRequest.downloadHandler.text);
                yield break;
            }
            else
            {
                JSONNode el=null;
                try
                {
                     el = JSON.Parse(webRequest.downloadHandler.text);
                }
                catch(System.Exception e)
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
                catch(Exception  e)
                {
                    Debug.Log(e);
                    gotEventNumber = false;
                }

            }
        }
    }
    IEnumerator GetLastEvents(Int64 delta)
    {
        lastEventList = new List<eventDesc>();
        string url = String.Format("{0}/{1}/{2}", mainURL, lastEvents,delta);
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
                    foreach(var eda in el["events"])
                    {
                        eventDesc ed = new eventDesc();
                        if(ed.tryLoadFromArray(eda.Value))
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

    bool gotLastBefore = false;
    IEnumerator GetLastEventsBefore(Int64 delta,Int64 runId, Int64 evd)
    {
        //inclusive
        gotLastBefore = false;
        lastEventList = new List<eventDesc>();
        string url = String.Format("{0}/{1}/{2}/{3}/{4}", mainURL, lastEventsBefore, delta,runId,evd);
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

    bool gotEventData = false;
    ProcessEventCsv procJob = null;
    IEnumerator GetEventData(eventDesc dsc)
    {
        gotEventData = false;
        string url = String.Format("{0}/{1}/{2}/{3}", mainURL, efile,dsc.run,dsc.evn);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            webRequest.chunkedTransfer = false;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
               
                Debug.Log("Error getting  " + url + " :" + webRequest.error);
                yield break;
            }
            else
            {
                ProcessEventCsv job = new ProcessEventCsv();
                job.eDesc = dsc;
                job.eMessage = null;
                job.InData = webRequest.downloadHandler.text;
                job.persistPath = Application.persistentDataPath;

                procJob = job;
                gotEventData = true;
            }
        }
    }
    //scans for last events
    public IEnumerator QueryAndProcess(MonoBehaviour handler) 
    {
        gotEventNumber = false;
        yield return handler.StartCoroutine(GetEventNumber());
        if(!gotEventNumber)
        {
            yield break;
        }
        if(nevents==oldEventNum)
        {
            Debug.Log(string.Format("No new events detected, {0} currently",nevents));
            yield break;
        }
        if (nevents < oldEventNum)
        {
            Debug.Log(string.Format("Events are shrinking?, {0} currently, {1} before", nevents, oldEventNum));
            yield break;
        }
        Int64 delta = nevents - oldEventNum;
        gotLastEvents = false;
        yield return handler.StartCoroutine(GetLastEvents(delta));
        if(!gotLastEvents)
        {
            yield break;
        }
        if (lastEventList.Count == 0) yield break;
        if (lastEventList.Count!=delta)
        {
            Debug.Log(string.Format("Got worng number of events? Ah well. {0} expected, {1} got", delta, lastEventList.Count));

        }
        foreach (eventDesc dsc in lastEventList)
        {
            yield return handler.StartCoroutine(GetEventData(dsc));
            if (!gotEventData) continue;
            if (procJob == null) continue;
            procJob.Start();
            yield return handler.StartCoroutine(procJob.WaitFor());
            if (procJob.eMessage!=null)
            {
                Debug.Log(procJob.eMessage);
            }
            if(procJob.OutData==null)
            {
                Debug.Log("Failed processing");
                continue;
            }
            latestData = procJob.OutData;
            if (processedEvents.Contains(new KeyValuePair<long, long>(dsc.run, dsc.evn)))
            {
                Debug.Log(string.Format("Duplicate event? {0}_{1}", dsc.run, dsc.evn));
            }
            else
            {
                events.Add(dsc);
                processedEvents.Add(new KeyValuePair<long, long>(dsc.run, dsc.evn));
            }

        }
        lastEventList.Clear();
        Debug.Log(string.Format("Done updating, cur events {0}",events.Count));
    }
    public fullEventData latestData = null;
}
public class EventRestAPI : MonoBehaviour
{
    EventAccessor nev;

    // Start is called before the first frame update
    void Start()
    {
        string strng= "{\"events\": [[132431, 65009599, \"gfu - gold\"]]}";
        var el = JSON.Parse(strng);

        Debug.Log(el["events"][0]);
        foreach (var eda in el["events"])
        {
            Debug.Log((Int64)eda.Value[0]);
        }
        nev = new EventAccessor();
        StartCoroutine(nev.QueryAndProcess(this));
    }
 
    // Update is called once per frame
    void Update()
    {
        
    }
}
