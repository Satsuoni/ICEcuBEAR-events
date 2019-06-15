using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseCentral : MonoBehaviour
{
    public Text txt;
    Firebase.FirebaseApp app;
    public void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                Firebase.Messaging.FirebaseMessaging.SubscribeAsync("/topics/ar").ContinueWith(res =>
                {
                    
                    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                    Debug.Log("Firebase initialized");
                });
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });


      
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);

    }
    /*
     *"data" : {
      "run" : 1000,
      "event" : 2000,
      "name" : "gold"
    }
     */
    eventDesc tryLoadingEventDesc(IDictionary<string,string> data)
    {
        if (data == null) return null;
        eventDesc ret = new eventDesc();
        if (!data.ContainsKey("run")) return null;
        if (!Int64.TryParse(data["run"], out ret.run)) return null;
        if (!data.ContainsKey("event")) return null;
        if (!Int64.TryParse(data["event"], out ret.evn)) return null;
        if (!data.ContainsKey("name"))
        {
            ret.baseDesc = "";
        }
        else
        {
            ret.baseDesc = data["name"];
        }
        if (!data.ContainsKey("date"))
        {
            ret.eventDate = "";
        }
        else
        {
            ret.eventDate = data["date"];
        }
        if (!data.ContainsKey("energy"))
        {
            ret.energy = 0;
        }
        else
        {
            ret.energy = 0 ;
            double.TryParse(data["energy"], out ret.energy);
        }
        if (data.ContainsKey("desc"))
        {
            ret.humName = data["desc"];
        }
        return ret;
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
        //e.Message.data; ->dict
        //e.Message.Notification.Title;
        //e.Message.Notification.Body;
        if (txt != null)
            txt.text = "Message " + e.Message.From + e.Message.Notification.Body;

        eventDesc newEvent = tryLoadingEventDesc(e.Message.Data);
        if (newEvent == null)
        {
            Debug.Log("Malformed data in message");
            return;
        }
        NotificationUI.spawnNotification(e.Message.Notification.Title, e.Message.Notification.Body, newEvent);
    }


  
}
