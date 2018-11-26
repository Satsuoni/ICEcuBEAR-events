using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManyMeshes : MonoBehaviour {
    public Material mt;
	// Use this for initialization
	void Start () {
		for(int i=0;i<1000;i++)
		{
		 Mesh ldm=Resources.Load<Mesh>("Meshes/rmeshes_"+i.ToString());
		 GameObject ngm=new GameObject("rmeshes_"+i.ToString());
		 ngm.transform.SetParent(gameObject.transform);
		 ngm.transform.localPosition=Vector3.zero;
		 ngm.transform.localScale=new Vector3(1f,1f,1f);
		 MeshFilter mf= ngm.AddComponent<MeshFilter>();
		 MeshRenderer mr=ngm.AddComponent<MeshRenderer>();
		 mf.mesh=ldm;
		 mr.sharedMaterial=mt;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
