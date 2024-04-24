using UnityEngine;

public class HippieMove : MonoBehaviour
{
    public GameObject player;
    private Rigidbody2D rb;

    public bool isMovable = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if(isMovable) MoveForward();
    }

    public void MoveForward(float vel = 10)
    {
        // 항상 왼쪽으로 이동하도록 방향을 설정합니다.
        Vector2 movementDirection = Vector2.left;

        // 설정된 방향과 속도를 사용하여 이동합니다.
        rb.velocity = movementDirection * vel * 20 * Time.deltaTime;
    }
}
