using System.Collections;
using UnityEngine;

public class RunningTarget : MonoBehaviour
{
    float _speedX;
    float _speedY;
    bool _running = false;
    private Transform _target;

    public void SetRunning(bool running)
    {
        _running = running;
    }

    public void SetSpeed(float speedX = 0f, float speedY = 0f)
    {
        _speedX = speedX;
        _speedY = speedY;
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsGameOver() && _running)
        {
            transform.position = new Vector2(transform.position.x + _speedX * Time.deltaTime, transform.position.y + _speedY * Time.deltaTime);
        }
    }
}
