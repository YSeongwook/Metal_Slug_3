using UnityEngine;

public class GlobalAudioPool : Singleton<GlobalAudioPool>
{
    [SerializeField] GameObject soundPrefab;
    private ObjectPool audioSourcePool;

    void Awake()
    {
        DontDestroyOnLoad(this);
        audioSourcePool = gameObject.AddComponent<ObjectPool>();
        if (soundPrefab != null)
        {
            audioSourcePool.pooledObject = soundPrefab;
        }
        else
        {
            soundPrefab = Resources.Load<GameObject>("Sound");
            audioSourcePool.pooledObject = soundPrefab;
        }

        audioSourcePool.pooledAmount = 10;
        audioSourcePool.willGrow = true;
    }

    public AudioSource GetAudioSource()
    {
        return audioSourcePool.GetPooledObject().GetComponent<AudioSource>();
    }
}