using UnityEngine;

public class EncounterBoss : MonoBehaviour
{
    public GameObject firstPart;
    public GameObject secondPart;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameManager.Instance.IsPlayer(collider))
        {
            CameraManager.Instance.AfterBossSpawn();

            firstPart.SetActive(false);
            Invoke("DisableSecondPart", 5f);

            Destroy(gameObject);
        }
    }

    void DisableSecondPart()
    {
        secondPart.SetActive(false);
    }
}
