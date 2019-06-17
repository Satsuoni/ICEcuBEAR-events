using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if WINDOWS_UWP
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using System;
#endif

public class Config
{
    public float domScale = 300f;
    public float globalScale = 0.1f;
};

public class DOM : MonoBehaviour { //DOM is simply the name used in unity and does not refer to IceCube DOM
	//public AudioSource audioSrc;
	float [] spectrum=new float[256];
    public Mesh mesh;
    public Mesh cylMesh;
    public Material material;
    private static readonly Config cfg = new Config();
    public Dictionary<int, dString> strings = new Dictionary<int, dString>();
	//public SimpleFlyingThing [] bls;
	Dictionary<int,float[]> spectrs=new Dictionary<int, float[]>();
	public void RegisterEmission(float energy, Vector3 pos)
	{
		foreach(KeyValuePair<int, dString> iString in strings)
		{
			iString.Value.RegisterEmission(pos,energy);
		}
	}
    private void Start() {
        Debug.Log("DOM start called.");

        int counter = 0;
        DOMController dc=gameObject.AddComponent<DOMController>();
        // Read the detector config file

#if WINDOWS_UWP

        Task task = new Task(

            async () =>
            {
                StorageFile textFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("detector_config.txt", UriKind.Relative));
                var readFile = await FileIO.ReadLinesAsync(textFile);
                foreach (var line in readFile)
                {
                    string[] fields = line.Split(' ');
                    int stringID = int.Parse(fields[0]);
                    int domID = int.Parse(fields[1]);
                    float x = float.Parse(fields[2]);
                    float y = float.Parse(fields[3]);
                    float z = float.Parse(fields[4]) + 550f; //move icecube coord system by 550 m

                    Vector3 pos = Vector3.right * x * cfg.globalScale
                        + Vector3.forward * y * cfg.globalScale
                        + Vector3.up * z * cfg.globalScale;

                    //create new string if it doesn't exist yet
                    if (!strings.ContainsKey(stringID))
                    {
                        String theString = new GameObject("String " + stringID).AddComponent<String>().
                                Initialize(this, stringID, pos);
                        strings.Add(stringID, theString);
                    }

                    strings[stringID].AddDOM(this, domID, pos);

                }


            });
        task.Start();
        task.Wait();
#endif

        var fileStream = new FileStream(@"Assets/detector_config.txt", FileMode.Open, FileAccess.Read);
        using (var streamReader = new System.IO.StreamReader(fileStream, System.Text.Encoding.UTF8))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] fields = line.Split(' ');
                int stringID = int.Parse(fields[0]);
                int domID = int.Parse(fields[1]);
                float x = float.Parse(fields[2]);
                float y = float.Parse(fields[3]);
                float z = float.Parse(fields[4]) + 550f; //move icecube coord system by 550 m
                
                Vector3 pos = Vector3.right * x * cfg.globalScale
                    + Vector3.forward * y * cfg.globalScale
                    + Vector3.up * z * cfg.globalScale;
                Debug.Log("New string");
                //create new string if it doesn't exist yet
                if(!strings.ContainsKey(stringID))
                {
                    dString theString = new GameObject("String " + stringID).AddComponent<dString>().
                            Initialize(this, stringID, pos);
                    strings.Add(stringID, theString);
                }
                dc.registerStringId(stringID);
                dc.registerDomId(stringID, domID);
                GameObject sDom=strings[stringID].AddDOM(this, domID, pos);
                dc.setBall(stringID, domID, sDom.GetComponent<singleBall>());
		if(!spectrs.ContainsKey(domID))
                 {
                 spectrs[domID]=new float[256];
                 float cnv=(float)domID/70.0f;
                 float dlt=1.0f+3*cnv;
                 for(int i=0;i<256;i++)
                   {
                   float di=(float)i/256.0f;
		   spectrs[domID][i]=dlt*Mathf.Exp(-(cnv-di)*(cnv-di)*60.0f);
                   }
                   }
                counter++;
            }
        }
//        fileStream.Close();
        
        foreach(KeyValuePair<int, dString> iString in strings)
        {
            iString.Value.Finalise();
            Destroy(iString.Value,6);
        }
        
        Debug.Log("Read " + counter.ToString() + " DOMs from file.");
       // GoogleARCore.Examples.HelloAR.HelloARController hc = GameObject.Find("Example Controller").GetComponent<GoogleARCore.Examples.HelloAR.HelloARController>();
       // hc.AndyPlanePrefab = gameObject;
    }
	//float atime=0;
	void Update ()
	{
		/*audioSrc.GetSpectrumData(spectrum,0,FFTWindow.Blackman);
		Dictionary<int,float> vals=new Dictionary<int, float>();
		foreach(KeyValuePair<int, float[]> iFl in spectrs)
		{
		float ret=0;
		for(int i=0;i<256;i++)
		  ret+=iFl.Value[i]*spectrum[i];
                vals[iFl.Key]=ret;
		}
		foreach(KeyValuePair<int, String> iString in strings)
		{
			iString.Value.ConsumeSpectrum(vals);
		} */
		/*atime+=Time.deltaTime;
		if(atime>1.0f)
		{
			if(Random.Range(0.0f,1.0f)>0.95f)
			{
				int bl=Random.Range(0,bls.Length);
				SimpleFlyingThing even=Instantiate<SimpleFlyingThing>(bls[bl]);
				even.transform.position=transform.position+Random.onUnitSphere*30.0f+Vector3.up*30;
				even.parent=this;
			}
			atime-=1.0f;
		}*/
	}
}

