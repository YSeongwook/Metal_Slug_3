using _01.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Input;

namespace _01.Scripts.Player
{
    public class PlayerController : Singleton<PlayerController>
    {
        private IObserver[] _observers;

        private AttackManager _attackManager; // 공격 매니저
        private AnimationManager _animManager; // 애니메이션 매니저

        [SerializeField] Animator topAnimator; // 사용할 top 애니메이터 컴포넌트
        [SerializeField] Animator bottomAnimator; // 사용할 bottom 애니메이터 컴포넌트

        [Space(10)] public GameObject waterStep;
        public Parachute parachute;

        [Space(10)] public BodyPosture body;
        private TimeUtils _timeUtils;

        public float moveSpeed = 3.5f; // 이동 속도           
        [SerializeField] float jumpForce = 70f; // 점프 힘
        [SerializeField] int maxJumps = 1; // 최대 점프 횟수

        private Vector2 _inputMovement = Vector2.zero;
        private int _jumpCount = 0; // 현재 점프 회수를 추적

        public float fixedZ = 0f; // 고정할 z축 값
        private float _rotationY = 0; // transform.rotation.eulerAngles.y;

        public bool IsRunning { get; set; } // 움직이고 있는지 여부
        public bool IsCrouched { get; set; } // 웅크리고 있는지 여부
        public bool IsLookUp { get; set; } // 위를 보고 있는지 여부
        public bool InTheAir { get; set; } // 공중에 떠 있는지 여부
        public bool IsAttacking { get; set; } // 현재 공격 중인지 여부
        public Vector2 LookingDirection { get; set; }

        private Rigidbody2D _playerRigidbody; // 사용할 리지드바디 컴포넌트
        private BoxCollider2D _collider; // 사용할 콜라이더 커포넌트

        private bool _parachuteActive = true;
        private bool _waterHitHandled = false;

        public ProjectileProperties projectile;

        // 애니메이터 파라미터 캐싱
        private static readonly int UpPressed = Animator.StringToHash("up_pressed");
        private static readonly int DownPressed = Animator.StringToHash("down_pressed");

        protected override void Awake()
        {
            base.Awake();
            EventManager<GameEvents>.StartListening(GameEvents.GameReset, GameReset);
        }

        private void Start()
        {
            // 게임 오브젝트로부터 사용할 컴포넌트들을 가져와 변수에 할당
            _observers = GetComponentsInChildren<IObserver>();
            _attackManager = GetComponentInChildren<AttackManager>();
            _animManager = GetComponent<AnimationManager>();

            _playerRigidbody = GetComponent<Rigidbody2D>();
            _timeUtils = GetComponent<TimeUtils>();
            _collider = GetComponent<BoxCollider2D>();

            IsCrouched = false;
            IsRunning = false;
            IsLookUp = false;
            InTheAir = false;

            _playerRigidbody.gravityScale = 0.2f;
        }

        private void Update()
        {
            _rotationY = transform.rotation.eulerAngles.y;

            // 캐릭터의 움직임 여부를 확인하여 isRun 파라미터를 설정
            _animManager.StartRunningAnim(IsRunning);

            // 위쪽 방향키 입력 처리: 눌리는 순간 topAnimator에 up_pressed를 true로 설정
            if (GetKeyDown(KeyCode.UpArrow) && !IsCrouched)
            {
                topAnimator.SetBool(UpPressed, true);
                LookingDirection = Vector2.up;
            }

            // 아래쪽 방향키 입력 처리: 눌리는 순간 topAnimator와 bottomAnimator에 down_pressed를 true로 설정
            if (GetKeyDown(KeyCode.DownArrow))
            {
                topAnimator.SetBool(DownPressed, true);
                bottomAnimator.SetBool(DownPressed, true);
                body = BodyPosture.Crouch;
                IsCrouched = true;
            }

            CheckLookingDirection();

            // 키를 뗄 때 애니메이터 파라미터를 false로 전환
            if (GetKeyUp(KeyCode.UpArrow))
            {
                topAnimator.SetBool(UpPressed, false);
                body = BodyPosture.Stand;
                CheckLeftRightDirection();
            }

            if (GetKeyUp(KeyCode.DownArrow))
            {
                topAnimator.SetBool(DownPressed, false);
                bottomAnimator.SetBool(DownPressed, false);
                body = BodyPosture.Stand;
                IsCrouched = false;
                AdaptColliderStanding();
                moveSpeed = 3.5f; // 일어나면 이동속도 초기화
                CheckLeftRightDirection();
            }

            if (anyKeyDown)
            {
                CancelInvoke();
                InvokeRepeating(nameof(SendPlayerInactiveEvent), 5, 2);
            }

            // blink.BlinkPlease();
        }

