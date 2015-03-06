using UnityEngine;
using System.Collections;
using SnowplowTracker.Events;

public class Player : MonoBehaviour {

	private int paddleSpeed;
	private AudioSource blip;

	/// <summary>
	/// Start this instance and fetches the audio source.
	/// </summary>
	void Start () {
		blip = GetComponent<AudioSource>();
		paddleSpeed = 40;
	}

	/// <summary>
	/// Called every second; contains control and boundary settings.
	/// </summary>
	void Update () {
		// Sets the player controls
		if (Input.GetButton ("UP_PLAYER")) {
			transform.Translate(new Vector3(0,paddleSpeed,0) * Time.deltaTime);
		} else if (Input.GetButton ("DOWN_PLAYER")) {
			transform.Translate(new Vector3(0,-paddleSpeed,0) * Time.deltaTime);
		}

		// Sets the player boundaries
		if (transform.position.y > 12f) {
			transform.position = new Vector3(transform.position.x, 12f, 0);
		} else if (transform.position.y < -12f) {
			transform.position = new Vector3(transform.position.x, -12f, 0);
		}
	}

	/// <summary>
	/// Raises the collision enter event and plays a sound effect.
	/// </summary>
	/// <param name="col">Col.</param>
	void OnCollisionEnter(Collision col) {
		MainManager.tracker.Track (new Structured ().SetCategory("GameScene").SetAction("PaddleCollision").SetLabel("Player 1 Paddle").Build ());
		blip.Play ();
	}
}
