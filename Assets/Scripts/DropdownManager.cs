using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownManager : MonoBehaviour
{
    Dropdown managed;
    // Start is called before the first frame update
    void Start()
    {
        managed = gameObject.GetComponent<Dropdown>();
        if(managed==null)
        {
            Destroy(this);
            return;
        }
        managed.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(managed);
        });
        //Dropdown.OptionData opt;
        //opt.image = null;

    }
    void Awake()
    {
        Utilz.eventListUpdated += UpdateList;
    }
    void OnDestroy()
    {
        Utilz.eventListUpdated -= UpdateList;
    }
    // Update is called once per frame
    List<eventDesc> curOptions = new List<eventDesc>();
    public void UpdateList()

    {
    
        if (EventRestAPI.Instance == null)
        {
            Debug.Log("NULL Instance");
            return;
        }
        curOptions.Clear();
        List<Dropdown.OptionData> lst = new List<Dropdown.OptionData>();
        if(EventRestAPI.settings==null)
        {
            Debug.Log("Nyanya NULL settings");
            return;
        }
        if (EventRestAPI.settings.eventData == null)
        {
            Debug.Log("Nyanya NULL evdata");
            return;
        }
        foreach (SavedEventData edat in EventRestAPI.settings.eventData)
        {
            Dropdown.OptionData dopt = new Dropdown.OptionData();
            //Debug.Log("Nyanya"+JsonUtility.ToJson(edat.description));
            dopt.image = null;
            dopt.text = string.Format("Ev. {0}/{1} : {2} ({3}), {4}GeV",edat.description.run,edat.description.evn,edat.description.baseDesc,edat.description.eventDate,edat.description.energy);
            lst.Add(dopt);
            curOptions.Add(edat.description);
        }
        //Debug.Log("Nyanya options");
        //Debug.Log("DropboxUpdate");
        if(managed==null)
            managed = gameObject.GetComponent<Dropdown>();
        managed.options = lst;
        //managed.ClearOptions();
        //managed.AddOptions(lst);
    }
    void Update()
    {
        
    }
    void DropdownValueChanged(Dropdown change)
    {
      
        if(change.value<curOptions.Count)
        {
            Debug.LogFormat("New val :{0}", change.value);
            EventRestAPI.Instance.SwitchToEvent(curOptions[change.value]);
        }
    }
}
