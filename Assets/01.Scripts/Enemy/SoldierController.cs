using System.Collections;
using _01.Scripts.Utils;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    [Header("Enemy Information")]
    public float speed = 1f;
    public bool isMovable = true;
    public bool canMelee = true;
    // 기존 AudioClip 배열은 사운드 이벤트로 대체할 예정이므로 삭제 가능
    public AudioClip[] deathClip;

    private GameObject _followPlayer;
    private HealthManager _healthManager;
    private Blink _enemyBlink;
    private HealthManager _playerHealthManager;

    [Space(10)]
    public ProjectileProperties projectile;

    [Header("Throwable")]
    public GameObject projSpawner;
    public GameObject throwableObj;
    public bool canThrow = false;

    [Header("Enemy Activation")]
    public float activationDistance = 1.8f;
    public float attackDistance = 1f; // Far attack
    public float meleeDistance = 1f;  // Near attack
    public bool facingRight = false;
    
    private const float ChangeSign = -1;
    private Rigidbody2D _rb;
    private Animator _animator;

    // Enemy gravity
    public bool collidingDown = false;

    [Header("Attack Timing")]
    public float rangedDelta = 2f;
    public float fireDelta = 0.5f;
    private float _shotTime = 0.0f;
    private float _nextFire = 0.8f;  // 공격 쿨타임

    private bool _canFall = false;

    // Knockback force when colliding with the player
    public float knockbackForce = 5f;
    
    // Animator parameter caching
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Knifing = Animator.StringToHash("Knifing");
    private static readonly int ThrowingGrenade = Animator.StringToHash("ThrowingGrenade");
    private static readonly int IsDying = Animator.StringToHash("isDying");

    private void Start()
    {
        Initialize();
        RegisterHealth();
        CheckCanFall();
        _enemyBlink = GetComponent<Blink>();
    }

    private void Initialize()
    {
        _followPlayer = GameManager.Instance.GetPlayer();
        _playerHealthManager = _followPlayer.GetComponent<HealthManager>();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void RegisterHealth()
    {
        _healthManager = GetComponent<HealthManager>();
        _healthManager.onDead += OnDead;
    }

    private void CheckCanFall()
    {
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.name == "isFalling")
            {
                _canFall = true;
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        FlipShoot();
        if (_canFall) _animator.SetBool(IsFalling, !collidingDown);

        float playerDistance = GetPlayerDistance();
        if (_playerHealthManager.IsAlive())
        {
            ProcessEnemyBehavior(playerDistance);
        }
        else
        {
            StopMove();
        }
        FlipEnemy(playerDistance);
    }
    
    // 적 행동 처리
    private void ProcessEnemyBehavior(float playerDistance)
    {
        if (playerDistance < activationDistance && collidingDown)
        {
            float absDistance = Mathf.Abs(playerDistance);
            if (absDistance <= meleeDistance && canMelee)
            {
                MeleeAttack();
            }
            else if (absDistance <= attackDistance && canThrow)
            {
                if (absDistance >= meleeDistance && canMelee)
                    RangedAttack();
                else
                    MeleeAttack();
            }
            else
            {
                MoveToPlayer(playerDistance);
            }
        }
    }

    private float GetPlayerDistance()
    {
        return transform.position.x - _followPlayer.transform.position.x;
    }

    private void FlipEnemy(float playerDistance)
    {
        if ((playerDistance < 0 && !facingRight) || (playerDistance > 0 && facingRight))
            Flip();
    }

    // 공격 관련 타이밍 로직을 별도 메서드로 분리하여 재사용성을 높임
    private bool IsAttackReady()
    {
        _shotTime += Time.deltaTime;
        if (_shotTime > _nextFire)
        {
            _nextFire = _shotTime; // _nextFire 업데이트 (상대적 시간 계산)
            _shotTime = 0.0f;
            return true;
        }
        return false;
    }

    private void MeleeAttack()
    {
        _animator.SetBool(Knifing, true);
        _animator.SetBool(ThrowingGrenade, false);

        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        if (!IsAttackReady()) return;
        
        if (Mathf.Abs(GetComponent<SpriteRenderer>().bounds.SqrDistance(_followPlayer.transform.position)) <= meleeDistance)
        {
            _followPlayer.GetComponent<HealthManager>().OnHitByProjectile(projectile);
            // 기존 직접 사운드 호출 대신 이벤트 발생
            EventManager<SoundEventType>.TriggerEvent<string>(SoundEventType.EnemyAttack, "soldier_melee");
        }
    }

    private void RangedAttack()
    {
        _animator.SetBool(ThrowingGrenade, true);
        _animator.SetBool(Knifing, false);

        if (_rb && !canMelee)
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        else
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (!IsAttackReady()) return;
        
        StartCoroutine(WaitSecondaryAttack());
    }

    private void MoveToPlayer(float playerDistance)
    {
        if (!_rb || !isMovable) return;
        
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (collidingDown)
        {
            Vector2 movementDirection = new Vector2(ChangeSign * Mathf.Sign(playerDistance), 0f);
            _rb.velocity = movementDirection * (speed * 100 * Time.deltaTime);
        }

        _animator.SetBool(IsWalking, true);
        _animator.SetBool(Knifing, false);
        _animator.SetBool(ThrowingGrenade, false);
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        facingRight = !facingRight;
    }

    private void FlipShoot()
    {
        if (projSpawner == null) return;
        projSpawner.transform.rotation = facingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, -180);
    }

    private void StopMove()
    {
        _animator.SetBool(IsWalking, false);
        _animator.SetBool(Knifing, false);
        _animator.SetBool(ThrowingGrenade, false);
    }

    private void OnDead()
    {
        // 적 사망 시 직접 SoundManager 호출 대신 이벤트 발생
        EventManager<SoundEventType>.TriggerEvent(SoundEventType.EnemyDeath, "soldierDeath");
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        _animator.SetTrigger(IsDying);
        _rb.velocity = Vector2.zero;
        if (_rb) _rb.isKinematic = true;
        DisableColliders();
        yield return new WaitForSeconds(0.6f);
        _enemyBlink.BlinkPlease(SoldierDeath);
        yield return new WaitForSeconds(1.2f);
        Destroy(gameObject);
    }

    private void DisableColliders()
    {
        Collider2D col = GetComponent<BoxCollider2D>() as Collider2D ?? GetComponent<CapsuleCollider2D>();
        if (col != null)
            col.enabled = false;
    }

    // 빈 메서드; Blink 이후 추가 처리 가능
    private void SoldierDeath() { }

    // 기존 PlayDeathAudio는 이벤트 기반으로 대체할 계획이므로 제거하거나 간략화 가능

    private IEnumerator WaitSecondaryAttack()
    {
        yield return new WaitForSeconds(0.1f);
        // 공격 사운드 이벤트 발생 (원거리 공격)
        EventManager<SoundEventType>.TriggerEvent(SoundEventType.EnemyAttack, "soldier_ranged");
        Instantiate(throwableObj, projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.2f);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        _rb.velocity = Vector2.zero;

        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") ||
            col.collider.CompareTag("Water Dead") || col.collider.CompareTag("World"))
        {
            collidingDown = true;
            _animator.SetBool(IsFalling, false);
        }
        if (col.collider.CompareTag("Player") && col.collider.gameObject.GetComponent<HealthManager>().IsAlive())
        {
            GameObject playerObject = col.gameObject;
            Rigidbody2D playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                Vector2 knockbackDirection = playerObject.transform.position - transform.position;
                playerRigidbody.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }
        else if (col.collider.CompareTag("Water Dead"))
        {
            _healthManager.onDead();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") ||
            col.collider.CompareTag("World"))
        {
            collidingDown = false;
            _animator.SetBool(IsFalling, true);
        }
    }
}
