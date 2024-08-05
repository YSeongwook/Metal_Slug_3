using EnumTypes;
using EventLibrary;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Input;

public class PlayerController : Singleton<PlayerController>
{
    private IObserver[] observers;

    private AttackManager attackManager; // 공격 매니저
    private AnimationManager animManager; // 애니메이션 매니저
    private Blink blink;

    [SerializeField] Animator topAnimator; // 사용할 top 애니메이터 컴포넌트
    [SerializeField] Animator bottomAnimator; // 사용할 bottom 애니메이터 컴포넌트

    [Space(10)] public GameObject waterStep;
    public Parachute parachute;

    [Space(10)] public BodyPosture body;
    private TimeUtils timeUtils;

    [SerializeField] float moveSpeed = 3.5f; // 이동 속도           
    [SerializeField] float jumpForce = 70f; // 점프 힘
    [SerializeField] int maxJumps = 1; // 최대 점프 횟수

    private Vector2 inputMovement = Vector2.zero;
    private bool isGrounded = false; // 바닥에 닿았는지 나타냄
    private int jumpCount = 0; // 현재 점프 회수를 추적

    public float fixedZ = 0f; // 고정할 z축 값
    float rotationY = 0; // transform.rotation.eulerAngles.y;

    public bool IsRunning { get; set; } // 움직이고 있는지 확인
    public bool IsCrouched { get; set; } // 웅크리고 있는지 확인
    public bool IsLookUp { get; set; } // 위를 보고 있는지 확인
    public bool InTheAir { get; set; } // 공중에 떠 있는지 확인
    public Vector2 LookingDirection { get; set; }

    private Rigidbody2D playerRigidbody; // 사용할 리지드바디 컴포넌트
    private BoxCollider2D collider; // 사용할 콜라이더 커포넌트

    private bool parachuteActive = true;

    public ProjectileProperties projectile;

    private new void Awake()
    {
        base.Awake();

        EventManager.StartListening(GlobalEvents.GameReset, GameReset);
    }

    private void Start()
    {
        // 게임 오브젝트로부터 사용할 컴포넌트들을 가져와 변수에 할당
        observers = GetComponentsInChildren<IObserver>();
        attackManager = GetComponentInChildren<AttackManager>();
        animManager = GetComponent<AnimationManager>();
        blink = GetComponent<Blink>();

        playerRigidbody = GetComponent<Rigidbody2D>();
        timeUtils = GetComponent<TimeUtils>();
        collider = GetComponent<BoxCollider2D>();

        IsCrouched = false;
        IsRunning = false;
        IsLookUp = false;
        InTheAir = false;

        playerRigidbody.gravityScale = 0.2f;
    }

    private void Update()
    {
        rotationY = transform.rotation.eulerAngles.y;

        // 캐릭터의 움직임 여부를 확인하여 isRun 파라미터를 설정
        animManager.StartRunningAnim(IsRunning);

        if (GetKeyDown(KeyCode.UpArrow) && !IsCrouched) LookingDirection = Vector2.up;

        CheckLookingDirection();

        if (GetKeyUp(KeyCode.UpArrow))
        {
            topAnimator.SetBool("up_pressed", false);
            body = BodyPosture.Stand;

            CheckLeftRightDirection();
        }

        if (GetKeyUp(KeyCode.DownArrow))
        {
            topAnimator.SetBool("down_pressed", false);
            bottomAnimator.SetBool("down_pressed", false);
            body = BodyPosture.Stand;
            IsCrouched = false;
            AdaptColliderStanding();
            moveSpeed = 3.5f; // 일어나면 이동속도 초기화

            CheckLeftRightDirection();
        }

        if (anyKeyDown)
        {
            CancelInvoke();
            InvokeRepeating("SendPlayerInactiveEvent", 5, 2);
        }

        // blink.BlinkPlease();
    }

