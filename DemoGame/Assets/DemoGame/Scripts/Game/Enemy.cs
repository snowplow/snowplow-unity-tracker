using UnityEngine;
using System.Collections;
using SnowplowTracker.Events;

public class Enemy : MonoBehaviour {

	private int paddleSpeed;
	private float aiSpeed = MainManager.gameAiSpeed;
	private float aiSleep;
	private bool singlePlayer = MainManager.singlePlayer;
	private bool moving = false;
	AudioSource blip;

	/// <summary>
	/// Start this instance and fetches the audio source.
	/// </summary>
	void Start () {
		blip = GetComponent<AudioSource>();

		aiSleep = (1 / aiSpeed) / (10 * aiSpeed);
		paddleSpeed = 30 + ((int)aiSpeed / 100);
	}

	/// <summary>
	/// Called every second; contains control and boundary settings.
	/// </summary>
	void Update () {
		if (singlePlayer && Ball.GetIfStarted() && !moving) {
			moving = true;
			StartCoroutine(AiSleepAndMove());
		} else if (!singlePlayer) {
			if (Input.GetButton ("UP_ENEMY")) {
				transform.Translate(new Vector3(0,paddleSpeed,0) * Time.deltaTime);
			} else if (Input.GetButton ("DOWN_ENEMY")) {
				transform.Translate(new Vector3(0,-paddleSpeed,0) * Time.deltaTime);
			}
		}

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
		MainManager.tracker.Track (new Structured ().SetCategory("GameScene").SetAction("PaddleCollision").SetLabel("Player 2 Paddle").Build ());
		blip.Play ();
	}

	/// <summary>
	/// Controls the AI Pause between moves
	/// </summary>
	/// <returns>The move.</returns>
	private IEnumerator AiSleepAndMove() {
		yield return new WaitForSeconds (aiSleep);

		float paddleY = transform.position.y;
		float ballY = Ball.GetPosition().y;
		
		if (paddleY < ballY) {
			transform.Translate(new Vector3(0,paddleSpeed,0) * Time.deltaTime);
		} else {
			transform.Translate(new Vector3(0,-paddleSpeed,0) * Time.deltaTime);
		}
		moving = false;
	}
}