        private void FixedUpdate()
        {
            // 이동 벡터 계산
            Vector2 moveVelocity = _inputMovement * moveSpeed;

            // Rigidbody2D의 velocity 설정
            _playerRigidbody.velocity = new Vector2(moveVelocity.x, _playerRigidbody.velocity.y);
        }

        private void LateUpdate()
        {
            // 오브젝트의 위치 값을 직접 조절하여 z축 값을 고정
            transform.position = new Vector3(transform.position.x, transform.position.y, fixedZ);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventManager<GameEvents>.StopListening(GameEvents.GameReset, GameReset);
        }

        private void OnMove(InputValue inputValue)
        {
            // 낙하산이 해제되면 움직임 가능
            if (!_parachuteActive)
            {
                _inputMovement = inputValue.Get<Vector2>(); // 입력 값을 가져옴
                IsRunning = Mathf.Abs(_inputMovement.x) > 0f; // 움직이는지 확인

                // 웅크린 상태에서는 다른 속도를 적용
                float currentSpeed = IsCrouched ? moveSpeed * 0.5f : moveSpeed;

                // 이동 벡터 설정
                Vector2 moveVelocity = _inputMovement * currentSpeed;
                _playerRigidbody.velocity = new Vector2(moveVelocity.x, _playerRigidbody.velocity.y);

                // 방향 전환 처리
                if (_inputMovement.x > 0)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    LookingDirection = Vector2.right;
                }
                else if (_inputMovement.x < 0)
                {
                    transform.rotation = Quaternion.Euler(0, -180, 0);
                    LookingDirection = Vector2.left;
                }

                // 애니메이션 처리
                if (IsRunning)
                {
                    _animManager.StartRunningAnim(true);
                }
                else
                {
                    _animManager.StopRunningAnim();
                }
            }
        }

