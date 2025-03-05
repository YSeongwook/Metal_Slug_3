using System.Collections;
using _01.Scripts.Utils;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy information")]
    public float speed = 1f;
    public bool isMovable = true;
    public bool canMelee = true;
    public AudioClip deathClip;
    public AudioClip meleeAttackClip;
    public AudioClip rangeAttackClip;
    public GameObject projSpawner;
    
    private GameObject _followPlayer;
    private HealthManager _healthManager;

    [Space(10)]
    public ProjectileProperties projectile;

    [Header("Throwable")]
    public GameObject throwableObj;
    public bool canThrow = false;

    [Header("Enemy activation")]
    public float activationDistance = 1.8f;
    public float attackDistance = 1f;         //Far attack
    public float meleeDistance = 1f;          //Near attack
    public const float ChangeSign = -1;
    public bool facingRight = false;
    
    private Rigidbody2D _rb;
    private Animator _animator;

    //Enemy gravity
    public bool collidingDown = false;

    [Header("Time shoot")]
    public float fireDelta = 0.5f;
    public float rangedDelta = 2f;
    
    private float _shotTime = 0.0f;
    private float _nextFire = 0.8f;  // 공격 쿨타임
    private bool _canFall = false;

    // 충돌 시 플레이어를 넉백할 힘의 크기
    public float knockbackForce = 5f;
    
    // 애니메이터 파라미터 캐싱
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsAttacking2 = Animator.StringToHash("isAttacking2");
    private static readonly int IsHitten = Animator.StringToHash("isHitten");
    private static readonly int IsDying = Animator.StringToHash("isDying");

    private void Start()
    {
        Initialize();
        RegisterHealth();
        CheckCanFall();
    }

    public void FixedUpdate()
    {
        if (_healthManager.IsAlive())
        {
            FlipShoot();
            if (_canFall) _animator.SetBool(IsFalling, !collidingDown);

            float playerDistance = GetPlayerDistance();

            // 플레이어가 살아있다면 추적 및 공격
            if(_followPlayer.GetComponent<HealthManager>().IsAlive())
            {
                if (playerDistance < activationDistance && collidingDown)
                {
                    if (Mathf.Abs(playerDistance) <= meleeDistance && canMelee)
                    {
                        MeleeAttack();
                    }
                    else if (Mathf.Abs(playerDistance) <= attackDistance && canThrow)
                    {
                        RangedAttack();
                    }
                    else
                    {
                        MoveToPlayer(playerDistance);
                    }
                }
            } 
            else
            {
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsAttacking, false);
                _animator.SetBool(IsAttacking2, false);
            }

            FlipEnemy(playerDistance);
        } 
        else
        {
            _rb.velocity = Vector2.zero;
        }

        if(GameManager.Instance.IsGameOver())
        {
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsAttacking, false);
            _animator.SetBool(IsAttacking2, false);
        }
    }
    
    private void Initialize()
    {
        _followPlayer = GameManager.Instance.GetPlayer();
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
            if (parameter.name != "isFalling") continue;
            
            _canFall = true;
            break;
        }
    }

    private float GetPlayerDistance()
    {
        return transform.position.x - _followPlayer.transform.position.x;
    }

    private void FlipEnemy(float playerDistance)
    {
        if ((playerDistance < 0 && !facingRight) || (playerDistance > 0 && facingRight)) Flip();
    }

    private void MeleeAttack()
    {
        _animator.SetBool(IsAttacking, true);
        _animator.SetBool(IsAttacking2, false);

        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        _shotTime += Time.deltaTime;

        if (_shotTime > _nextFire)
        {
            _nextFire = _shotTime + fireDelta;

            if (Mathf.Abs(GetComponent<SpriteRenderer>().bounds.SqrDistance(_followPlayer.transform.position)) <= meleeDistance)
            {
                _followPlayer.GetComponent<HealthManager>().OnHitByProjectile(projectile);

                if (meleeAttackClip) EventManager<SoundEventType>.TriggerEvent(SoundEventType.EnemyAttack, "enemyMeleeAttack");
            }

            _nextFire -= _shotTime;
            _shotTime = 0.0f;
        }
    }

    private void RangedAttack()
    {
        _animator.SetBool(IsAttacking2, true);
        _animator.SetBool(IsAttacking, false);

        if (_rb && !canMelee)
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        else
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        _shotTime += Time.deltaTime;

        if (_shotTime > _nextFire)
        {
            _nextFire = _shotTime + rangedDelta;

            StartCoroutine(WaitSecondaryAttack());

            _nextFire -= _shotTime;
            _shotTime = 0.0f;
        }
    }

    private void MoveToPlayer(float playerDistance)
    {
        if (_rb && isMovable)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (collidingDown)
            {
                Vector2 movementDirection = new Vector2(ChangeSign * Mathf.Sign(playerDistance), 0f);

                _rb.velocity = movementDirection * (speed * 100 * Time.deltaTime);
            }

            _animator.SetBool(IsWalking, true);
            _animator.SetBool(IsAttacking, false);
            _animator.SetBool(IsAttacking2, false);
        }
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
        _animator.SetTrigger(IsHitten);
    }

    private void OnDead()
    {
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        PlayDeathAudio();
        _animator.SetTrigger(IsDying);
        _rb.velocity = Vector2.zero;

        if (_rb) _rb.isKinematic = true;
        if (GetComponent<BoxCollider2D>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (GetComponent<CapsuleCollider2D>())
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        yield return new WaitForSeconds(1.6f);
        Destroy(gameObject);
    }

    private void PlayDeathAudio()
    {
        if (deathClip) EventManager<SoundEventType>.TriggerEvent(SoundEventType.EnemyDeath, "crabDeath");
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        _rb.velocity = Vector2.zero;

        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("Water Dead") || col.collider.CompareTag("World"))
        {
            collidingDown = true;
        }

        if ((col.collider.CompareTag("Player") && col.collider.gameObject.GetComponent<HealthManager>().IsAlive()) || col.collider.CompareTag("Enemy"))
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
            _healthManager.onDead();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("World"))
        {
            collidingDown = false;
        }
    }

    private IEnumerator WaitSecondaryAttack()
    {
        yield return new WaitForSeconds(0.1f);
        if (rangeAttackClip) EventManager<SoundEventType>.TriggerEvent(SoundEventType.EnemyAttack, "enemyRangeAttack");
        Instantiate(throwableObj, projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.2f);
    }
}
