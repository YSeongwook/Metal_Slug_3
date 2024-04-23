using Unity.VisualScripting;
using UnityEngine;

public class TowerCrabHouse : MonoBehaviour
{
    public SpriteRenderer bgBoat;

    private void Start()
    {
        GetComponent<HealthManager>().OnDestroy += OnDestroy;
    }

    void OnDestroy()
    {
        CameraManager.Instance.AfterCrabTower();
        SoundManager.Instance.PlayMetalSlugDestroy2();
        bgBoat.sprite = null;
    }
}
