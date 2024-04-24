using UnityEngine;

public class OpenSunkBoatDoor : MonoBehaviour
{
    public GameObject sunkBoat;
    private bool isDone;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDone) return;

        if (GameManager.Instance.IsPlayer(collider))
        {
            isDone = true;
            sunkBoat.GetComponent<SunkBoatController>().OpenDoor();
        }
    }
}
