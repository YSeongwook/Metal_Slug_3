using EventLibrary;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeathManager : MonoBehaviour, IDamaged
{
    private PlayerInput playerInput;
    private AnimationManager animManager;
    private HealthManager healthManager;
    private Blink blink;
    private TimeUtils timeUtils;
    private SpriteRenderer[] spriteRenderers;
    private FlashUsingMaterial flashBright;
    private Rigidbody2D rb;

    [Header("Managers")]
    public AttackManager attackManager;
    public AudioManager audioManager;

    [Space(10)]
    public GameObject playerIndicator;
    public int ignoreDamagesDuration = 3;

    private bool waitingForKeyInput = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        animManager = GetComponent<AnimationManager>();
        blink = GetComponent<Blink>();
        // physic = GetComponent<SlugPhysics>();
        healthManager = GetComponent<HealthManager>();
        timeUtils = GetComponent<TimeUtils>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        flashBright = GetComponent<FlashUsingMaterial>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        flashBright.FlashForDuration(ignoreDamagesDuration);
    }

    public void OnDamageReceived(ProjectileProperties projectileProp, int lifeCount)
    {
        playerInput.enabled = false;
        animManager.PlayDeathAnimation(projectileProp, DeathAnimCB);
        animManager.StopRunningAnim();
        gameObject.layer = 2;

        if (projectileProp.type == ProjectileType.Grenade)
        {
            /*
            physic.SetVelocityY(3);
            physic.SetVelocityX(-transform.right.x / 3);
            */
        }
        else if (projectileProp.type == ProjectileType.Knife)
        {
            EventManager.TriggerEvent(GlobalEvents.PlayerStabbed);
        }

        if (projectileProp.type == ProjectileType.Water) Invoke("PlayAudioDeathInWater", 0.6f);
        else audioManager.PlaySound(2);

        if (lifeCount >= 0) 
        {
            Invoke("SpawnPlayer", 3f);
        }
        else
        {
            if (lifeCount < 0) GameManager.Instance.SetGameOver();
            // Continue 상태에서 키 입력 대기
            StartCoroutine(WaitForKeyInputDuringContinue());
        }
    }

    public void DeathAnimCB()
    {
        blink.BlinkPlease(NotifyDeath);
    }

    private void NotifyDeath()
    {
        setPlayerVisible(false);
        EventManager.TriggerEvent(GlobalEvents.PlayerDead);
    }

    public void SpawnPlayer()
    {
        setPlayerVisible(true);
        playerInput.enabled = true;
        gameObject.layer = 8;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        animManager.ResetAnimators();
        attackManager.SetDefaultAttack();
        attackManager.RestoreGrenade();
        animManager.PlaySpawnAnim();
        healthManager.IgnoreDamages = true;
        healthManager.CurrentHP = healthManager.maxHP;
        flashBright.FlashForDuration(ignoreDamagesDuration);
        timeUtils.TimeDelay(ignoreDamagesDuration - 0.5f, () =>
        {
            healthManager.IgnoreDamages = false;
        });
        rb.gravityScale = 2f;

        // playerIndicator 5초간 활성화
        playerIndicator.SetActive(true);
    }

    public void ContinueGame()
    {
        Debug.Log("continue후 리스폰");

        healthManager.lifeCount = 2;
        SoundManager.Instance.PlayInsertCoin();
        GameManager.Instance.SetGameOverRespawn();  // GameManager의 isGameOver를 false로 변경
        HUDManager.Instance.AddCredit();
        HUDManager.Instance.ResetTime();
        HUDManager.Instance.SetLifeCount(healthManager.lifeCount);

        EventManager.TriggerEvent(GlobalEvents.Restart);
    }

    private void setPlayerVisible(bool visible)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].enabled = visible;
        }
    }

    // 엔터키 입력을 기다리는 코루틴
    IEnumerator WaitForKeyInputDuringContinue()
    {
        waitingForKeyInput = true;
        while (waitingForKeyInput)
        {
            // 엔터키 입력 여부 확인
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                // 입력이 감지되면 코루틴 종료
                waitingForKeyInput = false;
            }
            yield return null;
        }

        // 엔터키 입력 후 계속 실행할 코드
        ContinueGame();
        SpawnPlayer();
    }

    void PlayAudioDeathInWater()
    {
        audioManager.PlaySound(2);
    }
}