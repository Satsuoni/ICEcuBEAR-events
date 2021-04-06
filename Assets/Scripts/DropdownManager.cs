using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownManager : MonoBehaviour
{
    ColorDropdown managed;
    // Start is called before the first frame update
    void Start()
    {
        managed = gameObject.GetComponent<ColorDropdown>();
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
        StartCoroutine(waitAndDefault());

    }
    IEnumerator waitAndDefault()
    {
        while (!EventRestAPI.isDropdownReady)
            yield return null;
        DropdownValueChanged(managed);
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
            managed.interactable = false;
            return;
        }
        curOptions.Clear();
        List<ColorDropdown.OptionData> lst = new List<ColorDropdown.OptionData>();
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
            ColorDropdown.OptionData dopt = new ColorDropdown.OptionData();
            //Debug.Log("Nyanya"+JsonUtility.ToJson(edat.description));
            dopt.image = null;
            if (edat.description.humName != null)
            {
                dopt.text = string.Format("Ev. {0} : {1} ({2}), {3}TeV", edat.description.humName,edat.description.baseDesc, edat.description.eventDate, edat.description.energy);

            }
            else
            {
                dopt.text = string.Format("Ev. {0}/{1} : {2} ({3}), {4}TeV", edat.description.run, edat.description.evn, edat.description.baseDesc, edat.description.eventDate, edat.description.energy);
            }
            dopt.sortKey = edat.getSortLabel();
            lst.Add(dopt);
            curOptions.Add(edat.description);
        }
        lst.Sort((x, y) => -x.sortKey.CompareTo(y.sortKey));
        //Debug.Log("Nyanya options");
        //Debug.Log("DropboxUpdate");
        if(managed==null)
            managed = gameObject.GetComponent<ColorDropdown>();
        managed.options = lst;
        if (lst.Count>0)
        {
            managed.interactable = true;
            DropdownValueChanged(managed);
        }
       
        //managed.ClearOptions();
        //managed.AddOptions(lst);
    }
    void Update()
    {
        if (managed.options.Count == 0)
        {
            managed.interactable = false;
        }
    }
    void DropdownValueChanged(ColorDropdown change)
    {
      
        if(change.value<curOptions.Count)
        {
            Debug.LogFormat("New val :{0}", change.value);
            EventRestAPI.Instance.SwitchToEvent(curOptions[change.value]);
        }
    }
}
