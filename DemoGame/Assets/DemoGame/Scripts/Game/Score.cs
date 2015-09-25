using UnityEngine;
using System.Collections;
using SnowplowTracker.Events;

public class Score : MonoBehaviour {

	private static TextMesh scoreText;
	private static int playerScore = 0;
	private static int enemyScore = 0;
	
	void Start () {
		scoreText = GameObject.Find("ScoreText").GetComponent<TextMesh>();
	}

	/// <summary>
	/// Increments the player score.
	/// </summary>
	public static void IncrementPlayer() {
		playerScore++;
		UpdateScore ();

		MainManager.tracker.Track (new Structured ().SetCategory("GameScene").SetAction("Player1Scored").SetLabel("Player 1 score: " + playerScore).Build ());
	}

	/// <summary>
	/// Increments the enemy score.
	/// </summary>
	public static void IncrementEnemy() {
		enemyScore++;
		UpdateScore ();

		MainManager.tracker.Track (new Structured ().SetCategory("GameScene").SetAction("Player2Scored").SetLabel("Player 2 score: " + enemyScore).Build ());
	}

	/// <summary>
	/// Updates the score.
	/// </summary>
	private static void UpdateScore() {
		scoreText.text = playerScore + " : " + enemyScore;
	}

	/// <summary>
	/// Gets the player score.
	/// </summary>
	/// <returns>The player score.</returns>
	public static int GetPlayerScore() {
		return playerScore;
	}

	/// <summary>
	/// Gets the enemy score.
	/// </summary>
	/// <returns>The enemy score.</returns>
	public static int GetEnemyScore() {
		return enemyScore;
	}

	/// <summary>
	/// Reset this instance.
	/// </summary>
	public static void Reset() {
		playerScore = 0;
		enemyScore = 0;
		UpdateScore ();
	}
}
