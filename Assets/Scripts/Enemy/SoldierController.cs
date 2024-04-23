using EventLibrary;
using System.Collections;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    [Header("Enemy information")]
    GameObject followPlayer;
    public float speed = 1f;
    public float attackDamage = 10f;
    public bool isMovable = true;
    public bool canMelee = true;
    public AudioClip[] deathClip;
    public AudioClip meleeAttackClip;
    public AudioClip rangeAttackClip;
    private HealthManager healthManager;
    private Blink enemyBlink;

    [Space(10)]
    public ProjectileProperties projectile;


    [Header("Throwable")]
    public GameObject projSpawner;
    public GameObject throwableObj;
    public bool canThrow = false;

    [Header("Enemy activation")]
    public float activationDistance = 1.8f;
    public float attackDistance = 1f;         //Far attack
    public float meleeDistance = 1f;          //Near attack
    public const float CHANGE_SIGN = -1;
    private Rigidbody2D rb;
    private Animator animator;
    public bool facingRight = false;

    //Enemy gravity
    public bool collidingDown = false;
    Vector2 velocity = Vector2.zero;

    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.5f;
    private float nextFire = 0.8f;  // 공격 쿨타임
    public float rangedDelta = 2f;

    private bool canFall = false;

    // 충돌 시 플레이어를 넉백할 힘의 크기
    public float knockbackForce = 5f;

    private void Start()
    {
        Initialize();
        registerHealth();
        checkCanFall();
        enemyBlink = GetComponent<Blink>();
    }

    private void Initialize()
    {
        followPlayer = GameManager.Instance.GetPlayer();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void registerHealth()
    {
        healthManager = GetComponent<HealthManager>();
        healthManager.onDead += OnDead;
    }

    private void checkCanFall()
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == "isFalling")
            {
                canFall = true;
                break;
            }
        }
    }

    public void setFollow(GameObject follow)
    {
        followPlayer = follow;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver()) return;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver()) return;

        if (healthManager.IsAlive())
        {
            FlipShoot();
            if (canFall) animator.SetBool("isFalling", !collidingDown);

            float playerDistance = GetPlayerDistance();

            // 플레이어가 살아있다면 추적 및 공격
            if (followPlayer.GetComponent<HealthManager>().IsAlive() && !GameManager.Instance.IsGameOver())
            {
                if (playerDistance < activationDistance && collidingDown)
                {
                    if (Mathf.Abs(playerDistance) <= meleeDistance && canMelee)
                    {
                        MeleeAttack();
                    }
                    else if (Mathf.Abs(playerDistance) <= attackDistance && canThrow)
                    {
                        if(Mathf.Abs(playerDistance) >= meleeDistance && canMelee)
                        {
                            RangedAttack();
                        } 
                        else
                        {
                            MeleeAttack();
                        }

                    }
                    else
                    {
                        MoveToPlayer(playerDistance);
                    }
                }
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("Knifing", false);
                animator.SetBool("ThrowingGrenade", false);
            }

            FlipEnemy(playerDistance);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (GameManager.Instance.IsGameOver())
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("Knifing", false);
            animator.SetBool("ThrowingGrenade", false);
        }
    }

    private float GetPlayerDistance()
    {
        return transform.position.x - followPlayer.transform.position.x;
    }

    private void FlipEnemy(float playerDistance)
    {
        if ((playerDistance < 0 && !facingRight) || (playerDistance > 0 && facingRight)) Flip();
    }

    void MeleeAttack()
    {
        animator.SetBool("Knifing", true);
        animator.SetBool("ThrowingGrenade", false);

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        shotTime += Time.deltaTime;

        if (shotTime > nextFire)
        {
            nextFire = shotTime + fireDelta;

            if (Mathf.Abs(GetComponent<SpriteRenderer>().bounds.SqrDistance(followPlayer.transform.position)) <= meleeDistance)
            {
                followPlayer.GetComponent<HealthManager>().OnHitByProjectile(projectile);

                if (meleeAttackClip) SoundManager.Instance.PlayEnemyAttackAudio(meleeAttackClip);
            }

            nextFire -= shotTime;
            shotTime = 0.0f;
        }
    }

    void RangedAttack()
    {
        animator.SetBool("ThrowingGrenade", true);
        animator.SetBool("Knifing", false);

        if (rb && !canMelee)
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        else
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        shotTime += Time.deltaTime;

        if (shotTime > nextFire)
        {
            nextFire = shotTime + rangedDelta;

            StartCoroutine(WaitSecondaryAttack());

            nextFire -= shotTime;
            shotTime = 0.0f;
        }
    }

    void MoveToPlayer(float playerDistance)
    {
        if (rb && isMovable)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (collidingDown)
            {
                // rb.MovePosition(rb.position + new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance) * speed, rb.position.y) * Time.deltaTime);
                Vector2 movementDirection = new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance), 0f);

                rb.velocity = movementDirection * speed * 100 * Time.deltaTime;
            }

            animator.SetBool("isWalking", true);
            animator.SetBool("Knifing", false);
            animator.SetBool("ThrowingGrenade", false);
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        facingRight = !facingRight;
    }

    void FlipShoot()
    {
        if (projSpawner == null) return;

        if (facingRight)
        {
            //Fire right
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            //Fire left
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, -180);
        }
    }

    public void OnHit()
    {
        // StartCoroutine(Die());
    }

    private void OnDead()
    {
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        PlayDeathAudio();
        animator.SetTrigger("isDying");
        rb.velocity = Vector2.zero;

        if (rb) rb.isKinematic = true;
        if (GetComponent<BoxCollider2D>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (GetComponent<CapsuleCollider2D>())
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        yield return new WaitForSeconds(0.6f);

        enemyBlink.BlinkPlease(SoldierDeath);

        yield return new WaitForSeconds(1.2f);

        Destroy(gameObject);
    }

    private void SoldierDeath() { }

    private void PlayDeathAudio()
    {
        if (deathClip != null && deathClip.Length > 0)
        {
            // 랜덤한 인덱스를 선택합니다.
            int randomIndex = Random.Range(0, deathClip.Length);

            // 선택된 인덱스에 해당하는 클립을 재생합니다.
            AudioClip clipToPlay = deathClip[randomIndex];
            if (clipToPlay != null)
            {
                SoundManager.Instance.PlayEnemyDeathAudio(clipToPlay);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        rb.velocity = Vector2.zero;

        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("Water Dead") || col.collider.CompareTag("World"))
        {
            collidingDown = true;
            animator.SetBool("isFalling", false);
        }

        if (col.collider.CompareTag("Player") && col.collider.gameObject.GetComponent<HealthManager>().IsAlive())
        {
            // 플레이어 오브젝트 가져오기
            GameObject playerObject = col.gameObject;

            // 플레이어 오브젝트에 Rigidbody2D 컴포넌트가 있는지 확인
            Rigidbody2D playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                // 플레이어 오브젝트의 방향을 구함
                Vector2 knockbackDirection = playerObject.transform.position - transform.position;

                // 플레이어 오브젝트를 넉백시키는 힘을 가함
                playerRigidbody.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }

        else if (col.collider.CompareTag("Water Dead"))
        {
            healthManager.onDead();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("World"))
        {
            collidingDown = false;
            animator.SetBool("isFalling", true);
        }
    }

    private IEnumerator WaitSecondaryAttack()
    {
        yield return new WaitForSeconds(0.1f);
        if (rangeAttackClip) SoundManager.Instance.PlayEnemyAttackAudio(rangeAttackClip);
        Instantiate(throwableObj, projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.2f);
    }
}
