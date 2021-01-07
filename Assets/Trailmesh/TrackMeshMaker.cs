using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class extraData
{
   public int runId = 0;
  public   int evId = -1;
    public TextAsset csvFile;
}

public class TrackMeshMaker : MonoBehaviour
{
    public Material trackMaterial;
    public extraData[] cascades;
    //  public TextAsset csvFile;
    int runId = 0;
    int evId = -1;
    MultimeshObject data;
    public struct burstEntry
    {
        public double energy;
        public Vector3 coords;
        public int index;
        public double time;
    }
    burstEntry[] bursts = null;
    int prunId = 134698;
    int pevId = 40735501;
    public void readCsv(extraData ndat)
    {
        string[] fLines = null;
        if (ndat.csvFile == null) return;
        string InData = ndat.csvFile.text;
        if (InData.Contains("\r\n"))
        {
            fLines = System.Text.RegularExpressions.Regex.Split(InData, "\r\n");
        }
        else
        {
            fLines = System.Text.RegularExpressions.Regex.Split(InData, "\n|\r");

        }
        double energyMin = -1f, energyMax = -1f;
        bool skipHeader = true;
        List<burstEntry> lst = new List<burstEntry>();
        foreach (var line in fLines)
        {
            if(skipHeader)
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
                if (energyMin == -1 || energyMin > dat.energy) energyMin = dat.energy;
            }
            catch (System.Exception)
            {
                //Debug.Log(e);
                Debug.Log(line);

                //Debug.Log(line.Split(','));
            }
        }
        bursts = lst.ToArray();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetTimeScale(float scale)
    {
        trackMaterial?.SetFloat("timeScale", scale);
    }
    public void SetTimeWidth(float width)
    {
        trackMaterial?.SetFloat("timeWidth", width);
    }
    public void SetTime(float time)
    {
        trackMaterial?.SetFloat("mytime", time);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Clear()
    {
        foreach (Transform tr in gameObject.transform)
        {
            Destroy(tr.gameObject);
        }
        data = null;
        runId = -1;
        evId = -1;
    }
    public void Inactivate()
    {
        foreach (Transform tr in gameObject.transform)
        {
            tr.gameObject.SetActive(false);
        }
        
    }
    public void Activate()
    {
        foreach (Transform tr in gameObject.transform)
        {
            tr.gameObject.SetActive(true);
        }

    }
    public bool HasMesh()
    {
        return data != null;
    }
    public void SpawnChildren()
    {
     if (data==null)
        {
            Debug.Log("need mesh data");
            return;
        }
     foreach(Transform tr in gameObject.transform)
        {
            Destroy(tr.gameObject);
        }
        int cnt = 0;
     foreach(Meshobject m in data.meshes)
        {
            string name = string.Format("Submesh_{0}", cnt);
            GameObject child = new GameObject(name);
            MeshRenderer render = child.AddComponent<MeshRenderer>();
            MeshFilter filter = child.AddComponent<MeshFilter>();
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = new Vector3(1, 1, 1);
            filter.mesh = m.ToMesh();
            render.sharedMaterial = trackMaterial;
            cnt += 1;
        }
    }
    public void UseMesh(int run, int ev, MultimeshObject mmesh)
    {
        Activate();
        if (run == runId && ev == evId)
        {
            //same event..
            return;
        }
        data = mmesh;
        SpawnChildren();
        runId = run;
        evId = ev;
    }
    public void MaybeRecreateProperMesh(int run, int ev, Vector3 initialPos, Vector3 heading, float duration, float emrate, float itime)
    {
        if (run == runId && ev == evId)
        {
            //same event..
            return;
        }
        foreach (extraData csc in cascades)
        {
            if (run == csc.runId && ev == csc.evId)
            {
                readCsv(csc);
                CascadeData[] cdat = new CascadeData[bursts.Length];
                float maxEnergy = 0;
                float minnz = 0;
                for (int i = 0; i < bursts.Length; i++)
                {
                    if (bursts[i].energy > maxEnergy) maxEnergy = (float)bursts[i].energy;
                    if (minnz==0||(bursts[i].energy > 0&&minnz< bursts[i].energy)) minnz = (float)bursts[i].energy;
                }
                if (minnz*5000 / maxEnergy < 1000) maxEnergy = minnz * 1000;
                    for (int i = 0; i < bursts.Length; i++)
                {
                    int en =(int)( bursts[i].energy * 5000 / maxEnergy);
                    if (en > 5000) en = 5000;
                    cdat[i] = new CascadeData(heading, 1.0f, (int)en, (float)bursts[i].time - itime);
                    cdat[i].location = bursts[i].coords;
                    Debug.Log("ENERGY");
                    Debug.Log(maxEnergy);
                    Debug.Log(en);
                    Debug.Log((int)bursts[i].energy);
                    //cdat[i].in = bursts[i].coords;
                }
                //new CascadeData(heading, 1.0f, 9500);


                MuonTrack tr = new MuonTrack(initialPos, heading, duration, 0.299792458f, 6000/maxEnergy, cdat);//emrate
                data = tr.Simulate();
                data.SaveToFile(Application.persistentDataPath + "/" + "trail.gz");
                data.LoadFromFile(Application.persistentDataPath + "/" + "trail.gz");
                SpawnChildren();
                runId = run;
                evId = ev;
                break;
            }
        }
    }

    public void MaybeRecreateMesh(int run, int ev, Vector3 initialPos, Vector3 heading,float duration,float emrate)
    {
        if(run==runId&&ev==evId)
        {
            //same event..
            return;
        }
        CascadeData cdat = new CascadeData(heading, 1.0f, 9500,0);
        cdat.location = initialPos;

        MuonTrack tr = new MuonTrack(initialPos, heading, duration, 0.299792458f, emrate,cdat);//emrate
        data = tr.Simulate();
        SpawnChildren();
        runId = run;
        evId = ev;
    }
}
