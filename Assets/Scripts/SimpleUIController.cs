using UnityEngine;
using UnityEngine.UI;

public class SimpleUIController : MonoBehaviour
{
    [SerializeField] private Button test1;
    [SerializeField] private Button test2;
    [SerializeField] private Button test3;

    private void OnEnable()
    {
        test1.onClick.AddListener(OnTest1);
        test2.onClick.AddListener(OnTest2);
        test3.onClick.AddListener(OnTest3);
    }

    private void OnDisable()
    {
        test1.onClick.RemoveListener(OnTest1);
        test2.onClick.RemoveListener(OnTest2);
        test3.onClick.RemoveListener(OnTest3);
    }

    private void OnTest1()
    {
        AnalyticsService.Instance.TrackEvent("event_type_1", "data1");
    }

    private void OnTest2()
    {
        AnalyticsService.Instance.TrackEvent("event_type_2", "data2");
        AnalyticsService.Instance.TrackEvent("event_type_2", "data3");
    }

    private void OnTest3()
    {
        AnalyticsService.Instance.TrackEvent("event_type_3", "data4");
        AnalyticsService.Instance.TrackEvent("event_type_3", "data5");
        AnalyticsService.Instance.TrackEvent("event_type_3", "data6");
    }
}