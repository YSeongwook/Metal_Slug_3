using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using EventLibrary;
using EnumTypes;

public class HUDManager : Singleton<HUDManager>
{
    public GameObject lifeScoreBar;
    public GameObject munitionsGroup;

    [Space(10)]
    public Text scoreGUI;
    public Text bulletCountGUI;
    public Text grenadeCountGUI;
    public Text lifeCountGUI;
    public GameObject goRightReminder;
    public GameObject topContinue;
    public GameObject centerContinue;
    public MissionStartLettersAnimation missionLetters;
    public GameObject powCount;
    public GameObject creditCount;

    private Gradient bulletCountGradient;
    private TimeUtils timeUtils;

    private readonly int armsFontSize = 47;
    private readonly int armsFontSizeWhenGun = 70;

    private GameObject[] powObjects; // Pow 오브젝트 배열
    private int currentPowIndex = 0; // 현재 활성화된 Pow 인덱스
    private int credit = 0;

    [Space(10)]
    [Header("Time Display")]
    public Sprite[] timeNumberSprites;  // 0부터 9까지의 시간 이미지 배열
    public Image tensTimeImage;         // 십의 자리 숫자를 표시할 시간 이미지
    public Image onesTimeImage;         // 일의 자리 숫자를 표시할 시간 이미지

    [Header("Credit Display")]
    public Sprite[] creditNumberSprites;    // 0부터 9까지의 크레딧 이미지 배열
    public Image tensCreditImage;           // 십의 자리 숫자를 표시할 시간 이미지
    public Image onesCreditImage;           // 일의 자리 숫자를 표시할 시간 이미지

    float currentTime = 61f;

    void Awake()
    {
        EventManager.StartListening(GlobalEvents.GunUsed, SetBulletCount);
        EventManager.StartListening(GlobalEvents.PlayerDead, SetBulletCountToInfinity);
        EventManager.StartListening(GlobalEvents.GrenadeUsed, SetGrenadeCount);

        EventManager.StartListening(GlobalEvents.MissionStart, OnMissionStart);
        EventManager.StartListening(GlobalEvents.MissionSuccess, OnMissionSuccess);
        EventManager.StartListening(GlobalEvents.PointsEarned, OnPlayerPointsChanged);
        EventManager.StartListening(GlobalEvents.PlayerDead, OnPlayerDeath);
        EventManager.StartListening(GlobalEvents.GameOver, CheckGameOver);
        EventManager.StartListening(GlobalEvents.Restart, Restart);
        EventManager.StartListening(GlobalEvents.Home, () => SetVisible(false));

        bulletCountGradient = bulletCountGUI.GetComponent<Gradient>();
        timeUtils = GetComponent<TimeUtils>();
        // pauseButton.onClick.AddListener(OnPausePressed);

        // Pow 오브젝트들을 배열에 저장
        powObjects = new GameObject[powCount.transform.childCount];
        for (int i = 0; i < powObjects.Length; i++)
        {
            powObjects[i] = powCount.transform.GetChild(i).gameObject;
        }

        ResetCredit();
    }

    void OnDisable()
    {
        // 게임 오버 상태 변경 이벤트 리스너 제거
        EventManager.StopListening(GlobalEvents.GameOver, CheckGameOver);
    }

