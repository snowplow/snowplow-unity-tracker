using UnityEngine;
using System.Collections;
using SnowplowTracker.Events;

/// <summary>
/// Manages loading scenes and navigation.
/// </summary>
public class SceneManager : MonoBehaviour {

	/// <summary>
	/// Loads the game in single player mode.
	/// </summary>
	public void LoadGameSinglePlayer() {
		MainManager.singlePlayer = true;
		Application.LoadLevel("GameScene");
		MainManager.tracker.Track (new ScreenView ().SetId("GameScene").SetName("Game-Single").Build());
	}

	/// <summary>
	/// Loads the game in multi player mode.
	/// </summary>
	public void LoadGameMultiPlayer() {
		MainManager.singlePlayer = false;
		Application.LoadLevel("GameScene");
		MainManager.tracker.Track (new ScreenView ().SetId("GameScene").SetName("Game-Multi").Build());
	}

	/// <summary>
	/// Loads the settings scene.
	/// </summary>
	public void LoadSettings() {
		Application.LoadLevel("SettingsScene");
		MainManager.tracker.Track (new ScreenView ().SetId("SettingsScene").SetName("Settings").Build());
	}

	/// <summary>
	/// Quit this instance.
	/// </summary>
	public void Quit() {
		MainManager.tracker.Track (new Structured ().SetCategory("MenuScene").SetAction("Navigation").SetLabel("Quit").Build ());
		Application.Quit ();
	}

	/// <summary>
	/// Goes back to the Menu.
	/// </summary>
	public void Update() {
		if (Input.GetButton ("BACK")) {
			MainManager.tracker.Track (new Structured ().SetCategory("MenuScene").SetAction("Navigation").SetLabel("Menu").Build ());
			Application.LoadLevel("MenuScene");
			MainManager.tracker.Track (new ScreenView ().SetId("MenuScene").SetName("Menu").Build());
		}	
	}
}
