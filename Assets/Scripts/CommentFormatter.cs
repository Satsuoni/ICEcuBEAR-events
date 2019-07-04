using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommentFormatter : MonoBehaviour
{
    public Text nameField;
    public Text commentField;
    // Start is called before the first frame update
    void Start()
    {
        if(EventRestAPI.Instance!=null && EventRestAPI.Instance.currentEvent!=null)
        {
            Format(EventRestAPI.Instance.currentEvent.description, EventRestAPI.Instance.currentComment);
        }
        else
        {
            if(nameField!=null)
            {
                nameField.text = "No event selected";
            }
        }
    }
    void Format(eventDesc dsc,string comment)
    {
        if(nameField!=null)
        {
            if (dsc.humName != null)
            {
                nameField.text = string.Format(" {0} ({1}/{2}): {3} ({4}), {5}TeV", dsc.humName,dsc.run,dsc.evn, dsc.baseDesc, dsc.eventDate, dsc.energy);

            }
            else
            {
                nameField.text = string.Format(" {0}/{1}: {2} ({3}), {4}TeV",  dsc.run, dsc.evn, dsc.baseDesc, dsc.eventDate, dsc.energy);
            }

        }
        if(commentField!=null)
        {
            commentField.text = comment;
        }
    }
    // Update is called once per frame
   
    public void Updated()
    {
        if (EventRestAPI.Instance != null && EventRestAPI.Instance.currentEvent != null)
        {
            Format(EventRestAPI.Instance.currentEvent.description, EventRestAPI.Instance.currentComment);
        }
        else
        {
            if (nameField != null)
            {
                nameField.text = "No event selected";
            }
        }
    }
    void Awake()
    {
        Utilz.currentEventUpdated += Updated;
        //Debug.Log("Nyanya coment");
    }
    void OnDestroy()
    {
        Utilz.currentEventUpdated -= Updated;
    }
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
       
    }
}
