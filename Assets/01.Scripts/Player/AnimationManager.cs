using _01.Scripts.Player;
using _01.Scripts.Utils;
using UnityEngine;
using Utils;

public class AnimationManager : MonoBehaviour, IObserver
{
    // 상단, 하단, 혈흔 애니메이터 컴포넌트
    public Animator topAnimator;
    public Animator bottomAnimator;
    public Animator blood;
    
    // 상단, 하단 스프라이트 렌더러
    private SpriteRenderer _topSpriteRenderer;
    private SpriteRenderer _bottomSpriteRenderer;
    
    // 사망 및 기본 애니메이션 컨트롤러
    public RuntimeAnimatorController deathAnimController;
    public RuntimeAnimatorController defaultAnimController;
    
    // 사망 시 물 관련 애니메이션 오브젝트들
    public GameObject deathWaterWave;
    public GameObject deathWaterMarco;

    // 수류탄 애니메이션 콜백 델리게이트
    public RetVoidTakeVoid grenadeCB;
    // 사망 애니메이션 종료 후 호출할 콜백 델리게이트
    private RetVoidTakeVoid _endOfDeathCb;
    
    // 폭발성 사망 애니메이션 실행 여부 플래그
    private bool _inExplosiveDeathAnim;
    
    // 애니메이터 파라미터 캐싱
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int JumpLowSpeed = Animator.StringToHash("jump_low_speed");
    private static readonly int JumpTrigger = Animator.StringToHash("jump_trigger");
    private static readonly int JumpHighSpeed = Animator.StringToHash("jump_high_speed");
    private static readonly int UpPressed = Animator.StringToHash("up_pressed");
    private static readonly int DownPressed = Animator.StringToHash("down_pressed");
    private static readonly int LookUpTrigger = Animator.StringToHash("look_up_trigger");
    private static readonly int Sit = Animator.StringToHash("sit");
    private static readonly int TouchGroundDeath = Animator.StringToHash("touch_ground_death");
    private static readonly int HitGround = Animator.StringToHash("hit_ground");
    private static readonly int Grenade = Animator.StringToHash("grenade");
    private static readonly int MissionComplete = Animator.StringToHash("mission_complete");

    public void Start()
    {
        _topSpriteRenderer = topAnimator.gameObject.GetComponent<SpriteRenderer>();
        _bottomSpriteRenderer = bottomAnimator.gameObject.GetComponent<SpriteRenderer>();
    }

    public void StartRunningAnim(bool isRunning)
    {
        if (topAnimator.runtimeAnimatorController.name == "MarcoDeath")
            return;
        
        // 웅크리고 있지 않다면
        if (PlayerController.Instance.body != BodyPosture.Crouch)
        {
            topAnimator.SetBool(IsRunning, isRunning);
            bottomAnimator.SetBool(IsRunning, isRunning);
        }
        // 웅크리고 있다면
        else
        {
            topAnimator.SetBool(IsRunning, isRunning);
            bottomAnimator.SetBool(IsRunning, false);
        }
    }

    public void StopRunningAnim()
    {
        if (topAnimator.runtimeAnimatorController.name == "MarcoDeath")
            return;
        
        topAnimator.SetBool(IsRunning, false);
        bottomAnimator.SetBool(IsRunning, false);
    }

    public void StartLowVelJumpAnim()
    {
        bottomAnimator.SetTrigger(JumpLowSpeed);
        topAnimator.SetTrigger(JumpTrigger);
        topAnimator.SetBool(JumpLowSpeed, true);
    }

    public void StartHighVelJumpAnim()
    {
        topAnimator.SetTrigger(JumpTrigger);
        bottomAnimator.SetTrigger(JumpHighSpeed);
        topAnimator.SetBool(JumpHighSpeed, true);
    }

    public void StartLookUpAnim()
    {
        if (!topAnimator.GetBool(UpPressed))
        {
            if (!topAnimator.GetBool(JumpLowSpeed)
                    && !topAnimator.GetBool(JumpHighSpeed)
                    && !topAnimator.GetBool(DownPressed))
            {
                topAnimator.SetTrigger(LookUpTrigger);
            }
        }
        topAnimator.SetBool(UpPressed, true);
    }

    public void StartLookStraightAnim()
    {
        topAnimator.SetBool(DownPressed, false);
        bottomAnimator.SetBool(DownPressed, false);
        topAnimator.SetBool(UpPressed, false);
    }

    public void StartCrouchAnim()
    {
        topAnimator.SetTrigger(Sit);
        topAnimator.SetBool(DownPressed, true);
        bottomAnimator.SetBool(DownPressed, true);
        bottomAnimator.SetBool(IsRunning, false);
    }

    public void StartLookDownAnim()
    {
        topAnimator.SetBool(DownPressed, true);
    }

    public void StartStandingUpAnim()
    {
        StartLookStraightAnim();
    }

    public void Observe(SlugEvents ev)
    {
        if (!topAnimator.isInitialized)
            return;

        if (ev == SlugEvents.Fall && !_inExplosiveDeathAnim)
        {
            topAnimator.SetTrigger(JumpTrigger);
            topAnimator.SetBool(JumpLowSpeed, true);
            bottomAnimator.SetTrigger(JumpLowSpeed);
        }
        else if (ev == SlugEvents.HitGround)
        {
            if (_inExplosiveDeathAnim)
            {
                topAnimator.applyRootMotion = false;
                topAnimator.SetTrigger(TouchGroundDeath);
                return;
            }
            topAnimator.SetTrigger(HitGround);
            bottomAnimator.SetTrigger(HitGround);
            topAnimator.SetBool(JumpLowSpeed, false);
            topAnimator.SetBool(JumpHighSpeed, false);
        }
    }

    public void StartGrenadeAnim(RetVoidTakeVoid cb)
    {
        topAnimator.SetTrigger(Grenade);
        grenadeCB = cb;
    }

    public void PlaySpawnAnim()
    {
        topAnimator.Play("marco-spawn");
    }

    public void PlayDeathAnimation(ProjectileProperties proj, RetVoidTakeVoid cb)
    {
        string trigger;

        topAnimator.runtimeAnimatorController = deathAnimController;

        if (proj.type == ProjectileType.Grenade)
        {
            trigger = "explo";
            _inExplosiveDeathAnim = true;
        }
        else if (proj.type == ProjectileType.Knife)
        {
            trigger = "slash";
            blood.Play("1");
        }
        else if(proj.type == ProjectileType.Water)
        {
            _topSpriteRenderer.enabled = false;
            _bottomSpriteRenderer.enabled = false;

            deathWaterWave.SetActive(true);
            Invoke(nameof(ActiveDeathWaterMarco), 0.4f);
            return;
        }
        else
        {
            trigger = "slash";
        }
        _endOfDeathCb = cb;
        topAnimator.SetTrigger(trigger);
    }
    
    public void EndOfDeathAnim()
    {
        if (_endOfDeathCb != null)
            _endOfDeathCb();
    }

    public void MissionCompleteAnim()
    {
        topAnimator.SetTrigger(MissionComplete);
    }

    public void ResetAnimators()
    {
        topAnimator.runtimeAnimatorController = defaultAnimController;
        topAnimator.Rebind();
        bottomAnimator.Rebind();
        _inExplosiveDeathAnim = false;
    }

    private void ActiveDeathWaterMarco()
    {
        deathWaterMarco.SetActive(true);
    }
}