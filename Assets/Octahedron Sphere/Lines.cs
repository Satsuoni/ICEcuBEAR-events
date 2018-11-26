using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEditor;


public class Lines : MonoBehaviour {
  private List<int> indicesForLineStrip = new List<int>();
  private Mesh mesh;
  private float last_time;
  public float curr_time = 0;
  public int curr_vert = 1;


	// Use this for initialization
	Mesh makeRandomMesh(int numVerts)
	{
		List<Vector3> verts = new List<Vector3>();
		Vector2 [] uvs =new Vector2[numVerts];
		for (int i = 0; i < numVerts; i++)
            {
              //int newvertex = Random.Range(0, 6);
			Vector3 dx=Random.insideUnitSphere*0.1f;
             // float newlen = Random.Range(0.01f, 0.1f);
	    verts.Add(verts.LastOrDefault() + dx); //newlen*possible[newvertex]);
	    uvs[i]=new Vector2((float)i/(float)(numVerts),0f);
            }
            Mesh ret=new Mesh();
            ret.Clear();
            ret.vertices=verts.ToArray();
            ret.uv=uvs;
	    Color[] colors = new Color[verts.Count()];
	    for (int i = 0; i < verts.Count(); i++)
              colors[i] = Color.Lerp(Color.red, Color.green, ((float)i)/verts.Count());
          ret.colors = colors;
          int [] inds=new int[numVerts];
		for (int i = 0; i < numVerts; i++)
                  inds[i]=i;
		ret.SetIndices(inds, MeshTopology.LineStrip, 0);
       
          return ret; 
	}
	void Start () {
          //Vector3[] possible = new Vector3[]{Vector3.up, Vector3.right, Vector3.down, Vector3.left, Vector3.forward, Vector3.back};
          List<Vector3> verts = new List<Vector3>();
          Vector2 [] uvs =new Vector2[1000];
		/*for (int i = 0; i < 1000; i++)
		{
		Mesh sh=makeRandomMesh(1000);
		AssetDatabase.CreateAsset(sh, "Assets/Meshes/rmeshes_" + i.ToString() + ".asset");
		}*/
          for (int i = 0; i < 1000; i++)
            {
              //int newvertex = Random.Range(0, 6);
			Vector3 dx=Random.insideUnitSphere*0.1f;
             // float newlen = Random.Range(0.01f, 0.1f);
	    verts.Add(verts.LastOrDefault() + dx); //newlen*possible[newvertex]);
	    uvs[i]=new Vector2((float)i/1000.0f,0f);
            }

          var stringsArray = verts.Select(i=>i.ToString()).ToArray();
          // var asd = string.Join(",", stringsArray);
          //Debug.Log("Indx: " + asd);
          mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;
          mesh.Clear();
          mesh.vertices = verts.ToArray();
          mesh.uv=uvs;
          // create new colors array where the colors will be created.
          Color[] colors = new Color[verts.Count()];
          for (int i = 0; i < verts.Count(); i++)
            colors[i] = Color.Lerp(Color.red, Color.green, ((float)i)/verts.Count());
          mesh.colors = colors;

          //mesh.SetIndices(indicesForLineStrip.ToArray(), MeshTopology.LineStrip, 0);
          mesh.SetIndices(indicesForLineStrip.ToArray(), MeshTopology.Lines, 0);
          //mesh.RecalculateNormals();
          mesh.RecalculateBounds();
	}
	bool saved=false;
	// Update is called once per frame
	void Update () {
        var material = ((Renderer)gameObject.GetComponent("Renderer")).material;
        curr_time += Time.deltaTime;
        Debug.Log(curr_time.ToString());
        material.SetFloat("mytime", curr_time);
          if (curr_time - last_time > 10)
            {
            resetTime();
              if(curr_vert >= 1000)
                {
                if(!saved)
                {
		  //AssetDatabase.CreateAsset(mesh, "Assets/meshes_" + "random" + ".asset");
		  saved=true;
		  }
                  return;
                }
              indicesForLineStrip.Add(curr_vert);
              curr_vert += 1;
              mesh.SetIndices(indicesForLineStrip.ToArray(), MeshTopology.LineStrip, 0);
       
              //mesh.RecalculateNormals();
              //mesh.RecalculateBounds();
              last_time = curr_time;
            }
	}
    void resetTime()
    {
        curr_time = 0;
    }
}
