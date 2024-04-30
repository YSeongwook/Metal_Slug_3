using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Enemy information")]
    GameObject followPlayer;
    public float attackDamage = 25f;
    public bool isMovable = true;
    public AudioClip deathClip;
    private HealthManager healthManager;
    private float maxHP;
    public Transform bossSpawner;
    public GameObject projSpawner;
    private float spawnOffsetUp = 0.5f;

    public GameObject boat;
    private bool isSpawned = false;

    [Header("Speeds")]
    public float speed = 1f;
    private float chargingSpeed = 0f;
    private float restSpeed = 0.10f;
    private float sprintSpeed = 4f;
    private float initialSpeed = 1f;

    [Header("Throwable")]
    public GameObject normalFire;
    public GameObject heavyBomb;
    public bool canThrow = true;

    [Header("Enemy activation")]
    public const float CHANGE_SIGN = -1;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.5f;
    private float nextFire = 2f;

    [Header("Time sprint")]
    private bool canSprint = true;

    [Header("Camera")]
    public Parallaxing parallax;
    public RunningTarget runningTarget;

    [Header("Water Wave")]
    public GameObject waterWave;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        followPlayer = GameManager.Instance.GetPlayer();

        registerHealth();
        maxHP = healthManager.MaxHP;
        canSprint = false;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver()) return;

        if (!isSpawned && followPlayer.transform.position.x >= bossSpawner.position.x)
        {
            isSpawned = true;
            SoundManager.Instance.StartBossAudio();
            parallax.setActive(false);
            StartCoroutine(Spawn());
        }

        if (isSpawned)
        {
            if (healthManager.IsAlive())
            {
                // Check health
                if (healthManager.CurrentHP <= maxHP / 2)
                {
                    StartCoroutine(HalfHealth());
                }

                // Run and attack
                float playerDistance = transform.position.x - followPlayer.transform.position.x;
                if (rb && isMovable)
                {
                    rb.MovePosition(rb.position + new Vector2(1 * speed, 0) * Time.deltaTime);
                    // Vector2 movementDirection = new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance), 0f);
                    // rb.velocity = movementDirection * speed * 50 * Time.deltaTime;
                }

                if (canSprint && Random.Range(0, 100) < 30) // 30% chance of sprint
                {
                    canSprint = false;
                    StartCoroutine(Sprint());
                }

                if (!(healthManager.CurrentHP <= maxHP / 2))
                {
                    shotTime = shotTime + Time.deltaTime;

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        StartCoroutine(WaitFire(normalFire));

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }
                }
                else if (healthManager.CurrentHP <= maxHP / 2)
                {
                    shotTime = shotTime + Time.deltaTime;

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        StartCoroutine(WaitFire(heavyBomb));

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }
                }
            }
        }
    }

    private void registerHealth()
    {
        healthManager = GetComponent<HealthManager>();
        healthManager.onDead += OnDead;
    }

    private IEnumerator Die()
    {
        SoundManager.Instance.PlayMetalSlugDestroy3();
        animator.SetBool("isDying", true);
        if (rb) rb.isKinematic = true;

        if (GetComponent<BoxCollider2D>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (GetComponent<CapsuleCollider2D>())
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        yield return new WaitForSeconds(1.8f);
        SoundManager.Instance.PlayMetalSlugDestroy1();
        Destroy(gameObject);
    }

    public void setFollow(GameObject follow)
    {
        followPlayer = follow;
    }

    private IEnumerator Spawn()
    {
        foreach (Transform child in waterWave.transform)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                // Animator 컴포넌트가 있다면 원하는 애니메이션 재생 등의 작업 수행
                animator.SetTrigger("Spawn");
            }
        }

        yield return new WaitForSeconds(2f);

        rb.simulated = true;

        runningTarget.SetRunning(true);

        yield return new WaitForSeconds(1f);

        runningTarget.SetSpeed(initialSpeed);
    }

    private IEnumerator HalfHealth()
    {
        animator.SetBool("isHalfHealth", true);
        projSpawner.transform.position.Set(-0.63f, -0.21f, 0);
        yield return new WaitForSeconds(1f);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider != null)
        {
            if (GameManager.Instance.IsPlayer(col))
            {
                // followPlayer.GetComponent<HealthManager>().Hit(attackDamage);   // 플레이어는 1대만 맞아도 죽기 때문에 필요없음
                followPlayer.GetComponent<Rigidbody2D>().AddForce(new Vector2(3f, 0f), ForceMode2D.Impulse);
            }
            else if (col.collider.CompareTag("Enemy"))
            {
                // collision.collider.gameObject.GetComponent<HealthManager>().onHit(attackDamage);
                col.collider.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(2f, 0f), ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject != null)
        {
            if (col.CompareTag("Walkable") || col.CompareTag("World"))
            {
                GameObject bridge = col.gameObject;
                StartCoroutine(DestroyBridge(bridge));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject != null)
        {
            if (col.CompareTag("Walkable") || col.CompareTag("World"))
            {
                GameObject bridge = col.gameObject;
                StartCoroutine(DestroyBridge(bridge));
            }
        }
    }

    private IEnumerator WaitFire(GameObject throwableObj)
    {
        yield return new WaitForSeconds(0.1f);
        Instantiate(throwableObj, projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator DestroyBridge(GameObject bridge)
    {
        if (bridge)
        {
            if (bridge) bridge.GetComponent<Animator>().SetBool("onDestroy", true);
            yield return new WaitForSeconds(0.2f);

            if (bridge) bridge.GetComponent<Collider2D>().enabled = false;
            yield return new WaitForSeconds(0.5f);

            if (bridge) bridge.GetComponent<Animator>().SetBool("onDestroy", false);
            Destroy(bridge);
        }
    }

    private void OnDead()
    {
        this.GetComponent<Animator>().SetBool("isDying", true);

        StopCoroutine("Sprint");
        StopCoroutine("WaitFire");
        GameManager.Instance.PlayerWin();
        StopBossCoroutines();
    }

    private void StopBossCoroutines()
    {
        StopAllCoroutines();
    }

    private IEnumerator Sprint()
    {
        speed = chargingSpeed;
        runningTarget.SetSpeed(speed);
        yield return new WaitForSeconds(1.5f);
        runningTarget.SetRunning(true);
        speed = sprintSpeed;
        runningTarget.SetSpeed(speed);
        yield return new WaitForSeconds(1.2f);
        speed = restSpeed;
        runningTarget.SetSpeed(speed);
        yield return new WaitForSeconds(1f);
        speed = initialSpeed;
        runningTarget.SetSpeed(speed);
        yield return new WaitForSeconds(5f); // wait until next possible sprint
        canSprint = true;
    }
}
