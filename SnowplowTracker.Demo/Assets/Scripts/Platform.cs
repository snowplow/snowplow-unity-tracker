using System;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float speed = 200.0f;

    /// <summary>
    /// Processes Touch and Keyboard input to move platform
    /// </summary>
    void FixedUpdate()
    {
        float horizontalInput = 0.0f;
        if (Application.isMobilePlatform) {
            if (Input.touchCount > 0) {
                var touchPosition = Input.GetTouch(0).position;
                var screenTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Camera.main.nearClipPlane));
                if (screenTouchPosition.x > transform.position.x) {
                    horizontalInput = 1.0f;
                }
                else {
                    horizontalInput = -1.0f;
                }
            }
        }
        else {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        GetComponent<Rigidbody2D>().velocity = Vector2.right * horizontalInput * speed;
    }
}
