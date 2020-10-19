using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class Meshobject
{
    public Vector3[] verts;
    public Vector2[] uvs;
    public int[] indices;
    public Meshobject(Vector3 [] v, Vector2 [] u, int[] i)
    {
        verts = v;
        uvs = u;
        indices = i;
    }
    public Mesh ToMesh()
    {
        Mesh newM = new Mesh();
        newM.vertices = verts;
        newM.uv = uvs;
        newM.SetIndices(indices, MeshTopology.Lines, 0);
        return newM;
    }
}
public class MultimeshObject
{
    public List<Meshobject> meshes=new List<Meshobject>();

}

public class ScatterPhoton
{
    public static float speed = 0.299792458f / 1.32501299335f; //per nanosecond
    public float timeOffset;
    public Vector3 position;
    public Vector3 direction;
    public bool decayed=false;
    public Vector3 prevPosition;
    public float prevTime = 0;
    public void Propagate(float absorbtion, float scatter)// scatter length
    {
        float distance = TrailmeshMaker.DrawExp(scatter);
        float dur = distance / speed;
        float decayprob = 1.0f - Mathf.Exp(-distance / absorbtion);
        prevPosition = position;
        prevTime = timeOffset;
        position = position + direction * distance;
        timeOffset += dur;
        if (Random.value<=decayprob)
        {
            decayed = true;
            return;
        }
        direction=TrailmeshMaker.Scatter(direction);
    }
}