public class Dom : MonoBehaviour {
    public int id;
    private Mesh mesh;
    private Material material;
    private float radius = 0.01651f;
    private int depth;
    private static readonly Config cfg = new Config();

    // Use this for initialization
    private void Start() {
        //Debug.Log("PMT start called.");
       
      //  gameObject.AddComponent<MeshFilter>().mesh = mesh;
      //  gameObject.AddComponent<MeshRenderer>().material = material;
    }

    public Dom Initialize (DOM parent, int id, Vector3 pos) {
        this.id = id;
        this.mesh = parent.mesh;
        this.material = parent.material;
        this.radius *= cfg.domScale * cfg.globalScale;
        transform.parent = parent.transform;
        transform.localScale = Vector3.one * radius; //relative
        transform.localPosition = pos;
        return this;
    }


    //private IEnumerator CreateChildren () {
    //    yield return new WaitForSeconds(0.5f);
    //}

    // Update is called once per frame
    void Update () {

    }
}
//time, signal, om, string, time_period

public class dString : MonoBehaviour
{
    private static readonly Config cfg = new Config();
    public int id;
    private Mesh mesh;
    private Material material;
    private float maxZ = -1e9f;
    private float minZ = 1e9f;
    private float radius = 0.005f; //5cm??
    public Dictionary<int, Dom> doms = new Dictionary<int, Dom>();
    public Dictionary<int, singleBall> balls = new Dictionary<int, singleBall>();
	float decOffset=0;
    singleBall prefab;
    // Use this for initialization
    private void Start() {
        Debug.Log("String start called.");
        gameObject.AddComponent<MeshFilter>().mesh = mesh;//PrimitiveType.Cylinder;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
    
    public dString Initialize (DOM parent, int id, Vector3 pos) {
        this.id = id;
        this.mesh = parent.cylMesh;
        this.material = parent.material;
        


        this.radius = 0.005f* cfg.domScale * cfg.globalScale;
        transform.parent = parent.transform;
        pos.y = 0; //y is up, seems so  
        transform.localPosition = pos;
	decOffset=pos.magnitude;
        return this;
    }
    
    public void Finalise() {
        float length = this.maxZ - this.minZ;
        Debug.Log(cfg.domScale);
        Debug.Log(cfg.globalScale);
        Debug.Log(this.radius);
        transform.localScale = Vector3.one * this.radius + Vector3.up * length/2f; //relative
        transform.localPosition = transform.localPosition + Vector3.up * (length/2f + this.minZ);
    }

    
    public GameObject AddDOM(DOM parent, int domID, Vector3 pos)
    {      
		if(prefab==null)
		{
			prefab=Resources.Load<singleBall>("Prefabs/sBall");
			Debug.Log(prefab);
		}
		singleBall exp=Instantiate<singleBall>(prefab);
        //exp.gameObject.GetComponent<MeshRenderer>().sharedMaterial = parent.material;
		exp.gameObject.name="String " + this.id + " DOM "+ domID;
		//exp.absSpeed=decOffset;
		//exp.decExp=0.8f+Mathf.Min(decOffset/450.0f,0.1f);
		this.balls.Add(domID,exp);
	//	Dom dom = exp.gameObject.AddComponent<Dom>().
           //     Initialize(parent, domID, pos);
        //this.id = id;
        //this.mesh = parent.mesh;
        //this.material = parent.material;
        float rradius = 0.01651f* cfg.domScale * cfg.globalScale;
        exp.transform.parent = parent.transform;
        exp.transform.localScale = Vector3.one * rradius; //relative
        exp.transform.localPosition = pos;
        // this.doms.Add(domID, dom);
        this.maxZ = System.Math.Max(this.maxZ, pos.y);
        this.minZ = System.Math.Min(this.minZ, pos.y);
        return exp.gameObject;
    }
	public void ConsumeSpectrum(Dictionary<int,float> specs)
	{
		//foreach(KeyValuePair<int, AudioHearball> iDom in balls)
		//{
		//	iDom.Value.consumeSpectrum(specs[iDom.Key]);
//
		//}
			
	}
	public void RegisterEmission(Vector3 pos,float energy)
	{
		foreach(KeyValuePair<int, Dom> iDom in doms)
		{
			BallExpander cm=iDom.Value.gameObject.GetComponent<BallExpander>();
			if(cm!=null)
			{
				cm.RegisterPhoton((cm.transform.position-pos).magnitude,energy);
			}
		}
	}
}
