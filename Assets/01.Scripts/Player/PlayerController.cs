using EnumTypes;
using EventLibrary;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Input;

public class PlayerController : Singleton<PlayerController>
{
    private IObserver[] observers;

    private AttackManager _attackManager; // 공격 매니저
    private AnimationManager _animManager; // 애니메이션 매니저

    [SerializeField] Animator topAnimator; // 사용할 top 애니메이터 컴포넌트
    [SerializeField] Animator bottomAnimator; // 사용할 bottom 애니메이터 컴포넌트

    [Space(10)] public GameObject waterStep;
    public Parachute parachute;

    [Space(10)] public BodyPosture body;
    private TimeUtils _timeUtils;

    [SerializeField] float moveSpeed = 3.5f; // 이동 속도           
    [SerializeField] float jumpForce = 70f; // 점프 힘
    [SerializeField] int maxJumps = 1; // 최대 점프 횟수

    private Vector2 _inputMovement = Vector2.zero;
    private bool _isGrounded = false; // 바닥에 닿았는지 나타냄
    private int _jumpCount = 0; // 현재 점프 회수를 추적

    public float fixedZ = 0f; // 고정할 z축 값
    private float _rotationY = 0; // transform.rotation.eulerAngles.y;

    public bool IsRunning { get; set; } // 움직이고 있는지 확인
    public bool IsCrouched { get; set; } // 웅크리고 있는지 확인
    public bool IsLookUp { get; set; } // 위를 보고 있는지 확인
    public bool InTheAir { get; set; } // 공중에 떠 있는지 확인
    public Vector2 LookingDirection { get; set; }

    private Rigidbody2D _playerRigidbody; // 사용할 리지드바디 컴포넌트
    private BoxCollider2D _collider; // 사용할 콜라이더 커포넌트

    private bool _parachuteActive = true;
    private bool _waterHitHandled = false;

    public ProjectileProperties projectile;

    // 애니메이터 파라미터 캐싱
    private static readonly int UpPressed = Animator.StringToHash("up_pressed");
    private static readonly int DownPressed = Animator.StringToHash("down_pressed");
    
    private new void Awake()
    {
        base.Awake();

        EventManager.StartListening(GlobalEvents.GameReset, GameReset);
    }

    private void Start()
    {
        // 게임 오브젝트로부터 사용할 컴포넌트들을 가져와 변수에 할당
        observers = GetComponentsInChildren<IObserver>();
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

        if (GetKeyDown(KeyCode.UpArrow) && !IsCrouched) LookingDirection = Vector2.up;

        CheckLookingDirection();

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

    private void OnDestroy()
    {
        EventManager.StopListening(GlobalEvents.GameReset, GameReset);
    }

    private void OnMove(InputValue inputValue)
    {
        // 낙하산이 해제되면 움직임 가능
        if (!parachute.gameObject.activeSelf || !_parachuteActive)
        {
            _inputMovement = inputValue.Get<Vector2>(); // 움직임 입력을 받음

            IsRunning = Mathf.Abs(_inputMovement.x) > 0f; // 입력된 방향을 기준으로 캐릭터가 움직이는지 여부를 판단

            // transform.rotation을 변경하여 좌우 반전
            if (_inputMovement.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                // 왼쪽으로 움직이면 transform.positon.z값이 변경됨
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                LookingDirection = Vector2.right;
            }
            else if (_inputMovement.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, -180, 0);
                // 좌우 반전한 후 이동하면 반대로 이동되기에 이동값도 반전
                // inputMovement *= -1;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                LookingDirection = Vector2.left;
            }

            // 움직임이 감지되지 않으면 Animator의 isRunning을 false로 설정
            if (!IsRunning) _animManager.StopRunningAnim();

            if (GetKeyUp(KeyCode.UpArrow))
            {
                if (_inputMovement.x > 0) LookingDirection = Vector2.right;
                else LookingDirection = Vector2.left;
            }

            if (GetKeyUp(KeyCode.DownArrow))
            {
                if (_inputMovement.x > 0) LookingDirection = Vector2.right;
                else LookingDirection = Vector2.left;
            }
        }
    }

    private void OnJump(InputValue inputValue)
    {
        if (!parachute.gameObject.activeSelf || !_parachuteActive)
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

    private void OnAttack(InputValue inputValue)
    {
        if (inputValue.isPressed) _attackManager.PrimaryAttack();

        _playerRigidbody.gravityScale = 2f;

        _parachuteActive = false;
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
        if (collision.gameObject.layer == (int)Layers.World || collision.gameObject.layer == (int)Layers.Enemy ||
            collision.gameObject.CompareTag("Walkable"))
        {
            // 낙하산이 활성화 되어 있다면 낙하산 해제
            if (parachute.gameObject.activeSelf)
            {
                _parachuteActive = false;
                parachute.GroundedParachute();
            }

            NotifyObservers(SlugEvents.HitGround);
            body = BodyPosture.Stand;

            _isGrounded = true;
            InTheAir = false;

            if (LookingDirection == Vector2.down)
            {
                body = BodyPosture.Crouch;

                CheckLeftRightDirection();
            }

            _waterHitHandled = false;
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
            _isGrounded = false;
            InTheAir = true;
        }
    }

    // 플레이어가 비활성 상태임을 나타내는 이벤트를 발생
    private void SendPlayerInactiveEvent()
    {
        EventManager.TriggerEvent(GlobalEvents.PlayerInactive);
    }

    private void NotifyObservers(SlugEvents ev)
    {
        if (observers == null)
        {
            observers = GetComponents<IObserver>();
        }

        foreach (IObserver obs in observers)
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