public static class IcecubeDust
{
    public static float[] grid =  new float[] { 1400, 1450, 1500, 1550, 1600, 1650, 1700, 1750, 1800, 1850, 1900, 1950, 2000, 2050, 2100, 2150, 2200, 2250, 2300, 2350, 2400, 2450, 2500 };
    public static float[]  abslength = new float[] { 55, 70, 150, 73, 100, 190, 125, 80, 140, 125, 166, 156, 33, 40, 230, 160, 75, 166, 260, 90, 200, 250, 200 };
    public static float[] sctlen = new float[] { 13, 20, 34, 15, 25, 40, 33, 21, 37, 22, 49, 40, 11, 6, 45, 51, 33, 53, 70, 34, 52, 75, 60 };
    public static float getApx(Vector3 icCoords, float[] inp) ///needs tilt!
    {
        float cor = 1450 + 2450 / 2;

        float pz = icCoords.z + cor; //0=2500?

        if (pz <= 1400) return inp[0];
        if (pz >= 2500) return inp[inp.Length - 1];
        float r = (pz - 1400) * inp.Length / (2500 - 1400);
        int lower = Mathf.FloorToInt(r);
        int upper = lower + 1;
        if (upper >= inp.Length) upper = lower;
        float cf = r - lower;
        return inp[lower] * (1 - cf) + inp[upper] * cf;
    }
    public static float getAbsorption(Vector3 icCoords)
    {
        return getApx(icCoords, abslength);

    }
    public static float getScatter(Vector3 icCoords)
    {
        return getApx(icCoords, sctlen);

    }
}
public class MuonTrack
{
    public float duration;
    public float speed;
    public Vector3 initialPos;
    public Vector3 direction;
    public float emissionRate; //per nanosecond...
    public static float sin41 = Mathf.Sin(41.0f * Mathf.PI / 180.0f);
    public static float cos41 = Mathf.Cos(41.0f * Mathf.PI / 180.0f);
    ScatterPhoton EmitCherenkovPhoton(Vector3 pos,float timeOffset)
    {
        float shift = Random.value;
        Vector3 perp = new Vector3(-direction.y, direction.x, 0);
        if (direction.z>0.9)
        {
            perp= new Vector3(0, direction.z, -direction.y);
        }
        Quaternion rt = Quaternion.AngleAxis(shift * 360, direction);
        perp = rt * perp;
        perp /= perp.magnitude;
        Vector3 photondir = direction * cos41 + perp * sin41;
        ScatterPhoton ret = new ScatterPhoton();
        ret.direction = photondir;
        ret.position = pos;
        ret.timeOffset = timeOffset;
        //ret.Propagate(IcecubeDust.getAbsorption(pos), IcecubeDust.getScatter(pos));
        return ret;
    }
    public MuonTrack(Vector3 start,Vector3 orientation, float dur,float speed,float emrate=50)
    {
        initialPos = start;
        direction = orientation/orientation.magnitude;
        duration = dur;
        this.speed = speed;
        emissionRate = emrate;
    }
    public MultimeshObject PhotonsToMesh(List<ScatterPhoton> photons)
    {
        MultimeshObject ret = new MultimeshObject();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //List<Color> clrs = new List<Color>();
        List<int> indc = new List<int>();
        foreach(ScatterPhoton ph in photons)
        {
            int cnt = 0;
            while(!ph.decayed)
            {
                verts.Add(ph.position-initialPos);
               // float shft = (en.time - mintime) / td;
                uvs.Add(new Vector2(ph.timeOffset, 0));
                //65530
                // clrs.Add(Color.Lerp(Color.red, Color.green, shft));   
                if(cnt==0 && ph.decayed)
                {
                    verts.RemoveAt(verts.Count-1);
                    uvs.RemoveAt(uvs.Count - 1);
                }
                if(cnt>0)
                {
                    indc.Add(verts.Count - 2);
                    indc.Add(verts.Count - 1);
                }
                if (verts.Count > 65530) break;
                ph.Propagate(IcecubeDust.getAbsorption(ph.position), IcecubeDust.getScatter(ph.position));
                cnt += 1;
            }
            if(verts.Count > 65530)
            {
                Meshobject newM = new Meshobject(verts.ToArray(),uvs.ToArray(),indc.ToArray());
                ret.meshes.Add(newM);
                verts.Clear();
                uvs.Clear();
                indc.Clear();
                   
            }
        }
        if (indc.Count>1)
        {
            Meshobject newM = new Meshobject(verts.ToArray(), uvs.ToArray(), indc.ToArray());
            ret.meshes.Add(newM);
        }
        return ret;
    }
    virtual public MultimeshObject Simulate()
    {
        int emittedPhotons = Mathf.RoundToInt(emissionRate * duration);
        MultimeshObject  ret = new MultimeshObject();
        List<ScatterPhoton> emitted = new List<ScatterPhoton>();
        for(int i=0;i<emittedPhotons;i++)
        {
            float emitTime = Random.value * duration;
            Vector3 ppos = emitTime * speed * direction + initialPos;
            emitted.Add(EmitCherenkovPhoton(ppos,emitTime));
            if (emitted.Count>= 16384)
            {
                ret.meshes.AddRange(PhotonsToMesh(emitted).meshes);
                emitted.Clear();
            }
        }
        ret.meshes.AddRange(PhotonsToMesh(emitted).meshes);
        return ret;
    }
}
public class TrailmeshMaker : MonoBehaviour
{
    // Start is called before the first frame update
    static Quaternion rot = Quaternion.Euler(new Vector3(0,0,125));
    static float al = Mathf.Exp(-0.082f);
    static float bet = Mathf.Exp(0.040f);
    static float gm = Mathf.Exp(0.042f);
    void Start()
    {
       /*trackData frc = new trackData();
        frc.azi_rad = 2.160849318134296f;
        frc.dec_rad = 0.18339852545918298f;
        frc.mjd = 58694.86853369251;
        frc.ra_rad = 3.958934679276621;
        frc.rec_t0 = 17437.752211638854f;
        frc.rec_x = -175.9504622785223f;
        frc.rec_y = -76.50362830103359f;
        frc.rec_z = -502.3460613431437f;
        frc.zen_rad = 1.752881090566949f;
        MakeEventTrail(frc); */
        //MakeTrail();  
    }
    public static float DrawExp(float ilam)
    {
        if (ilam <= 0) ilam = 0.01f;
        float ran = Random.value;
        float ret = -Mathf.Log(1.0f - ran) *ilam ;
        if (ret > ilam * 10000) return ilam * 10000;
        return ret;
    }
    public static float scatterprob(float cosn)
    {
        float g = 0.94f;
        float alpha = 2 * g / (1 - g);
        float liu = 0.5f * (1 + alpha) * Mathf.Pow(0.5f*(1+cosn), alpha);
        float hg = 0.5f * (1 - g * g) /Mathf.Pow (1+g*g-2*g*cosn,1.5f);
        return 0.55f * hg + 0.45f * liu;
    }
    public static float scatterdirprob(Vector3 dir, Vector3 d2)
    {
       Vector3 rv = rot * dir;
       Vector3 rv2 = rot * d2;
       Vector3 adj1 = new Vector3(rv.x*al,rv.y*bet,rv.z*gm);
       Vector3 adj2 = new Vector3(rv2.x * al, rv2.y * bet, rv2.z * gm);
       float cosn = Vector3.Dot(adj1, adj2) / (adj1.magnitude * adj2.magnitude);
       return scatterprob(cosn);
    }
    public static Vector3 Scatter(Vector3 inc)
    {
        int cnt = 0;
        while(true)
        {
            Vector3 rd = Random.onUnitSphere;
            float prob = scatterdirprob(inc, rd);
            if (prob>=Random.value)
            {
                return rd;
            }
            if (cnt > 100) return inc;
            cnt += 1;
        }
    }
    public void MakeTrail()
    {
       
        
      /*  MuonTrack track = new MuonTrack(new Vector3(0, 0, 0), Random.onUnitSphere, 500, 2.99792458f/1.001f, 100);
        MultimeshObject meshes = track.Simulate();
        int cnt = 0;
        foreach(Meshobject m in meshes.meshes)
        {
            AssetDatabase.CreateAsset(m.ToMesh(), "Assets/Trailmesh/tmesh2_" + cnt.ToString() + ".asset");
            cnt += 1;
        }*/
        
      
        //            }

    }
    public static void MakeEventTrail(trackData track)
    {
       /* Vector3 tdir = new Vector3(Mathf.Sin(track.zen_rad) * Mathf.Cos(track.azi_rad), Mathf.Sin(track.zen_rad) * Mathf.Sin(track.azi_rad), Mathf.Cos(track.zen_rad));
        Vector3 ipos = new Vector3(track.rec_x, track.rec_y, track.rec_z);
        MuonTrack mtrack = new MuonTrack(ipos, tdir,5000, 0.299792458f / 1.00000001f, 20);
        MultimeshObject meshes = mtrack.Simulate();
        int cnt = 0;
        foreach (Meshobject m in meshes.meshes)
        {
            AssetDatabase.CreateAsset(m.ToMesh(), "Assets/Trailmesh/tmesh2_" + cnt.ToString() + ".asset");
            cnt += 1;
        }*/
    }

}