        private void OnJump(InputValue inputValue)
        {
            if (!_parachuteActive)
            {
                // 점프 허용 상태에서만 점프
                if (_jumpCount < maxJumps && inputValue.isPressed)
                {
                    body = BodyPosture.InTheAir;

                    // Rigidbody2D에 점프 힘 추가
                    _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, jumpForce);

                    // 점프 회수 증가
                    _jumpCount++;

                    InTheAir = true;

                    if (GetKeyDown(KeyCode.DownArrow) && InTheAir) LookingDirection = Vector2.down;

                    if (body == BodyPosture.Crouch) LookDown();

                    if (IsRunning) // 플레이어가 좌우로 움직이고 있다면
                    {
                        _timeUtils.FrameDelay(_animManager.StartHighVelJumpAnim);
                    }
                    else
                    {
                        _timeUtils.FrameDelay(_animManager.StartLowVelJumpAnim);
                    }

                    AdaptColliderStanding();
                }
            }
        }
        
        private void OnCrouchAndLookDown(InputValue inputValue)
        {
            if (inputValue.isPressed) // 키가 눌렸다면
            {
                if (!IsCrouched)
                {
                    _animManager.StartCrouchAnim();
                    body = BodyPosture.Crouch;
                    IsCrouched = true;
                    moveSpeed = 1.75f; // 웅크린 상태의 이동속도 (기본 이동속도의 50%)
                }
            }
            else // 키를 떼었을 때
            {
                _animManager.StartStandingUpAnim();
                body = BodyPosture.Stand;
                IsCrouched = false;
                moveSpeed = 3.5f; // 원래 속도로 복구
            }
        }

        private void OnAttack(InputValue inputValue)
        {
            if (inputValue.isPressed)
            {
                if (IsCrouched && !InTheAir) // 웅크린 상태에서 공격하면 이동 정지
                {
                    _playerRigidbody.velocity = Vector2.zero; // 즉시 이동 멈춤
                    moveSpeed = 0f;
                    IsAttacking = true;
                }

                _attackManager.PrimaryAttack(); // 실제 공격 실행
                DisableParachute();
            }
        }

        private void OnGrenade(InputValue inputValue)
        {
            if (!parachute.gameObject.activeSelf || !_parachuteActive)
            {
                if (inputValue.isPressed) _attackManager.SecondaryAttack();
            }
        }

        private void LookDown()
        {
            _animManager.StartLookDownAnim();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == (int)Layers.World ||
                collision.gameObject.layer == (int)Layers.Enemy ||
                collision.gameObject.CompareTag("Walkable"))
            {
                DisableParachute();
                NotifyObservers(SlugEvents.HitGround);
                body = BodyPosture.Stand;
                InTheAir = false;
                _waterHitHandled = false;
                
                if (LookingDirection == Vector2.down)
                {
                    body = BodyPosture.Crouch;

                    CheckLeftRightDirection();
                }
            }

            // 바닥에 닿으면 점프 회수 초기화
            if (collision.gameObject.layer == (int)Layers.World ||
                collision.gameObject.layer == (int)Layers.Walkable) _jumpCount = 0;

            _playerRigidbody.gravityScale = 2f;

            if (collision.gameObject.CompareTag("Water Dead") && !_waterHitHandled)
            {
                gameObject.GetComponent<HealthManager>().OnHitByProjectile(projectile);
                _waterHitHandled = true;
            }
        }
        
        private void DisableParachute()
        {
            if (!_parachuteActive) return;
            _parachuteActive = false;
            parachute.GroundedParachute();
            _playerRigidbody.gravityScale = 2.0f;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == (int)Layers.World)
            {
                InTheAir = true;
            }
        }

        // 플레이어가 비활성 상태임을 나타내는 이벤트를 발생
        private void SendPlayerInactiveEvent()
        {
            EventManager<PlayerEvents>.TriggerEvent(PlayerEvents.PlayerInactive);
        }

        private void NotifyObservers(SlugEvents ev)
        {
            if (_observers == null)
            {
                _observers = GetComponents<IObserver>();
            }

            foreach (IObserver obs in _observers)
            {
                obs.Observe(ev);
            }
        }

        private void AdaptColliderStanding()
        {
            _collider.offset = new Vector2(0, 0.09f);
            _collider.size = new Vector2(0.16f, 0.357f);
        }

        private void CheckLookingDirection()
        {
            if (Keyboard.current.upArrowKey.isPressed && Keyboard.current.leftArrowKey.isPressed)
                LookingDirection = Vector2.up;
            if (Keyboard.current.upArrowKey.isPressed && Keyboard.current.rightArrowKey.isPressed)
                LookingDirection = Vector2.up;

            if (InTheAir && Keyboard.current.downArrowKey.isPressed) LookingDirection = Vector2.down;

            if (body == BodyPosture.Crouch)
            {
                if (Keyboard.current.downArrowKey.isPressed && Keyboard.current.aKey.isPressed)
                {
                    CheckLeftRightDirection();
                }
            }
        }

        private void CheckLeftRightDirection()
        {
            LookingDirection = _rotationY == 0 ? Vector2.right : Vector2.left;
        }

        private void GameReset()
        {
            this.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}