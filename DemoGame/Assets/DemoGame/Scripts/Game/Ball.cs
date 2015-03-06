using UnityEngine;
using System.Collections;
using SnowplowTracker.Events;

public class Ball : MonoBehaviour {

	private TextMesh countdown;
	private AudioSource blip;
	private static Rigidbody rb;
	private static bool winner = false;
	private static bool started = false;

	private float sx;
	private float sy;
	private float speed = MainManager.gameSpeed;
	private int points = MainManager.gamePoints;

	/// <summary>
	/// On start initiate a countdown and then start
	/// the play sequence.
	/// </summary>
	void Start() {
		started = false;
		countdown = GameObject.Find("Countdown").GetComponent<TextMesh>();
		blip = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody>();

		StartCoroutine (DoCountdown());
	}
	
	/// <summary>
	/// Called every frame; checks whether the ball has gone out of bounds and
	/// assigns a score to the winning player.
	/// </summary>
	void Update () {
		// Ball has gone past enemy paddle
		if (rb.position.x > 25) {
			Score.IncrementPlayer();
			Reset ();
		}

		// Ball has gone past player paddle
		if (rb.position.x < -25) {
			Score.IncrementEnemy();
			Reset ();
		}

		// Restart once winner has been found or if points == 0
		if (Input.GetButton ("RESTART") && (winner || points == 0)) {
			winner = false;
			rb.position = new Vector3 (0, 0, 0);
			rb.velocity = new Vector3 (0, 0, 0);
			StartCoroutine (DoCountdown());
		}
	}

	/// <summary>
	/// Gets the position.
	/// </summary>
	/// <returns>The position.</returns>
	public static Vector3 GetPosition() {
		return rb.position;
	}

	/// <summary>
	/// Gets if started.
	/// </summary>
	/// <returns><c>true</c>, if if winner was gotten, <c>false</c> otherwise.</returns>
	public static bool GetIfStarted() {
		return started;
	}

	// --- Helpers

	/// <summary>
	/// Reset the ball and check if we have a winner!
	/// </summary>
	private void Reset() {
		rb.position = new Vector3(0, 0, 0);
		rb.velocity = new Vector3 (0, 0, 0);

		if (points != 0) {
			if (Score.GetPlayerScore () >= points) {
				SetWinnerMessage("Player 1");
			} else if (Score.GetEnemyScore () >= points) {
				SetWinnerMessage("Player 2");
			} else {
				StartCoroutine(DoResetWait());
			}
		} else {
			StartCoroutine(DoResetWait());
		}
	}

	/// <summary>
	/// Sets the ball velocity.
	/// </summary>
	private void SetBallVelocity() {
		sx = Random.Range (0, 2) == 0 ? 1 : -1;
		sy = Random.Range (0, 2) == 0 ? 1 : -1;
		rb.velocity = new Vector3 (speed * sx, speed * sy, 0);
	}

	/// <summary>
	/// Sets the winner message.
	/// </summary>
	/// <param name="message">Message.</param>
	private void SetWinnerMessage(string message) {
		winner = true;
		countdown.fontSize = 150;
		countdown.text = message + " has won!\nSpacebar to restart...";

		MainManager.tracker.Track (new Structured ().SetCategory("GameScene").SetAction("GameOver").SetLabel("Winner-"+message).SetValue(points).Build ());

		started = false;
	}

	// --- Enumerators

	/// <summary>
	/// Does a countdown sequence for the game.
	/// </summary>
	/// <returns>The countdown.</returns>
	private IEnumerator DoCountdown() {
		Score.Reset ();
		countdown.fontSize = 500;

		countdown.text = "3";
		blip.Play ();
		yield return new WaitForSeconds(1);
		countdown.text = "2";
		blip.Play ();
		yield return new WaitForSeconds(1);
		countdown.text = "1";
		blip.Play ();
		yield return new WaitForSeconds (1);
		countdown.text = "";
		Reset ();

		started = true;
	}

	/// <summary>
	/// Does the reset wait.
	/// </summary>
	/// <returns>The reset wait.</returns>
	private IEnumerator DoResetWait() {
		yield return new WaitForSeconds(1);
		SetBallVelocity ();
	}
}
