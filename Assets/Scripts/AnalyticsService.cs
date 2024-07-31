using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticsService : MonoBehaviour
{
    private static string PendingEventsName => "pendingEvents";

    [Serializable]
    public class Event
    {
        public string type;
        public string data;
    }

    [Serializable]
    public class EventList
    {
        public List<Event> events = new List<Event>();
    }

    public static AnalyticsService Instance { get; private set; }

    [SerializeField] private string serverUrl;
    [SerializeField] private float cooldownBeforeSend = 1.0f;
    [SerializeField] private int maxMessagesForSave = 1000;
    [SerializeField] private int NetworkingTimeoutSeconds = 10;
    [SerializeField] private float NetworkingWaitSecondsForNextTryeng = 15f;

    private List<Event> eventQueue = new List<Event>();
    private bool isCooldownActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPendingEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SavePendingEvents();
    }

    public void TrackEvent(string type, string data)
    {
        Debug.LogWarning($"EventService.TrackEvent type:{type} data: {data}");
        var newEvent = new Event {type = type, data = data};
        eventQueue.Add(newEvent);

        if (!isCooldownActive)
        {
            StartCoroutine(EventCooldownCoroutine());
        }
    }

    private IEnumerator EventCooldownCoroutine()
    {
        isCooldownActive = true;

        while (eventQueue.Count > 0)
        {
            yield return new WaitForSeconds(cooldownBeforeSend);
            string jsonData = JsonUtility.ToJson(GetLimitedEvents());
            yield return SendEventsCoroutine(jsonData);
        }

        isCooldownActive = false;
    }

    private IEnumerator SendEventsCoroutine(string jsonData)
    {
        using UnityWebRequest www = UnityWebRequest.Post(serverUrl, jsonData);

        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.timeout = NetworkingTimeoutSeconds;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            eventQueue.Clear();
            SavePendingEvents();
        }
        else
        {
            Debug.LogWarning($"EventService.SendEventsCoroutine Failed to send events, saving to file");
            SavePendingEvents();
            yield return new WaitForSeconds(NetworkingWaitSecondsForNextTryeng);
        }
    }

    private void SavePendingEvents()
    {
        var jsonData = JsonUtility.ToJson(GetLimitedEvents());
        PlayerPrefs.SetString(PendingEventsName, jsonData);
    }

    private object GetLimitedEvents()
    {
        if (eventQueue.Count > maxMessagesForSave)
        {
            var events = eventQueue.Skip(Math.Max(0, eventQueue.Count - maxMessagesForSave)).ToList();
            Debug.LogWarning($"AnalyticsService.GetLimitedEvents warning: Events is over limits: {eventQueue.Count}/{maxMessagesForSave}");
            return new EventList {events = events};
        }

        return new EventList {events = eventQueue};
    }

    private void LoadPendingEvents()
    {
        string jsonData = PlayerPrefs.GetString(PendingEventsName, string.Empty);

        if (!string.IsNullOrEmpty(jsonData))
        {
            EventList eventList = JsonUtility.FromJson<EventList>(jsonData);
            eventQueue = eventList.events ?? new List<Event>();
            Debug.LogWarning($"EventService.LoadPendingEvents eventQueue:{eventQueue.Count}");
        }
    }
}