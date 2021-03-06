using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class EventManager : MonoBehaviour
{
    [System.Serializable]
    public class Event : UnityEvent<EventDict> { }
    // public class EventData : Dictionary<string, EventDict> {} //I wish to add this for less text

    public Dictionary<string, Event> eventDictionary;

    private static EventManager eventManager;

    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, Event>();
        }
    }

    public static void StartListening(string eventName, UnityAction<EventDict> listener)
    {
        Event thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new Event();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<EventDict> listener)
    {
        if (eventManager == null) return;
        Event thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }


    /// <summary>
    /// Emit an event, you need to also add the gameObject as a parameter, and also for some Events is necessary adding more data,
    /// check documentation for more info 
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="sender"></param>
    /// <param name="additionalDataDict"></param>
    public static void TriggerEvent(string eventName, GameObject sender = null, EventDict additionalDataDict = null)
    {
        Event thisEvent = null;
        EventDict data = new EventDict() { ["sender"] = sender };
        if (additionalDataDict != null)
        {
            foreach (var item in additionalDataDict)
            {
                data[item.Key] = item.Value;
            }
        }

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(data);
            Debug.Log("Event Triggered: " + eventName);
        }else{
            Debug.Log("No one is listening: " + eventName);
        }
    }
}
public class EventDict : Dictionary<string, object>
{
}