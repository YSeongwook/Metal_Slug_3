using Unity.VisualScripting;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private HealthManager healthManager;
    public Sprite destroyedSprite;

    private SpriteRenderer sr;
    private Collider2D cl;
    // private BlinkingSprite blinkingSprite;

    private void Start()
    {
        // blinkingSprite = GetComponent<BlinkingSprite>();
        sr = GetComponent<SpriteRenderer>();
        cl = GetComponent<Collider2D>();
        registerHealth();
    }

    private void registerHealth()
    {
        healthManager = GetComponent<HealthManager>();
        // register health delegate
        healthManager.OnDestroy += OnDestroy;
    }

    void OnDestroy()
    {
        sr.sprite = destroyedSprite;
        cl.enabled = false;
        // blinkingSprite.Stop();
    }
}
