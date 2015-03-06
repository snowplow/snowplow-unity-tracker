using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SnowplowTracker.Enums;
using SnowplowTracker.Events;

/// <summary>
/// Controls the loading and saving of values in the SettingsScene.
/// </summary>
public class SaveAndLoad : MonoBehaviour {

	private Slider gameSpeedSlider;
	private InputField gamePointsField;
	private Slider gameAiSpeedSlider;
	private InputField trackerUriField;
	private InputField trackerProtoField;
	private InputField trackerTypeField;

	/// <summary>
	/// On start load all fields and set the Global manager settings.
	/// </summary>
	public void Start() {
		gameSpeedSlider = GameObject.Find("SpeedSlider").GetComponent<Slider>();
		gamePointsField = GameObject.Find("PointField").GetComponent<InputField>();
		gameAiSpeedSlider = GameObject.Find("AiSlider").GetComponent<Slider>();
		trackerUriField = GameObject.Find("UriField").GetComponent<InputField>();
		trackerProtoField = GameObject.Find("ProtocolField").GetComponent<InputField>();
		trackerTypeField = GameObject.Find("TypeField").GetComponent<InputField>();

		SetValues ();
	}

	/// <summary>
	/// Saves the settings to the Global manager.
	/// </summary>
	public void Apply() {
		MainManager.gameSpeed =  float.Parse(gameSpeedSlider.value.ToString());
		MainManager.gamePoints =  int.Parse(gamePointsField.text);
		MainManager.gameAiSpeed = float.Parse (gameAiSpeedSlider.value.ToString());
		MainManager.trackerUri = string.IsNullOrEmpty(trackerUriField.text) ? "change-this-uri" : trackerUriField.text;
		MainManager.trackerProto = trackerProtoField.text == "HTTPS" ? HttpProtocol.HTTPS : HttpProtocol.HTTP;
		MainManager.trackerType = trackerTypeField.text == "GET" ? HttpMethod.GET : HttpMethod.POST;

		SetValues ();

		MainManager.tracker.Track (new Structured ().SetCategory("SettingsScene").SetAction("Saved").Build ());
	}

	/// <summary>
	/// Resets all settings back to default.
	/// </summary>
	public void Reset() {
		MainManager.gameSpeed = 30.0f;
		MainManager.gamePoints = 10;
		MainManager.gameAiSpeed = 25.0f;
		MainManager.trackerUri = "change-this-uri";
		MainManager.trackerProto = HttpProtocol.HTTP;
		MainManager.trackerType = HttpMethod.POST;

		SetValues ();

		MainManager.tracker.Track (new Structured ().SetCategory("SettingsScene").SetAction("Reset").Build ());
	}

	/// <summary>
	/// Sets the values.
	/// </summary>
	private void SetValues() {
		gameSpeedSlider.value = float.Parse (MainManager.gameSpeed.ToString());
		gamePointsField.text = MainManager.gamePoints.ToString ();
		gameAiSpeedSlider.value = float.Parse (MainManager.gameAiSpeed.ToString());
		trackerUriField.text = MainManager.trackerUri;
		trackerProtoField.text = MainManager.trackerProto.ToString();
		trackerTypeField.text = MainManager.trackerType.ToString();

		MainManager.SaveSettings ();
	}
}
