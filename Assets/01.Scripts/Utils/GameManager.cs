using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EnumTypes;
using System.Linq;
using EventLibrary;

public class GameManager : Singleton<GameManager>
{
    float totalGameTime;
    bool isBossSpawn;
    bool isGameOver;
    int score = 0;
    int initialBombs = 10;
    int bombs;
    int heavyMachineAmmo = 0;
    int powCount = 0;
    float bgmAudio = 1f;
    float sfxAudio = 1f;
    Missions InstanceMission = Missions.Home;
    float mission1Points = 0f;
    float mission2Points = 0f;
    float mission3Points = 0f;

    void Awake()
    {
        // LoadSettings();
        // LoadRecords();
        // SaveRecords
    }

    void Start()
    {
        GameReset();
    }

    void Update()
    {
        totalGameTime += Time.deltaTime;
    }

    public bool ToggleGodMode()
    {
        var player = GetPlayer();
        if (player)
        {
            var health = player.GetComponent<HealthManager>();
            health.CurrentHP = 1000;
            // SetBombs(200);
            return true;
        }
        return false;
    }

    public void AddScore(float amount)
    {
        int intAmount = (int)amount;
        Instance.score += intAmount;
        HUDManager.Instance.SetScore(score);
    }

    public int GetScore()
    {
        return Instance.score;
    }

    public int GetBombs()
    {
        return Instance.bombs;
    }

    public void RemoveBomb()
    {
        Instance.bombs--;
        // UIManager.UpdateBombsUI();
    }

    public int GetHeavyMachineAmmo()
    {
        //Return the state of the game
        return Instance.heavyMachineAmmo;
    }

    public void SetHeavyMachineAmmo(int ammo)
    {
        //Return the state of the game
        Instance.heavyMachineAmmo = ammo;
    }

    public void SetGameOver()
    {
        isGameOver = true;
        EventManager.TriggerEvent(GlobalEvents.GameOver);
    }

    public void SetGameOverRespawn()
    {
        isGameOver = false;
    }

    public bool IsGameOver()
    {
        //Return the state of the game
        return Instance.isGameOver;
    }

    public void SetBossSpawn()
    {
        isBossSpawn = true;
        EventManager.TriggerEvent(GlobalEvents.BossSpawn);
    }

    public bool IsBossSpawn()
    {
        return Instance.isBossSpawn;
    }

    // 게임 재시작 메서드 필요

    public void PlayerDied()
    {
        Instance.isGameOver = true;

        // UIManager.DisplayGameOverText();
        SoundManager.Instance.PlayGameOverAudio();

        Instance.StartCoroutine(Instance.WaitHome());
    }

    public void PlayerWin()
    {
        // UIManager.DisplayWinText();
        SoundManager.Instance.PlayLevelCompleteAudio();
        SoundManager.Instance.PlayGameOverAudio();

        Instance.isGameOver = true;

        Instance.InstanceMission = (Missions)SceneManager.GetActiveScene().buildIndex;

        if (Instance.InstanceMission >= Missions.Mission3Boss)
        {
            Instance.InstanceMission = Missions.Home;
        }
        else
        {
            Instance.InstanceMission++;
        }

        Instance.StartCoroutine(Instance.WaitNextMission());
    }

    public GameObject GetPlayer() // not cached
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    public GameObject GetPlayer(GameObject player)
    {
        if (player.GetComponent<PlayerController>()) // return itself
            return player;
        else if (player.transform.parent.gameObject.GetComponent<PlayerController>()) // return parent
            return player.transform.parent.gameObject;
        return GameObject.FindGameObjectWithTag("Player"); // return uncached finded by tag
    }

    public GameObject GetPlayer(Collider2D collider)
    {
        return GetPlayer(collider.gameObject);
    }

    public GameObject GetPlayer(Collision2D collision)
    {
        return GetPlayer(collision.collider);
    }

    public GameObject GetRunningTarget() // not cached
    {
        return GameObject.FindGameObjectWithTag("RunningTarget");
    }

    public bool IsPlayer(GameObject player)
    {
        if (player.CompareTag("Player")) return true;
        else return false;
    }

    public bool IsPlayer(Collider2D collider)
    {
        return IsPlayer(collider.gameObject);
    }

    public bool IsPlayer(Collision2D collision)
    {
        return IsPlayer(collision.collider);
    }

    public bool CanTriggerThrowable(Collider2D collider)
    {
        string[] triggerableTags = { "Enemy", "Building", "Walkable", "Roof", "Bridge", "EnemyBomb", "World" };
        string tag = collider.tag;

        return IsPlayer(collider) || triggerableTags.Contains(tag);
    }

    // 포로 구출 시 호출되는 메서드
    public void RescuePow()
    {
        // PowCount 증가
        powCount++;

        // HUDManager를 통해 PowCount 갱신
        HUDManager.Instance.ActivatePow(powCount);
    }

    // PowCount 초기화 메서드
    public void ResetPow()
    {
        powCount = 0;
        HUDManager.Instance.ResetPowCount();
    }

    public int GetPowCount()
    {
        return powCount; 
    }

    public void SetBgmAudio(float bgmAudio, bool save = false)
    {
        Instance.bgmAudio = bgmAudio;
        // if (save) Instance.SaveSettings();
    }

    public float GetBgmAudio()
    {
        return Instance.bgmAudio;
    }

    public void SetSfxAudio(float sfxAudio, bool save = false)
    {
        Instance.sfxAudio = sfxAudio;

        // if (save) Instance.SaveSettings();
    }

    public float GetSfxAudio()
    {
        return Instance.sfxAudio;
    }

    public void SetMissionPoints(int missionIndex, float points)
    {
        switch (missionIndex)
        {
            case 1:
                Instance.mission1Points = points;
                break;
            case 2:
                Instance.mission2Points = points;
                break;
            case 3:
                Instance.mission3Points = points;
                break;
            default:
                Debug.LogError("Invalid mission index: " + missionIndex);
                break;
        }
    }

    public float GetMissionPoints(int missionIndex)
    {
        switch (missionIndex)
        {
            case 1:
                return Instance.mission1Points;
            case 2:
                return Instance.mission2Points;
            case 3:
                return Instance.mission3Points;
            default:
                Debug.LogError("Invalid mission index: " + missionIndex);
                return 0f;
        }
    }

    public void GameReset()
    {
        Time.timeScale = 1;
        isGameOver = false;
        score = 0;
        totalGameTime = 0;
        bombs = initialBombs;
        powCount = 0;
    }

    public void PauseExit()
    {
        LoadHome();
    }

    public void LoadHome()
    {
        LoadScene((int)Missions.Home);
    }

    public void LoadNextMission()
    {
        // InstanceMission is updated in the PlayerWin method
        LoadScene((int)Instance.InstanceMission);
    }

    public bool CanTriggerEnemyBombs(string tag)
    {
        return tag == "Player" || tag == "Walkable" || tag == "Marco Boat" || tag == "Bridge";
    }

    private IEnumerator WaitHome()
    {
        yield return new WaitForSeconds(7f);
        LoadHome();
    }

    private IEnumerator WaitNextMission()
    {
        yield return new WaitForSeconds(7f);
        LoadNextMission();
    }

    public void LoadScene(int id, bool skipReset = false)
    {
        if (!skipReset) GameReset();

        SceneManager.LoadScene(id);
    }
}