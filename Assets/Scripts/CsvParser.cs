﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using System.Text.RegularExpressions;
public class PosEntry
{
public float time;
public Vector3 pos;
public int id;
public int meshnum;
}
public class CsvParser : MonoBehaviour {
    int[] jet = new int[] { 12, 22, 24, 29, 44, 51, 55, 64, 84, 105, 106, 136, 156, 160, 165, 175, 182, 190, 196, 200, 261, 263, 269, 287, 293, 299, 307, 314, 315, 319, 360, 368, 373, 374, 387 };
    public TextAsset csvFile;
    public Vector3 offset;
   List <PosEntry> lst=new List<PosEntry>();
	// Use this for initialization
	PosEntry entryFromLine(string line)
	{
	   Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
	   string[] X = CSVParser.Split(line);
	   PosEntry ret=new PosEntry();
	   ret.time=float.Parse(X[1]);
	   ret.pos=new Vector3(float.Parse(X[2]),float.Parse(X[3]),float.Parse(X[4]));
	   ret.id=int.Parse(X[5]);
	   ret.meshnum=int.Parse(X[6]);
	   return ret;
	}
	void Start () {
        MultimeshObject output = new MultimeshObject();
        output.meshes = new List<Meshobject>();
		//string meshPath="Assets/csmesh800/cmesh800_";
	 string fs = csvFile.text;
         string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );	
         int cnt=0;
         int curMesh=-1;
         int curId=-1;
	 List<Vector3> verts = new List<Vector3>();
	 List<Vector2> uvs = new List<Vector2>();
	 List<Color> clrs = new List<Color>();
		List<int> indc = new List<int>();
	 Mesh curM=null;
	 Vector3 avg=Vector3.zero;
	 float mintime=-1;
	 float maxtime=-1;
         foreach(string line in fLines)
         {
         if(line.Length<2) continue;
         PosEntry en=entryFromLine(line);
         lst.Add(en);
         cnt++;
         avg+=en.pos;
         if(maxtime<0||en.time>maxtime) maxtime=en.time;
         if(mintime<0||en.time<mintime) mintime=en.time;

        /* if(curMesh!=en.meshnum)
         {
         if(curM!=null)
           {
	    AssetDatabase.CreateAsset(curM, "Assets/csmesh/cmesh_" + curMesh.ToString() + ".asset");
           }
         }
         if(curMesh==-1) curMesh=en.meshnum;
         if(curId==-1) curId=en.id;

	
	   cnt++;
	   if(cnt>100)
	   break;*/
         }
         Debug.Log(mintime);
	 Debug.Log(maxtime);
	 Debug.Log(avg/cnt);
	 avg/=(float)cnt;
	 cnt=0;
	 float td=maxtime-mintime;
	 foreach(PosEntry en in lst)
	 {
	 if(curMesh!=en.meshnum)
          {
                Meshobject msh = new Meshobject(verts.ToArray(), uvs.ToArray(), indc.ToArray());
                output.meshes.Add(msh);
                //msh.indices = indc.ToArray();
               // msh.verts = verts.ToArray();
               // msh.uvs = uvs.ToArray();
           //if(curM!=null)
           // {
           // curM.vertices=verts.ToArray();
           // curM.uv=uvs.ToArray();
           // curM.colors=clrs.ToArray();
           // curM.SetIndices(indc.ToArray(),MeshTopology.Lines,0);
           //AssetDatabase.CreateAsset(curM, meshPath + curMesh.ToString() + ".asset");
           //}
                verts.Clear();
            uvs.Clear();
            clrs.Clear();
            indc.Clear();
            curMesh=en.meshnum;
            curM=new Mesh();
            cnt=0;
          }
        if(curId==en.id)
        {
        indc.Add(cnt-1);
	indc.Add(cnt);
        }
        else
        curId=en.id;
            verts.Add(en.pos+offset);// -avg);
	float shft=(en.time-mintime);
            if (System.Array.IndexOf(jet, en.id) > -1)
            {
                //ccolor.b = 0.8f;
                uvs.Add(new Vector2(shft, 1));
            }
            else
            {
                uvs.Add(new Vector2(shft, 0));
            }
        clrs.Add( Color.Lerp(Color.red, Color.green, shft));

	   cnt++;
	 }
     if (verts.Count>=2 && indc.Count>0)
        {
            Meshobject msh = new Meshobject(verts.ToArray(), uvs.ToArray(), indc.ToArray());
            output.meshes.Add(msh);
        }
        //if(curM!=null)
        //        {
        //       curM.vertices=verts.ToArray();
        //       curM.uv=uvs.ToArray();
        //       curM.colors=clrs.ToArray();
        //      curM.SetIndices(indc.ToArray(),MeshTopology.Lines,0);
        //			AssetDatabase.CreateAsset(curM, meshPath + curMesh.ToString() + ".asset");
        //            }
        //
        output.SaveToFile(Application.persistentDataPath + "/" + "csvparsed.gz");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
