using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMeshMaker : MonoBehaviour
{
    public Material trackMaterial;
    int runId = 0;
    int evId = -1;
    MultimeshObject data;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetTimeScale(float scale)
    {
        trackMaterial?.SetFloat("timeScale", scale);
    }
    public void SetTime(float time)
    {
        trackMaterial?.SetFloat("mytime", time);
    }
    // Update is called once per frame
    void Update()
    {
        
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
    public void MaybeRecreateMesh(int run, int ev, Vector3 initialPos, Vector3 heading,float duration,float emrate)
    {
        if(run==runId&&ev==evId)
        {
            //same event..
            return;
        }
        CascadeData cdat = new CascadeData(heading, 1.0f, 9500);
        cdat.location = initialPos;

        MuonTrack tr = new MuonTrack(initialPos, heading, duration, 0.299792458f, emrate,cdat);//emrate
        data = tr.Simulate();
        SpawnChildren();
        runId = run;
        evId = ev;
    }
}
