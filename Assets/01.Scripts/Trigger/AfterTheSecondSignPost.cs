using UnityEngine;

public class AfterTheSecondSignPost : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            CameraManager.Instance.AfterBoatSignPost();
            Destroy(gameObject);
        }
    }
}
