using System.Collections;
using _01.Scripts.Camera;
using _01.Scripts.Sound;
using _01.Scripts.Utils;
using UnityEngine;

public class EnemyBoatController : MonoBehaviour
{
    public Sprite[] boatSprites;
    public Collider2D[] colliders;
    public Collider2D sunkCollider;
    public GameObject sunk;
    public GameObject explosion;
    public GameObject exhaust;
    public GameObject waterWave;

    private HealthManager healthManager;
    private Animator animator;
    private SpriteRenderer spriteRender;

    private bool destroyHalf = false;
    private bool destroy = false;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();

        registerHealth();
    }

    private void Update()
    {
        if (healthManager.CurrentHp > 60)
        {
            spriteRender.sprite = boatSprites[0];
            //Nothing
        }
        else if (healthManager.CurrentHp > 45)
        {
            spriteRender.sprite = boatSprites[1];
            EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayEffect, "metalSlugDestroy3");
        }
        else if (healthManager.CurrentHp > 30)
        {
            spriteRender.sprite = boatSprites[2];
            EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayEffect, "metalSlugDestroy1");
        }
        else if (healthManager.CurrentHp > 15)
        {
            spriteRender.sprite = boatSprites[3];
            EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayEffect, "metalSlugDestroy1");
        }
        else if (healthManager.CurrentHp > 0)
        {
            // 한 번만 실행하게끔 설정
            if(!destroyHalf) DestroyHalf();
        } 
        else
        {
            if(!destroy) OnExplosion();
            destroy = true;

            Invoke("DestroyBoat", 1f);
        }
    }

    private void registerHealth()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void DestroyHalf()
    {
        OnExplosion();

        spriteRender.enabled = false;

        sunk.SetActive(true);
        exhaust.SetActive(false);
        waterWave.SetActive(false);

        foreach(Collider2D c in colliders)
        {
            c.enabled = false;
        }

        sunkCollider.enabled = true;

        GetComponent<PolygonCollider2D>().enabled = false;

        if (rb) rb.isKinematic = true;

        Invoke("OffExplosion", 2f);
    }

    void DestroyBoat()
    {
        Destroy(gameObject);
        CameraManager.Instance.SwitchZ2AtoZ2B();
    }

    void OnExplosion()
    {
        explosion.SetActive(true);
        explosion.GetComponent<Animator>().SetBool("Destroy", true);
        EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayEffect, "metalSlugDestroy2");

        destroyHalf = true;
    }

    void OffExplosion()
    {
        explosion.GetComponent<Animator>().SetBool("Destroy", false);
        explosion.SetActive(false);
    }
}
