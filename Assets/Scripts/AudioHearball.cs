using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHearball : MonoBehaviour {

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
	public float decExp=0.99f;
	public float sizeCf=1;
	public Gradient colorLine;
	public Gradient colorRot;
	public float rotcf=0.2f;
	public AnimationCurve absorbSpectrum;
	public float [] abspec=null;
	float rot=0;
	public void consumeSpectrum(float cons)//
	{
		/*if(abspec==null||abspec.Length==0)
		{
			abspec=new float[spectrum.Length];
			float mt=0;
			float dt=1.0f/(float)spectrum.Length;
			for(int i=0;i<abspec.Length;i++)
			{
				abspec[i]=absorbSpectrum.Evaluate(mt);
				mt+=dt;
			}
		}

		UnityEngine.Profiling.Profiler.BeginSample("prof");
		float ret=0;
		for(int i=0;i<abspec.Length;i++)
		{
			ret+=abspec[i]*spectrum[i];
		}
		UnityEngine.Profiling.Profiler.EndSample();*/
		if(cons>0) absorbAcc=absorbAcc+cons;
	}
	// Use this for initialization
	MeshRenderer rnd;
	void Start () {
		rnd=gameObject.GetComponent<MeshRenderer>();
		rotcf=Random.Range(0.2f,0.3f);
	}

	// Update is called once per frame
	float oldAbsrb=0;
	float dlt=0;
	void Update () {
		float dt=Time.deltaTime;
		if(absorbAcc>0)
		{
			float absrb=dt*absSpeed;
			if(absrb>=_absorbAcc)
			{
				absorbed+=_absorbAcc;
				rot+=_absorbAcc*rotcf;
				absorbAcc=0;
			}
			else
			{
				absorbed+=absrb;
				rot+=absrb*rotcf;
				_absorbAcc-=absrb;
			}
		}
		//else
		rot-=Mathf.Floor(rot);
		Vector3 sz=new Vector3(1,1,1);
		gameObject.transform.localScale=sz*(1.0f+sizeCf*absorbed);

		//Debug.Log(dlt);
		if(rnd!=null)
		{
			float odd=0.5f+dlt*10.0f;//Mathf.Min(absorbed/(_absorbAcc+0.1f),_absorbAcc/(absorbed+0.1f));
			Color clr=(colorLine.Evaluate(Mathf.Clamp(odd,0.0f,1.0f)));//((colorLine.Evaluate(Mathf.Clamp(timedec,0.0f,1.0f))))+
			Color rtc=(colorRot.Evaluate(rot));
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            props.SetColor("_color", new Color(r, g, b));
            var renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(props);
            //rnd.material.SetColor("_color",Color.Lerp(clr,rtc,0.3f));
        }
		if(timedec>0)
		{
			timedec-=dt*timingDecay;
			if(timedec<0) timedec=0;

		}
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
			absorbed*=decExp;
		}
		dlt=absorbed-oldAbsrb;
		oldAbsrb=absorbed;
		//if(photons.Count>2)
		//	Debug.LogFormat("Photons {0} ",photons.Count);

	}
}
