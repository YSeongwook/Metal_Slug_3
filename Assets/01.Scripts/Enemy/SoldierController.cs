using System.Collections;
using _01.Scripts.Sound;
using _01.Scripts.Utils;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    [Header("Enemy information")]
    public float speed = 1f;
    public bool isMovable = true;
    public bool canMelee = true;
    public AudioClip[] deathClip;
    public AudioClip meleeAttackClip;
    public AudioClip rangeAttackClip;
    
    private GameObject _followPlayer;
    private HealthManager _healthManager;
    private Blink _enemyBlink;

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
    public bool facingRight = false;
    
    private const float ChangeSign = -1;
    private Rigidbody2D _rb;
    private Animator _animator;

    //Enemy gravity
    public bool collidingDown = false;

    [Header("Time shoot")]
    public float rangedDelta = 2f;
    public float fireDelta = 0.5f;
    private float _shotTime = 0.0f;
    private float _nextFire = 0.8f;  // 공격 쿨타임

    private bool _canFall = false;

    // 충돌 시 플레이어를 넉백할 힘의 크기
    public float knockbackForce = 5f;
    
    // 애니메이터 파라미터 캐시 처리
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Knifing = Animator.StringToHash("Knifing");
    private static readonly int ThrowingGrenade = Animator.StringToHash("ThrowingGrenade");
    private static readonly int IsDying = Animator.StringToHash("isDying");

    private void Start()
    {
        Initialize();
        registerHealth();
        checkCanFall();
        _enemyBlink = GetComponent<Blink>();
    }

    private void Initialize()
    {
        _followPlayer = GameManager.Instance.GetPlayer();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void registerHealth()
    {
        _healthManager = GetComponent<HealthManager>();
        _healthManager.onDead += OnDead;
    }

    private void checkCanFall()
    {
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.name != "isFalling") continue;
            
            _canFall = true;
            break;
        }
    }

    private void FixedUpdate()
    {
        if (_healthManager.IsAlive())
        {
            FlipShoot();
            if (_canFall) _animator.SetBool(IsFalling, !collidingDown);

            float playerDistance = GetPlayerDistance();

            // 플레이어가 살아있다면 추적 및 공격
            if (_followPlayer.GetComponent<HealthManager>().IsAlive())
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
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(Knifing, false);
                _animator.SetBool(ThrowingGrenade, false);
            }

            FlipEnemy(playerDistance);
        }
        else
        {
            _rb.velocity = Vector2.zero;
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
        _animator.SetBool(Knifing, true);
        _animator.SetBool(ThrowingGrenade, false);

        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        _shotTime += Time.deltaTime;

        if (_shotTime <= _nextFire) return;
        
        _nextFire = _shotTime + fireDelta;

        if (Mathf.Abs(GetComponent<SpriteRenderer>().bounds.SqrDistance(_followPlayer.transform.position)) <= meleeDistance)
        {
            _followPlayer.GetComponent<HealthManager>().OnHitByProjectile(projectile);

            if (meleeAttackClip) SoundManager.Instance.PlayEnemyAttackAudio(meleeAttackClip);
        }

        _nextFire -= _shotTime;
        _shotTime = 0.0f;
    }

    private void RangedAttack()
    {
        _animator.SetBool(ThrowingGrenade, true);
        _animator.SetBool(Knifing, false);

        if (_rb && !canMelee)
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        else
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        _shotTime += Time.deltaTime;

        if (_shotTime <= _nextFire) return;
        
        _nextFire = _shotTime + rangedDelta;

        StartCoroutine(WaitSecondaryAttack());

        _nextFire -= _shotTime;
        _shotTime = 0.0f;
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

        yield return new WaitForSeconds(0.6f);

        _enemyBlink.BlinkPlease(SoldierDeath);

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
        _rb.velocity = Vector2.zero;

        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("Water Dead") || col.collider.CompareTag("World"))
        {
            collidingDown = true;
            _animator.SetBool(IsFalling, false);
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
            _healthManager.onDead();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Walkable") || col.collider.CompareTag("Marco Boat") || col.collider.CompareTag("World"))
        {
            collidingDown = false;
            _animator.SetBool(IsFalling, true);
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
