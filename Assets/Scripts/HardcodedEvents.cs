using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using evId = System.Collections.Generic.KeyValuePair<long, long>;
[System.Serializable]
public class HadcodedDesc
{
    public string baseDesc;
    public double energy;
    public string eventDate;
    public string humName = null;
    public string comment = null;
}
    [System.Serializable]
public class HardcodedEventData
{
    public long runId = 0;
    public long evId = -1;
    public HadcodedDesc desc;
    public TextAsset csvFile;
    public TextAsset millipedeFile;
    public TextAsset meshFile;
    public trackData[] tracks; //only 1 allowed really... 
    public bool isTrackSuppressed = false;
}

public class HardcodedEvents : MonoBehaviour
{
    public static HardcodedEvents instance = null;
    public HardcodedEventData[] events;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }
    void Start()
    {
        
    }
    void OnDestroy()
    {
        instance = null;
    }
    public HardcodedEventData GetHardcoded(evId ev)
    {
        foreach(HardcodedEventData ed in events)
        {
            if (ed.runId == ev.Key && ed.evId == ev.Value)
                return ed;
        }
        return null;
    }
  
}
