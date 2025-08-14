using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine;
using SharpCompress.Compressors.Deflate;
using SimpleJSON;
using System.Security.Cryptography;

public class SkyMeshobject
{
    public Vector3[] verts;
    public Vector3[] normals;
    public Vector2[] uv;
    public Vector2[] uv2;
    public Vector2[] uv3;
    public Vector2[] uv4;
    public Vector2[] uv5;
    public Vector2[] uv6;
    public int[] triangles;

    public void transUV(JSONArray uvs,ref Vector2[]uv, bool norm=false)
    {
        int cnt = 0;
        uv = new Vector2[uvs.Count];
        float dn = 0.0f;
        foreach (JSONNode n in uvs)
        {
            if (!n.IsArray) return;
            uv[cnt] = new Vector2(n[0], n[1]);
            cnt++;
        }
        if(norm)
        {
            float mx = uv[0].x;
            float mn= uv[0].x;
            foreach(Vector2 v in uv)
            {
                if (mx > v.x) mx = v.x;
                if (mn < v.x) mn = v.x;
            }
            for(int i = 0; i < uv.Length; i++)
            {
                uv[i].x = (uv[i].x - mx) / (mn - mx);
            }
        }
    }
    public SkyMeshobject(JSONObject obj)
    {
        JSONArray vrt =(JSONArray) obj["vertices"];
        JSONArray _uv = (JSONArray) obj["uv"];
        JSONArray _uv2 = (JSONArray)obj["uv2"];
        JSONArray _uv3 = (JSONArray)obj["uv3"];
        JSONArray _uv4 = (JSONArray)obj["uv4"];
        JSONArray _uv5 = (JSONArray)obj["uv5"];
        JSONArray _uv6 = (JSONArray)obj["uv6"];
        JSONArray _normals = (JSONArray)obj["normals"];
        JSONArray ind = (JSONArray)obj["triangles"];
        if(vrt==null||_uv==null||ind==null)
        {
            return;
        }
        verts = new Vector3[vrt.Count];
        int cnt = 0;
        foreach(JSONNode n in vrt)
        {
            if (!n.IsArray) return;
            verts[cnt] =new Vector3(n[0],n[1],n[2]);
            cnt++;
        }
        cnt = 0;
        normals = new Vector3[_normals.Count];
        foreach (JSONNode n in _normals)
        {
            if (!n.IsArray) return;
            normals[cnt] =-( new Vector3(n[0], n[1], n[2]));
            cnt++;
        }
        transUV(_uv, ref uv);
        transUV(_uv2, ref uv2);
        transUV(_uv3, ref uv3);
        transUV(_uv4, ref uv4,true);
        transUV(_uv5, ref uv5, true);
        transUV(_uv6, ref uv6, true);
        Debug.LogFormat("vert len {0}", uv4.Length);
        Debug.LogFormat("uv4 len {0}", uv4.Length);
        Debug.LogFormat("uv5 len {0}", uv5.Length);
        Debug.LogFormat("uv6 len {0}", uv6.Length);
        cnt = 0;

        triangles = new int[ind.Count];
        foreach (JSONNode n in ind)
        {
            triangles[cnt] = n;
            cnt++;
        }
    }
    public Mesh ToMesh()
    {
        Mesh newM = new Mesh();
        newM.vertices = verts;
        newM.uv = uv;
        newM.uv2 = uv2;
        newM.uv3 = uv3;
        newM.uv4 = uv4;
        newM.uv5 = uv5;
        newM.uv6 = uv6;
        newM.SetTriangles(triangles, 0);
        newM.normals = normals;
        return newM;
    }
    
}


public class SkymeshMaker : MonoBehaviour
{

    public TextAsset jsonFile;
    public string meshName = "jmesh";
    public Mesh toCheck;
    void Start()
    {
        JSONNode dat = SimpleJSON.JSON.Parse(jsonFile.text);
        Debug.LogWarning(dat.IsObject);
        SkyMeshobject obj =new SkyMeshobject((JSONObject)dat);
        Mesh tosave = obj.ToMesh();
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(tosave,"Assets/"+meshName+".asset");
        Debug.LogFormat("UV {0} vs {1}", toCheck.uv[0], tosave.uv[0]);
        Debug.LogFormat("UV2 {0} vs {1}", toCheck.uv2[0], tosave.uv2[0]);
        Debug.LogFormat("UV3 {0} vs {1}", toCheck.uv3[0], tosave.uv3[0]);
        Debug.LogFormat("UV4 {0} vs {1}", toCheck.uv4[0], tosave.uv4[0]);
        Debug.LogFormat("UV4-1 {0} vs {1}", toCheck.uv4[1], tosave.uv4[1]);
        Debug.LogFormat("UV4-2 {0} vs {1}", toCheck.uv4[2], tosave.uv4[2]);
        Debug.LogFormat("UV5 {0} vs {1}", toCheck.uv5[0], tosave.uv5[0]);
        Debug.LogFormat("UV6 {0} vs {1}", toCheck.uv6[0], tosave.uv6[0]);
        Debug.LogFormat("UVNorm {0} vs {1}", toCheck.normals[0], tosave.normals[0]);
        Debug.LogFormat("UVVert {0} vs {1}", toCheck.vertices[0], tosave.vertices[0]);
#endif
    }


}
