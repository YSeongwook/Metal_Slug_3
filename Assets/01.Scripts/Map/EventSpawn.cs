using System.Collections;
using UnityEngine;

public class EventSpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemyPrefabRandom0;
    public GameObject enemyPrefabRandom1;
    public GameObject enemyPrefabRandom2;
    public GameObject enemyPrefabRandom3;

    public Transform spawnPoints;
    public float spawnDelay = 1f;
    public float startDelay = 1f;
    private int killCount;
    private int totalKillCount;

    public delegate void OnVoidEvent();
    public OnVoidEvent onFinish;
    private bool isFinished;

    void OnKill()
    {
        if (isFinished) return;

        killCount += 1;
        if (killCount >= spawnPoints.childCount)
        {
            isFinished = true;
            onFinish?.Invoke();
        }
    }

    public void Trigger()
    {
        StartCoroutine("Spawn");
    }

    public void TriggerRandomCollectible()
    {
        StartCoroutine("RandomSpawn");
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (Transform point in spawnPoints)
        {
            yield return new WaitForSeconds(spawnDelay);
            GameObject mob = Instantiate(enemyPrefab, point.position, point.rotation);
            mob.GetComponent<HealthManager>().onDead += OnKill;
        }
    }

    private IEnumerator RandomSpawn()
    {
        foreach (Transform point in spawnPoints)
        {
            int collectible = Random.Range(0, 4);
            switch (collectible)
            {
                case 0:
                    yield return new WaitForSeconds(spawnDelay);
                    GameObject collectible1 = Instantiate(enemyPrefabRandom0, point.position, point.rotation);
                    break;
                case 1:
                    yield return new WaitForSeconds(spawnDelay);
                    GameObject collectible2 = Instantiate(enemyPrefabRandom1, point.position, point.rotation);
                    break;
                case 2:
                    yield return new WaitForSeconds(spawnDelay);
                    GameObject collectible3 = Instantiate(enemyPrefabRandom2, point.position, point.rotation);
                    break;
                default:
                    yield return new WaitForSeconds(spawnDelay);
                    GameObject collectible4 = Instantiate(enemyPrefabRandom3, point.position, point.rotation);
                    break;
            }

        }
    }
}
