using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterTheLocusts : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            CameraManager.Instance.AfterLocusts();
            Destroy(gameObject);
        }
    }
}