    private void FixedUpdate()
    {
        // 이동 벡터 계산
        Vector2 moveVelocity = inputMovement * moveSpeed;

        // Rigidbody2D의 velocity 설정
        playerRigidbody.velocity = new Vector2(moveVelocity.x, playerRigidbody.velocity.y);
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
        if (!parachute.gameObject.activeSelf || !parachuteActive)
        {
            // 움직임 입력을 받음
            inputMovement = inputValue.Get<Vector2>();

            // 입력된 방향을 기준으로 캐릭터가 움직이는지 여부를 판단
            IsRunning = Mathf.Abs(inputMovement.x) > 0f;

            // transform.rotation을 변경하여 좌우 반전
            if (inputMovement.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                // 왼쪽으로 움직이면 transform.positon.z값이 변경됨
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                LookingDirection = Vector2.right;
            }
            else if (inputMovement.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, -180, 0);
                // 좌우 반전한 후 이동하면 반대로 이동되기에 이동값도 반전
                // inputMovement *= -1;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                LookingDirection = Vector2.left;
            }

            // 움직임이 감지되지 않으면 Animator의 isRunning을 false로 설정
            if (!IsRunning) animManager.StopRunningAnim();

            if (GetKeyUp(KeyCode.UpArrow))
            {
                if (inputMovement.x > 0) LookingDirection = Vector2.right;
                else LookingDirection = Vector2.left;
            }

            if (GetKeyUp(KeyCode.DownArrow))
            {
                if (inputMovement.x > 0) LookingDirection = Vector2.right;
                else LookingDirection = Vector2.left;
            }
        }
    }

    private void OnLookUp(InputValue inputValue)
    {
        if (inputValue.isPressed) LookUp();
    }

    private void OnJump(InputValue inputValue)
    {
        if (!parachute.gameObject.activeSelf || !parachuteActive)
        {
            // 점프 허용 상태에서만 점프
            if (jumpCount < maxJumps && inputValue.isPressed)
            {
                body = BodyPosture.InTheAir;

                // Rigidbody2D에 점프 힘 추가
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, jumpForce);

                // 점프 회수 증가
                jumpCount++;

                InTheAir = true;

                if (GetKeyDown(KeyCode.DownArrow) && InTheAir) LookingDirection = Vector2.down;

                if (body == BodyPosture.Crouch) LookDown();

                if (IsRunning) // 플레이어가 좌우로 움직이고 있다면
                {
                    timeUtils.FrameDelay(animManager.StartHighVelJumpAnim);
                }
                else
                {
                    timeUtils.FrameDelay(animManager.StartLowVelJumpAnim);
                }

                AdaptColliderStanding();
            }
        }
    }

    private void OnCrouchAndLookDown(InputValue inputValue)
    {
        // 키를 누르고 있는 동안에만 IsCrouched 상태를 변경
        if (inputValue.isPressed) DownMovement();
    }

    private void OnAttack(InputValue inputValue)
    {
        if (inputValue.isPressed) attackManager.PrimaryAttack();

        playerRigidbody.gravityScale = 2f;

        parachuteActive = false;
    }

    private void OnGrenade(InputValue inputValue)
    {
        if (!parachute.gameObject.activeSelf || !parachuteActive)
        {
            if (inputValue.isPressed) attackManager.SecondaryAttack();
        }
    }

    public void DefaultBodyPosition()
    {
        body = BodyPosture.Stand;
        animManager.StartLookStraightAnim();
        AdaptColliderStanding();
    }

    public bool JumpLowVel()
    {
        if (InTheAir) return false;

        InTheAir = true;

        return true;
    }

    public bool JumpHighVel()
    {
        if (InTheAir) return false;

        InTheAir = true;

        return true;
    }

    public void LookUp()
    {
        animManager.StartLookUpAnim();
    }

    private void LookDown()
    {
        animManager.StartLookDownAnim();
    }

    void Crouch()
    {
        body = BodyPosture.Crouch;
        IsCrouched = true;
        animManager.StartCrouchAnim();
        AdaptColliderCrouching();
    }

    public void DownMovement()
    {
        if (!isGrounded)
        {
            LookDown();
        }
        else
        {
            Crouch();
            moveSpeed = 2.5f; // 웅크리면 이동속도 감소
        }
    }

    private bool waterHitHandled = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == (int)Layers.World || collision.gameObject.layer == (int)Layers.Enemy ||
            collision.gameObject.CompareTag("Walkable"))
        {
            // 낙하산이 활성화 되어 있다면 낙하산 해제
            if (parachute.gameObject.activeSelf)
            {
                parachuteActive = false;
                parachute.GroundedParachute();
            }

            NotifyObservers(SlugEvents.HitGround);
            body = BodyPosture.Stand;

            isGrounded = true;
            InTheAir = false;

            if (LookingDirection == Vector2.down)
            {
                body = BodyPosture.Crouch;

                CheckLeftRightDirection();
            }

            waterHitHandled = false;
        }

        // 바닥에 닿으면 점프 회수 초기화
        if (collision.gameObject.layer == (int)Layers.World ||
            collision.gameObject.layer == (int)Layers.Walkable) jumpCount = 0;

        playerRigidbody.gravityScale = 2f;

        if (collision.gameObject.CompareTag("Water Dead") && !waterHitHandled)
        {
            gameObject.GetComponent<HealthManager>().OnHitByProjectile(projectile);
            waterHitHandled = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == (int)Layers.World)
        {
            isGrounded = false;
            InTheAir = true;
        }
    }

    // 플레이어가 비활성 상태임을 나타내는 이벤트를 발생
    void SendPlayerInactiveEvent()
    {
        EventManager.TriggerEvent(GlobalEvents.PlayerInactive);
    }

    // 아래 방향으로 ray를 쏘고, 캐릭터의 발 아래에 있는 레이어를 반환하는 메서드
    public int RaycastBelow(float maxDistance)
    {
        // 캐릭터의 발 아래 위치 설정
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;

        // 콜라이더의 아래쪽 경계에 해당하는 지점으로 origin을 이동
        origin.y = GetComponent<Collider2D>().bounds.min.y;

        // Raycast를 통해 아래 방향으로 검출
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance);

        // Raycast로 검출된 레이어 반환
        if (hit.collider != null)
        {
            return hit.collider.gameObject.layer;
        }
        else
        {
            // 아무 레이어도 검출되지 않으면 LayerMask.None 반환
            return 0;
        }
    }

    void NotifyObservers(SlugEvents ev)
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

    private void AdaptColliderCrouching()
    {
        float new_size = collider.size.y / 2;
        float diff = collider.size.y - new_size;
        collider.offset = new Vector2(collider.offset.x - 0.01f, collider.offset.y - diff / 2);
        collider.size = new Vector2(collider.size.x + 0.05f, new_size);
    }

    private void AdaptColliderStanding()
    {
        collider.offset = new Vector2(0, 0.09f);
        collider.size = new Vector2(0.16f, 0.357f);
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
        LookingDirection = rotationY == 0 ? Vector2.right : Vector2.left;
    }

    private void GameReset()
    {
        this.gameObject.transform.parent.gameObject.SetActive(false);
    }
}