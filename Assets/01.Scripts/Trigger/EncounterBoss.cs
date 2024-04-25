using UnityEngine;

public class EncounterBoss : MonoBehaviour
{
    public GameObject hugeHermit;
    public GameObject spawnWaterWave;
    public GameObject firstPart;
    public GameObject secondPart;
    public Animator warpTubeOut;
    public GameObject warpTubeCover;
    private bool encounter = false;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(!encounter)
        {
            if (GameManager.Instance.IsPlayer(collider))
            {
                CameraManager.Instance.AfterBossSpawn();

                firstPart.SetActive(false);
                warpTubeCover.SetActive(false);
                warpTubeOut.SetTrigger("Hide");
                Invoke("DisableSecondPart", 1f);

                encounter = true;
            }
        }
    }

    public void DisableSecondPart()
    {
        spawnWaterWave.SetActive(true);
        hugeHermit.SetActive(true);
        secondPart.SetActive(false);

        HUDManager.Instance.ResetTime();
    }
}
