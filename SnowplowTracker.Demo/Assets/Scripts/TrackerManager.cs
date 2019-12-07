using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SnowplowTracker;
using SnowplowTracker.Enums;
using SnowplowTracker.Emitters;
using SnowplowTracker.Payloads.Contexts;
using System;
using UnityEngine.UI;

public class TrackerManager : MonoBehaviour
{
    /// <summary>
    /// Can be linked up to a on screen UI control for the collector URL
    /// </summary>
    public InputField CollectorInput;
    private static string _collectorUrl = "<<COLLECTORURL>>";

    /// <summary>
    /// Static Tracker object that accesses the Lazy Tracker variable defined below
    /// </summary>
    public static Tracker SnowplowTracker => _snowplowTracker.Value;

    /// <summary>
    /// Initialises the Emitter and Tracker.
    /// Stored in a Lazy so it is not initialised until it is first accessed
    /// This allows the Collector URL to be entered on the initial Main Menu screen
    /// </summary>
    /// <typeparam name="Tracker"></typeparam>
    /// <returns></returns>
    private static readonly Lazy<Tracker> _snowplowTracker = new Lazy<Tracker>(() => {
        IEmitter emitter = new AsyncEmitter(_collectorUrl, HttpProtocol.HTTP, HttpMethod.POST, 1, 52000L, 52000L);
        var tracker = new Tracker(emitter, "SnowplowUnityTrackerNamespace", "SnowplowUnityTracker-AppId", GetSubject(), new Session(null));
        tracker.StartEventTracking();
        return tracker;
    });

    /// <summary>
    /// Gets an example context list.
    /// </summary>
    /// <returns>The context list.</returns>
    public static List<IContext> GetExampleContextList()
    {
        List<IContext> contexts = new List<IContext>
        {
            new DesktopContext().SetOsType("OS-X").SetOsVersion("10.10.5").SetOsServicePack("Yosemite").SetOsIs64Bit(true).SetDeviceManufacturer("Apple").SetDeviceModel("Macbook Pro").SetDeviceProcessorCount(4).Build(),
            new MobileContext().SetOsType("iOS").SetOsVersion("9.0").SetDeviceManufacturer("Apple").SetDeviceModel("iPhone 6S+").SetCarrier("FREE").SetNetworkType(NetworkType.Mobile).SetNetworkTechnology("LTE").Build(),
            new GeoLocationContext().SetLatitude(12.56).SetLongitude(-12.6).SetLatitudeLongitudeAccuracy(5.6).SetAltitude(5.5).SetAltitudeAccuracy(2.1).SetBearing(3.2).SetSpeed(100.2).SetTimestamp(1234567890000).Build()
        };
        return contexts;
    }

    /// <summary>
    /// Gets an example subject from PlayerPrefs
    /// </summary>
    /// <returns>The subject.</returns>
    private static Subject GetSubject()
    {
        if (!PlayerPrefs.HasKey("userId"))
        {
            PlayerPrefs.SetString("userId", Guid.NewGuid().ToString());
            PlayerPrefs.Save();
        }
        var subject = new Subject();
        subject.SetUserId(PlayerPrefs.GetString("userId"));
        return subject;
    }

    /// <summary>
    /// Adds an event listener if a Collector URL Input Field is hooked up to this MonoBehaviour
    /// </summary>
    private void Start() {
        if (CollectorInput != null) CollectorInput.onEndEdit.AddListener((x) => _collectorUrl = x);
    }
}
