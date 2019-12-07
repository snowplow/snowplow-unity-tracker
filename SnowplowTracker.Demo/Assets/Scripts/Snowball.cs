using SnowplowTracker.Events;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    public float speed = 100.0f;

    public GameObject platform;

    /// <summary>
    /// Gives Snowball initial velocity
    /// </summary>
    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.up * speed;
    }

    /// <summary>
    /// Checks if Snowball has fallen off bottom of screen
    /// </summary>
    private void Update() {
        var viewportPoint = Camera.main.WorldToViewportPoint(transform.position, Camera.main.stereoActiveEye);

        if (viewportPoint.x <= 0 || viewportPoint.x >= 1 || viewportPoint.y <= 0 || viewportPoint.y >= 1)
        {
            transform.position = new Vector3(platform.transform.position.x, -90, 0);
            GetComponent<Rigidbody2D>().velocity = Vector2.down * speed;

            TrackerManager.SnowplowTracker.Track(
                new Structured()
                    .SetCategory("UnityDemo")
                    .SetAction("Gameplay")
                    .SetLabel("SnowballOutOfBounds")
                    .SetCustomContext(TrackerManager.GetExampleContextList())
                    .Build());     
        }
    }

    /// <summary>
    /// Checks if Snowball has collided with platform and alters velocity to "bounce" off the platform
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.name == "Platform")
        {
            float newHorizontalDirection = calculateBounceDirection(transform.position, other.transform.position, other.collider.bounds.size.x);

            GetComponent<Rigidbody2D>().velocity = new Vector2(newHorizontalDirection, 1).normalized * speed;
        }
    }

    /// <summary>
    /// Simple calculate to calculate new horizontal direction for snowball based on location of collision
    /// </summary>
    /// <param name="ballPosition"></param>
    /// <param name="platformPosition"></param>
    /// <param name="platformWidth"></param>
    /// <returns></returns>
    private float calculateBounceDirection(Vector2 ballPosition, Vector2 platformPosition, float platformWidth)
    {
        return (ballPosition.x - platformPosition.x) / platformWidth;
    }
}
