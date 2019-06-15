using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Photon
{
	public float time;
	public float energy;
	public Photon(float _t, float _e)
	{
		time=_t;
		energy=_e;
	}
}

public class BallExpander : MonoBehaviour {
	float _absorbAcc=0;
	float timedec=0;
	public float absorbAcc
	{
		get{return _absorbAcc;}
		set{
			_absorbAcc=value;
			timedec=1;
		}
	}
	public float timingDecay=0.05f;
	public float absorbed=0;
	public float absSpeed=0.3f;
	public float decSpeed=0.2f;
	public float sizeCf=1;
	public Gradient colorLine;


	List<Photon> photons=new List<Photon>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float dt=Time.deltaTime;
		if(absorbAcc>0)
		{
			float absrb=dt*absSpeed;
			if(absrb>=absorbAcc)
			{
				absorbed+=absorbAcc;
				absorbAcc=0;
			}
			else
			{
				absorbed+=absrb;
				absorbAcc-=absrb;
			}
		}
		else
		{
			//decay
			float decd=dt*decSpeed;
			if(decd>=absorbed)
			{
				absorbed=0;
			}
			else
			{
				absorbed-=decd;
			}
		}
		Vector3 sz=new Vector3(1,1,1);
		gameObject.transform.localScale=sz*(1.0f+sizeCf*absorbed);

		MeshRenderer rnd=gameObject.GetComponent<MeshRenderer>();
		if(rnd!=null)
		{
			rnd.material.SetColor("_color",colorLine.Evaluate(Mathf.Clamp(timedec,0.0f,1.0f)));
		}
		if(timedec>0)
		{
			timedec-=dt*timingDecay;
			if(timedec<0) timedec=0;

		}
		//if(photons.Count>2)
		//	Debug.LogFormat("Photons {0} ",photons.Count);
		for (int i=0;i<photons.Count;i++)
		{
			photons[i].time=photons[i].time-dt;
			if(photons[i].time<=0)
			{
				absorbAcc=absorbAcc+photons[i].energy;
			}
		}
		photons.RemoveAll(item => item.time<=0);
	}
	public void RegisterPhoton(float dist,float energy)
	{
		float en=energy/(dist*dist+0.1f);
		if(en<1e-2) return;

		photons.Add(new Photon(dist/CSpeed,en));
	}
	public static float CSpeed=5.0f;
}
