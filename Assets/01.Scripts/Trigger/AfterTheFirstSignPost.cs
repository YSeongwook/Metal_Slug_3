using UnityEngine;

public class AfterTheFirstSignPost : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            CameraManager.Instance.AfterSunkSignPost();
            Destroy(gameObject);
        }
    }
}