    private void Update()
    {
        if(!GameManager.Instance.IsGameOver())
        {
            UpdateTime();
            UpdateTimeDisplay();
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetLifeCount(int lifeCount)
    {
        lifeCountGUI.text = lifeCount.ToString();
    }

    public void SetScore(int score)
    {
        scoreGUI.text = score.ToString();
    }

    public void SetGrenadeCount(float grenadeCount)
    {
        grenadeCountGUI.text = grenadeCount.ToString();
    }

    public void SetBulletCount(float bulletCount)
    {
        if (bulletCount > 0)
        {
            bulletCountGradient.enabled = false;
            timeUtils.TimeDelay(0.05f, () => { bulletCountGradient.enabled = true; });
            bulletCountGUI.fontSize = armsFontSize;
            bulletCountGUI.text = bulletCount.ToString();
        }
        else
        {
            SetBulletCountToInfinity();
        }
    }

    public void ShowMissionStartAnimation()
    {
        missionLetters.SetStart();
        missionLetters.StartAnim();
    }

    void SetBulletCountToInfinity()
    {
        bulletCountGUI.text = "∞";
        bulletCountGUI.fontSize = armsFontSizeWhenGun;
    }

    void OnPausePressed()
    {
        // UIManager.Instance.Dialog.Activate(DialogType.Pause);
    }

    void OnMissionStart()
    {
        SetVisible(true);
        ShowMissionStartAnimation();
        // SetLifeCount(GameManager.PlayerLifeCount);
        // SetScore(GameManager.PlayerScore);
    }

    void OnMissionSuccess()
    {
        missionLetters.SetComplete();
        missionLetters.StartAnim();
    }

    void OnPlayerPointsChanged()
    {
        // DOVirtual.DelayedCall(0.1f, () => SetScore(GameManager.PlayerScore));
    }

    void OnPlayerDeath()
    {
        // DOVirtual.DelayedCall(0.1f, () => SetLifeCount(GameManager.PlayerLifeCount));
    }

    // 체력에 따라 체력바 채우는 메서드

    // PowCount에 맞게 Pow를 활성화하는 메서드
    public void ActivatePow(int powCount)
    {
        ResetPowCount();

        // 활성화할 Pow 개수만큼 순차적으로 활성화, powCount는 2인데 3개가 출력되는 경우가 있음
        for (int i = 0; i < powCount; i++)
        {
            // 현재 인덱스에 해당하는 Pow를 활성화하고 인덱스 증가
            powObjects[currentPowIndex].SetActive(true);
            currentPowIndex = (currentPowIndex + 1) % powObjects.Length;
        }
    }

    // PowCount를 초기화하는 메서드
    public void ResetPowCount()
    {
        currentPowIndex = 0;
        // 모든 Pow 오브젝트를 비활성화
        foreach (GameObject powObject in powObjects)
        {
            powObject.SetActive(false);
        }
    }

    void UpdateTime()
    {
        currentTime -= Time.deltaTime / 5f;
        if (currentTime <= 0)
        {
            GameManager.Instance.SetGameOver();
            // currentTime = 61f; 이건 currentTime 초기화
        }
    }

    void UpdateTimeDisplay()
    {
        // 남은 시간에서 십의 자리와 일의 자리 숫자 계산
        int tensPlace = Mathf.FloorToInt(currentTime / 10);
        int onesPlace = Mathf.FloorToInt(currentTime % 10);

        // 계산된 숫자에 해당하는 이미지를 표시
        tensTimeImage.sprite = timeNumberSprites[tensPlace];
        onesTimeImage.sprite = timeNumberSprites[onesPlace];
    }

    // 맵의 일정 부분을 넘어가면 시간초 초기화 해야하고, 게임을 다시 시작한 경우 시간 초기화 해야함
    public void ResetTime()
    {
        currentTime = 61f;
    }

    void CheckGameOver()
    {
        if(GameManager.Instance.IsGameOver())
        {
            lifeScoreBar.SetActive(false);
            munitionsGroup.SetActive(false);
            topContinue.SetActive(true);
            centerContinue.SetActive(true);
            SoundManager.Instance.PlayContinueSiren();

            // 게임 오버 상태에서는 이 메서드가 호출되지 않도록 이벤트 리스너를 제거
            EventManager.StopListening(GlobalEvents.GameOver, CheckGameOver);
        }
    }

    void Restart()
    {
        topContinue.SetActive(false);
        centerContinue.SetActive(false);
        lifeScoreBar.SetActive(true);
        munitionsGroup.SetActive(true);

        SoundManager.Instance.ClearEffectSource();

        // 재시작시 게임 오버 이벤트 등록
        EventManager.StartListening(GlobalEvents.GameOver, CheckGameOver);
    }

    public void AddCredit()
    {
        credit++;
        UpdateCreditDisplay();
    }

    public void ResetCredit()
    {
        credit = 0;
        UpdateCreditDisplay();
    }

    void UpdateCreditDisplay()
    {
        // 십의 자리와 일의 자리 숫자 계산
        int tensPlace = credit / 10;
        int onesPlace = credit % 10;

        // 계산된 숫자에 해당하는 이미지를 표시
        tensCreditImage.sprite = creditNumberSprites[tensPlace];
        onesCreditImage.sprite = creditNumberSprites[onesPlace];
    }
}