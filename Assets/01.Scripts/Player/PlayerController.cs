using _01.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Input;

namespace _01.Scripts.Player
{
    public class PlayerController : Singleton<PlayerController>
    {
        // 플레이어 상태 업데이트 및 이벤트 전파를 위한 옵저버 배열
        private IObserver[] _observers;
        
        // 공격 및 애니메이션 처리를 위한 매니저 컴포넌트
        private AttackManager _attackManager;   // 공격 매니저
        private AnimationManager _animManager;  // 애니메이션 매니저
        
        // 상단과 하단 애니메이터 컴포넌트
        [SerializeField] private Animator topAnimator;      // 상단 애니메이터
        [SerializeField] private Animator bottomAnimator;   // 하단 애니메이터
        
        private Rigidbody2D _playerRigidbody;   // 사용할 리지드바디 컴포넌트
        private BoxCollider2D _collider;    // 사용할 콜라이더 커포넌트
        private TimeUtils _timeUtils;   // 시간 관련 유틸리티
        
        // 플레이어 이동 및 상태 관련 변수들
        public float moveSpeed = 3.5f;                  // 기본 이동 속도           
        [SerializeField] private float jumpForce = 70f; // 점프 힘
        [SerializeField] private int maxJumps = 1;      // 최대 점프 횟수
        private Vector2 _inputMovement = Vector2.zero;  // 이동 입력 벡터
        private int _jumpCount = 0;                     // 현재 점프 횟수
        public Vector2 LookingDirection { get; private set; }   // 플레이어가 바라보는 방향

        // 플레이어 자세 및 상태 플래그
        [Space(10)] public BodyPosture body;    // 현재 플레이어 자세
        public bool IsRunning { get; set; }     // 이동 중 여부
        public bool IsCrouched { get; set; }    // 웅크림 상태 여부
        public bool IsLookUp { get; set; }      // 위쪽을 보는 상태 여부
        public bool InTheAir { get; set; }      // 공중에 있는 상태 여부
        public bool IsAttacking { get; set; }   // 공격 중 여부
        
        // 물 관련 오브젝트와 낙하산 컴포넌트들
        public GameObject waterStep;
        public Parachute parachute;
        
        public ProjectileProperties projectile;
        public float fixedZ = 0f; // 고정할 z축 값
        
        private bool _parachuteActive = true;   // 낙하산 활성 여부
        private bool _waterHitHandled = false;  // 물 충돌 처리 플래그
        private float _rotationY = 0;   // 회전 각도(플레이어 좌우 방향)

        // 애니메이터 파라미터 캐싱
        private static readonly int UpPressed = Animator.StringToHash("up_pressed");
        private static readonly int DownPressed = Animator.StringToHash("down_pressed");

        // 싱글톤 인스턴스 초기화 및 이벤트 구독
        protected override void Awake()
        {
            base.Awake();
            EventManager<GameEvents>.StartListening(GameEvents.GameReset, GameReset);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            _rotationY = transform.rotation.eulerAngles.y;
            UpdateAnimationBasedOnState();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void LateUpdate()
        {
            FixZPosition();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager<GameEvents>.StopListening(GameEvents.GameReset, GameReset);
        }

        // 컴포넌트 및 상태 변수들 초기화
        private void Initialize()
        {
            _observers = GetComponentsInChildren<IObserver>();
            _attackManager = GetComponentInChildren<AttackManager>();
            _animManager = GetComponent<AnimationManager>();
            _playerRigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _timeUtils = GetComponent<TimeUtils>();

            IsCrouched = false;
            IsRunning = false;
            IsLookUp = false;
            InTheAir = false;
            LookingDirection = Vector2.right;

            _playerRigidbody.gravityScale = 0.2f;
        }

        // 이동 입력이 들어올 때 처리하는 메서드
        private void OnMove(InputValue inputValue)
        {
            if (_parachuteActive) return; // 낙하산이 활성 중이면 이동 불가

            _inputMovement = inputValue.Get<Vector2>(); // 입력 값을 가져옴
            IsRunning = Mathf.Abs(_inputMovement.x) > 0f; // 움직이는지 확인

            // 웅크린 상태이면 이동 속도 감소
            float currentSpeed = IsCrouched ? moveSpeed * 0.5f : moveSpeed;
            Vector2 moveVelocity = _inputMovement * currentSpeed;
            _playerRigidbody.velocity = new Vector2(moveVelocity.x, _playerRigidbody.velocity.y);

            // 이동 방향에 따라 플레이어 회전 및 바라보는 방향 업데이트
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

            // 달리기 애니메이션 처리
            if (IsRunning)
            {
                _animManager.StartRunningAnim(true);
            }
            else
            {
                _animManager.StopRunningAnim();
            }
        }

        // 점프 입력이 들어올 때 처리하는 메서드
        private void OnJump(InputValue inputValue)
        {
            if (_parachuteActive) return;
            if (_jumpCount >= maxJumps || !inputValue.isPressed) return;

            body = BodyPosture.InTheAir;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, jumpForce);
            _jumpCount++;
            InTheAir = true;

            // 점프 시, 만약 아래 방향 키가 눌려 있다면 바라보는 방향을 아래로 업데이트
            if (Keyboard.current.downArrowKey.isPressed)
            {
                UpdateLookingDirection(Vector2.down);
            }
            
            // 웅크림 상태에서 점프 시 아래 보기 애니메이션 실행
            if (body == BodyPosture.Crouch)
                LookDown();

            // 점프 애니메이션 처리: 이동 중이면 고속 점프, 아니면 저속 점프
            if (IsRunning)
                _timeUtils.FrameDelay(_animManager.StartHighVelJumpAnim);
            else
                _timeUtils.FrameDelay(_animManager.StartLowVelJumpAnim);

            AdaptColliderStanding();
        }

