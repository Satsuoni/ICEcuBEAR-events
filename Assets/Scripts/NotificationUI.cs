using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NotificationUI : MonoBehaviour
{
    static int npers=10;
    public static NotificationUI prefab;
    public static List<NotificationUI> notifications;
    public static void spawnNotification(string title, string body,eventDesc ev)
    {
        if(notifications==null)
        {
            notifications = new List<NotificationUI>();
        }
        if(prefab==null)
        {
            prefab = Resources.Load<NotificationUI>("Prefabs/Notification");
        }
        if(prefab==null)
        {
            Debug.Log("Fix notification prefab Prefabs/Notification");
            return;
        }
        NotificationUI newNote = Instantiate<NotificationUI>(prefab);
        GameObject canv = GameObject.FindGameObjectWithTag("Canvas");
        if(canv==null)
        {
            Debug.Log("Add canvas tag to your canvas");
            return;
        }
        newNote.gameObject.transform.SetParent(canv.transform, false);
        if (ev != null)
        {
            string date = ev.eventDate;
            if (date == null) date = "";
            newNote.textbox.text = string.Format("{0}: ( {1} {2} / {3} : {4} ) \n {5}", title, date, ev.run, ev.evn, ev.baseDesc, body);
        }
        else
        {
            Debug.Log("Event not parsed?");
            newNote.textbox.text = string.Format("{0}: InValid event",title);
        }
        newNote.ev = ev;
        notifications.Add(newNote);
        newNote.updateLocation = true;
        newNote.updatePos();
    }
    public float duration;
    public float fadeoutDuration;
    public Text textbox;
    public bool updateLocation = false;
    CanvasGroup main;
    eventDesc ev;

    // Start is called before the first frame update
    void Start()
    {
        main = this.gameObject.GetComponent<CanvasGroup>();
        if (main == null) Destroy(this);
        remTime = duration;
        fading = false;
        if (fadeoutDuration <= 0) fadeoutDuration = 0.01f;
    }
    float remTime = 0.0f;
    bool fading = false;
    // Update is called once per frame
    void Update()
    {
        Debug.Log(remTime);
        float dt = Time.deltaTime;
        if (!fading)
        {
            if (dt >= remTime)
            {
                dt -= remTime;
                remTime = fadeoutDuration;
                fading = true;
            }
            else
            {
                remTime -= dt;
                return;
            }
        }
        //not else!
        if(fading)
        {
            if (dt >= remTime)
            {
                main.alpha = 0;
                remTime = 0;
                selfDestruct();
                return;
            }
            remTime -= dt;
            float alpha = remTime/fadeoutDuration;
            main.alpha = alpha;
        }
    }
    void updatePos()
    {
        RectTransform myplace = gameObject.GetComponent<RectTransform>();
        if (myplace == null)
        {
            Debug.Log("Selfdestructing");
            selfDestruct();
            return;
        }
        int count = 0;
        foreach (NotificationUI n in notifications)
        {
            if (n == this)
            {
                break;
            }
            count++;
        }
        myplace.anchorMax = new Vector2(1.0f, 1.0f-(count) * (1.0f / (float)npers));
        myplace.anchorMin = new Vector2(0.0f, 1.0f-(count + 1) * (1.0f / (float)npers));
        myplace.offsetMax = new Vector2(16, 16);
        myplace.offsetMin = new Vector2(16, 16);


    }
    void LateUpdate()

    {
        if (updateLocation)
        {
            updatePos();

            updateLocation = false;
        }
    }
    void selfDestruct()
    {
        if (notifications == null)
        {
            notifications = new List<NotificationUI>();
        }
        bool found = false;
        foreach(NotificationUI n in notifications)
        {
            if (found) n.updateLocation = true;
            if (n == this) found = true;
        }
        notifications.Remove(this);
        Destroy(gameObject);
    }
}
