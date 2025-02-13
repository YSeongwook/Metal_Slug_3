using System.Collections;
using _01.Scripts.Utils;
using UnityEngine;

namespace _01.Scripts.Enemy.Boss
{
    public class BossController : MonoBehaviour
    {
        [Header("Enemy information")]
        public bool isMovable = true;
        public Transform bossSpawner;
        public GameObject normalBombSpawner;
        public GameObject heavyBombSpawner;
    
        private GameObject _followPlayer;
        private HealthManager _healthManager;
        private float _maxHp;
        private bool _isSpawned = false;
        private bool _isDie = false;
        
        [Header("Speeds")]
        public float speed = 1f;

        private const float ChargingSpeed = 0f;
        private const float RestSpeed = 0.10f;
        private const float SprintSpeed = 4f;
        private const float InitialSpeed = 1f;

        [Header("Throwable")]
        public GameObject normalFire;
        public GameObject heavyBomb;

        [Header("Enemy activation")]
        private Rigidbody2D _rb;
        private Animator _animator;

        [Header("Time shoot")]
        public float fireDelta = 0.5f;
        private float _shotTime = 0.0f;
        private float _nextFire = 2f;

        [Header("Time sprint")]
        private bool _canSprint = true;

        [Header("Camera")]
        public Parallaxing parallax;
        public RunningTarget runningTarget;

        [Header("Water Wave")]
        public GameObject waterWave;

        // 애니메이터 파라미터 캐시 처리
        private static readonly int IsHalfHealth = Animator.StringToHash("isHalfHealth");
        private static readonly int Spawn1 = Animator.StringToHash("Spawn");
        private static readonly int OnDestroy = Animator.StringToHash("onDestroy");
        private static readonly int IsDying = Animator.StringToHash("isDying");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
            _followPlayer = GameManager.Instance.GetPlayer();

            registerHealth();
            _maxHp = _healthManager.MaxHp;
            _canSprint = false;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsGameOver()) return;

            if (!_isSpawned && _followPlayer.transform.position.x >= bossSpawner.position.x)
            {
                _isSpawned = true;
                SoundManager.Instance.StartBossAudio();
                parallax.setActive(false);
                StartCoroutine(Spawn());
            }

            if (!_isSpawned) return;
            if (!_healthManager.IsAlive()) return;
            
            // Check health
            if (_healthManager.CurrentHp <= _maxHp / 2)
            {
                StartCoroutine(HalfHealth());
            }

            // Run and attack
            if (_rb && isMovable)
            {
                _rb.MovePosition(_rb.position + new Vector2(1 * speed, 0) * Time.deltaTime);
            }

            if (_canSprint && Random.Range(0, 100) < 30) // 30% chance of sprint
            {
                _canSprint = false;
                StartCoroutine(Sprint());
            }

            if (_healthManager.CurrentHp > _maxHp / 2)
            {
                _shotTime = _shotTime + Time.deltaTime;

                if (_shotTime > _nextFire)
                {
                    _nextFire = _shotTime + fireDelta;

                    StartCoroutine(WaitFire(normalFire));

                    _nextFire = _nextFire - _shotTime;
                    _shotTime = 0.0f;
                }
            }
            else if (_healthManager.CurrentHp <= _maxHp / 2)
            {
                _shotTime = _shotTime + Time.deltaTime;

                if (_shotTime > _nextFire)
                {
                    _nextFire = _shotTime + fireDelta;

                    StartCoroutine(WaitFire(heavyBomb));

                    _nextFire = _nextFire - _shotTime;
                    _shotTime = 0.0f;
                }
            }
        }

        private void registerHealth()
        {
            _healthManager = GetComponent<HealthManager>();
            _healthManager.onDead += OnDead;
        }

        private IEnumerator Spawn()
        {
            EventManager<BossEvents>.TriggerEvent(BossEvents.BossSpawn);

            foreach (Transform child in waterWave.transform)
            {
                Animator animator = child.GetComponent<Animator>();
                if (animator != null)
                {
                    // Animator 컴포넌트가 있다면 원하는 애니메이션 재생 등의 작업 수행
                    animator.SetTrigger(Spawn1);
                }
            }

            yield return new WaitForSeconds(2f);

            _rb.simulated = true;

            runningTarget.SetRunning(true);

            yield return new WaitForSeconds(1f);

            runningTarget.SetSpeed(InitialSpeed);
        }

        private IEnumerator HalfHealth()
        {
            _animator.SetBool(IsHalfHealth, true);
            heavyBombSpawner.transform.position.Set(0.55f, 0.3f, 0);  // 이 부분 수정해서 Heavy Bomb 위치 수정해야함

            yield return new WaitForSeconds(1f);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider == null) return;
            
            if (GameManager.Instance.IsPlayer(col))
            {
                _followPlayer.GetComponent<Rigidbody2D>().AddForce(new Vector2(3f, 0f), ForceMode2D.Impulse);
            }
            else if (col.collider.CompareTag("Enemy"))
            {
                col.collider.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(2f, 0f), ForceMode2D.Impulse);
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject == null) return;
            
            if (col.CompareTag("Walkable") || col.CompareTag("World"))
            {
                GameObject bridge = col.gameObject;
                StartCoroutine(DestroyBridge(bridge));
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.gameObject == null) return;
            
            if (col.CompareTag("Walkable") || col.CompareTag("World"))
            {
                GameObject bridge = col.gameObject;
                StartCoroutine(DestroyBridge(bridge));
            }
        }

        private IEnumerator WaitFire(GameObject throwableObj)
        {
            yield return new WaitForSeconds(0.1f);
            if(_healthManager.CurrentHp <= _maxHp / 2)
            {
                Instantiate(throwableObj, heavyBombSpawner.transform.position, heavyBombSpawner.transform.rotation);
            } else
            {
                Instantiate(throwableObj, normalBombSpawner.transform.position, normalBombSpawner.transform.rotation);
            }

            yield return new WaitForSeconds(0.15f);
        }

        private IEnumerator DestroyBridge(GameObject bridge)
        {
            if (bridge)
            {
                if (bridge) bridge.GetComponent<Animator>().SetBool(OnDestroy, true);
                yield return new WaitForSeconds(0.2f);

                if (bridge) bridge.GetComponent<Collider2D>().enabled = false;
                yield return new WaitForSeconds(0.5f);

                if (bridge) bridge.GetComponent<Animator>().SetBool(OnDestroy, false);
                Destroy(bridge);
            }
        }

        private void OnDead()
        {
            if (_isDie) return;
        
            this.GetComponent<Animator>().SetBool(IsDying, true);
            _isDie = true;

            StopCoroutine(nameof(Sprint));
            StopCoroutine(nameof(WaitFire));
            GameManager.Instance.PlayerWin();
            StopBossCoroutines();
        
            EventManager<MissionEvents>.TriggerEvent(MissionEvents.MissionSuccess);
        }

        private void StopBossCoroutines()
        {
            StopAllCoroutines();
        }

        private IEnumerator Sprint()
        {
            speed = ChargingSpeed;
            runningTarget.SetSpeed(speed);
            yield return new WaitForSeconds(1.5f);
            runningTarget.SetRunning(true);
            speed = SprintSpeed;
            runningTarget.SetSpeed(speed);
            yield return new WaitForSeconds(1.2f);
            speed = RestSpeed;
            runningTarget.SetSpeed(speed);
            yield return new WaitForSeconds(1f);
            speed = InitialSpeed;
            runningTarget.SetSpeed(speed);
            yield return new WaitForSeconds(5f); // wait until next possible sprint
            _canSprint = true;
        }
    }
}