        // 웅크림 및 아래 보기 입력을 처리하는 메서드
        private void OnCrouchAndLookDown(InputValue inputValue)
        {
            if (inputValue.isPressed) // 키가 눌렸다면
            {
                if (IsCrouched) return;

                _animManager.StartCrouchAnim();
                body = BodyPosture.Crouch;
                IsCrouched = true;
                moveSpeed = 1.75f; // 웅크린 상태의 이동속도 (기본 이동속도의 50%)
            }
            else // 키를 떼었을 때
            {
                _animManager.StartStandingUpAnim();
                body = BodyPosture.Stand;
                IsCrouched = false;
                moveSpeed = 3.5f; // 원래 속도로 복구
            }
        }

        // 공격 입력이 들어올 때 처리하는 메서드
        private void OnAttack(InputValue inputValue)
        {
            if (!inputValue.isPressed) return;

            if (IsCrouched && !InTheAir) // 웅크린 상태에서 공격하면 이동 정지
            {
                _playerRigidbody.velocity = Vector2.zero; // 즉시 이동 멈춤
                moveSpeed = 0f;
                IsAttacking = true;
            }

            _attackManager.PrimaryAttack(); // 실제 공격 실행
            DisableParachute();
        }

        // 수류탄 공격 입력을 처리하는 메서드
        private void OnGrenade(InputValue inputValue)
        {
            if (_parachuteActive) return;
            if (inputValue.isPressed)
                _attackManager.SecondaryAttack();
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

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == (int)Layers.World)
            {
                InTheAir = true;
            }
        }
        
        private void UpdateAnimationBasedOnState()
        {
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
        }

        private void ApplyMovement()
        {
            // 이동 벡터 계산
            Vector2 moveVelocity = _inputMovement * moveSpeed;

            // Rigidbody2D의 velocity 설정
            _playerRigidbody.velocity = new Vector2(moveVelocity.x, _playerRigidbody.velocity.y);
        }

        // 오브젝트의 위치 값을 직접 조절하여 z축 값을 고정
        private void FixZPosition()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, fixedZ);
        }

        // 플레이어가 비활성 상태임을 나타내는 이벤트를 발생
        private void SendPlayerInactiveEvent()
        {
            EventManager<PlayerEvents>.TriggerEvent(PlayerEvents.PlayerInactive);
        }

        // 옵저버들에게 이벤트를 전달하는 메서드
        private void NotifyObservers(SlugEvents ev)
        {
            if (_observers == null)
                _observers = GetComponents<IObserver>();

            foreach (var obs in _observers)
            {
                obs.Observe(ev);
            }
        }

        // 서 있는 상태에 맞춰 콜라이더의 크기와 오프셋을 조정하는 메서드
        private void AdaptColliderStanding()
        {
            _collider.offset = new Vector2(0, 0.09f);
            _collider.size = new Vector2(0.16f, 0.357f);
        }

        // 현재 키 입력 상태를 기반으로 바라보는 방향을 결정하는 메서드
        private void CheckLookingDirection()
        {
            if (Keyboard.current.upArrowKey.isPressed && Keyboard.current.leftArrowKey.isPressed)
            {
                UpdateLookingDirection(Vector2.up);
            }
            if (Keyboard.current.upArrowKey.isPressed && Keyboard.current.rightArrowKey.isPressed)
            {
                UpdateLookingDirection(Vector2.up);
            }
            if (InTheAir && Keyboard.current.downArrowKey.isPressed)
            {
                UpdateLookingDirection(Vector2.down);
            }
            if (body == BodyPosture.Crouch && Keyboard.current.downArrowKey.isPressed && Keyboard.current.aKey.isPressed)
            {
                CheckLeftRightDirection();
            }
        }

        // 현재 회전 값을 기준으로 좌우 방향을 결정하는 메서드
        private void CheckLeftRightDirection()
        {
            UpdateLookingDirection(_rotationY == 0f ? Vector2.right : Vector2.left);
        }

        // 바라보는 방향을 업데이트하고 디버그 로그를 남기는 메서드
        private void UpdateLookingDirection(Vector2 newDirection)
        {
            LookingDirection = newDirection;
        }
        
        private void DisableParachute()
        {
            if (!_parachuteActive) return;
            _parachuteActive = false;
            parachute.GroundedParachute();
            _playerRigidbody.gravityScale = 2.0f;
        }

        private void GameReset()
        {
            gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}