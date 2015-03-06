using UnityEngine;
using System.Collections.Generic;
using SnowplowTracker;
using SnowplowTracker.Storage;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Enums;
using SnowplowTracker.Emitters;

/// <summary>
/// Manages the loading of settings from file as well
/// as creating the Tracker!
/// </summary>
public class MainManager : MonoBehaviour {

	private static string settingsPath = "snowplow_pong_settings.dict";

	// Keys
	public static string GAME_SPEED = "gameSpeed";
	public static string GAME_POINTS = "gamePoints";
	public static string GAME_AI_SPEED = "gameAiSpeed";
	public static string TRACKER_URI = "trackerUri";
	public static string TRACKER_PROTO = "trackerProto";
	public static string TRACKER_TYPE = "trackerType";

	// Variables
	public static float gameSpeed = 30.0f;
	public static int gamePoints = 10;
	public static float gameAiSpeed = 25.0f;
	public static string trackerUri = "change-this-uri";
	public static HttpProtocol trackerProto = HttpProtocol.HTTP;
	public static HttpMethod trackerType = HttpMethod.POST;

	public static bool singlePlayer = true;

	// Tracker
	public static Tracker tracker;

	/// <summary>
	/// Initializes the <see cref="Global"/> class with settings on file.
	/// </summary>
	static MainManager() {
		Dictionary<string, object> maybeSettings = Utils.ReadDictionaryFromFile (settingsPath);
		if (maybeSettings != null) {
			gameSpeed = (float)maybeSettings[GAME_SPEED];
			gamePoints = (int)maybeSettings[GAME_POINTS];
			gameAiSpeed = (float)maybeSettings[GAME_AI_SPEED];
			trackerUri = (string)maybeSettings[TRACKER_URI];
			trackerProto = (HttpProtocol)maybeSettings[TRACKER_PROTO];
			trackerType = (HttpMethod)maybeSettings[TRACKER_TYPE];
		}

		SetupTracker ();
	}

	/// <summary>
	/// Saves the settings.
	/// </summary>
	public static void SaveSettings() {
		Dictionary<string, object> settings = new Dictionary<string, object> ();
		settings.Add (GAME_SPEED, gameSpeed);
		settings.Add (GAME_POINTS, gamePoints);
		settings.Add (GAME_AI_SPEED, gameAiSpeed);
		settings.Add (TRACKER_URI, trackerUri);
		settings.Add (TRACKER_PROTO, trackerProto);
		settings.Add (TRACKER_TYPE, trackerType);

		Utils.WriteDictionaryToFile (settingsPath, settings);
		UpdateTracker ();
	}

	// --- Tracker Init

	/// <summary>
	/// Gets the tracker.
	/// </summary>
	/// <returns>The tracker.</returns>
	private static void SetupTracker() {
		IEmitter emitter = new AsyncEmitter (trackerUri, trackerProto, trackerType);
		tracker = new Tracker (emitter, "SnowplowPong-Namespace", "SnowplowPong-AppId", null, GetSession());
		tracker.StartEventTracking ();

		tracker.Track (new Structured ().SetCategory("SnowplowPong").SetAction("Tracker").SetLabel("Init").Build ());
	}

	// --- Helpers

	private static void UpdateTracker() {
		IEmitter emitter = tracker.GetEmitter ();
		emitter.SetHttpProtocol (trackerProto);
		emitter.SetHttpMethod (trackerType);
		emitter.SetCollectorUri (trackerUri);

		tracker.Track (new Structured ().SetCategory("SnowplowPong").SetAction("Tracker").SetLabel("Updated").Build ());
	}

	/// <summary>
	/// Gets the session.
	/// </summary>
	/// <returns>The session.</returns>
	private static Session GetSession() {
		return new Session ();
	}

	/// <summary>
	/// Gets the context list.
	/// </summary>
	/// <returns>The context list.</returns>
	private static List<IContext> GetContextList() {
		List<IContext> contexts = new List<IContext> ();
		contexts.Add (new DesktopContext ().SetOsType ("OS-X").SetOsVersion ("10.10.5").SetOsServicePack ("Yosemite") .SetOsIs64Bit (true).SetDeviceManufacturer ("Apple").SetDeviceModel ("Macbook Pro").SetDeviceProcessorCount (4).Build ());
		contexts.Add (new MobileContext ().SetOsType ("iOS").SetOsVersion ("9.0").SetDeviceManufacturer ("Apple").SetDeviceModel ("iPhone 6S+").SetCarrier ("FREE").SetNetworkType (NetworkType.Mobile).SetNetworkTechnology ("LTE").Build ());
		contexts.Add (new GeoLocationContext ().SetLatitude(123.564).SetLongitude(-12.6).SetLatitudeLongitudeAccuracy(5.6).SetAltitude(5.5).SetAltitudeAccuracy(2.1).SetBearing(3.2).SetSpeed(100.2).SetTimestamp(1234567890000).Build ());
		return contexts;
	}
}
