using UnityEngine;

// 체력을 관리하는 클래스
public class HealthManager : MonoBehaviour
{
    public int maxHP = 100; // 최대 체력
    public int currentHP = 0;
    public int lifeCount = 2;
    private IDamaged[] componInterestedInDamages;   // 데미지를 받는 컴포넌트들의 배열
    public int interestedInDamagesCount = 0;        // 데미지를 받는 컴포넌트의 개수

    public int MaxHP {  get { return maxHP; } }
    public int CurrentHP { get; set; }      // 현재 체력
    public bool IgnoreDamages { get; set; } // 데미지를 무시하는지 여부

    public delegate void OnDamageEvent();
    public OnDamageEvent onDead;

    public delegate void OnDestroyEvent();
    public OnDestroyEvent OnDestroy;

    private EnemyController enemyController;

    void Start()
    {
        componInterestedInDamages = GetComponents<IDamaged>();
        interestedInDamagesCount = componInterestedInDamages.Length;
        CurrentHP = maxHP;
        currentHP = CurrentHP;
    }

    void OnEnable()
    {
        CurrentHP = maxHP;

        if (gameObject.name.Contains("Crab"))
        {
            enemyController = gameObject.GetComponent<EnemyController>();
        }
    }

    // 투사체에 의해 공격을 받은 경우
    public void OnHitByProjectile(ProjectileProperties projectile)
    {
        // 데미지를 무시하거나 현재 체력이 0 이하이면 처리 중단
        if (IgnoreDamages || CurrentHP <= 0)
        {
            OnDestroy?.Invoke();
            onDead?.Invoke();

            return;
        }
        else
        {
            CurrentHP -= projectile.strength; // 투사체의 강도만큼 체력 감소
            currentHP = CurrentHP;
            NotifyDamageWasTaken(projectile); // 데미지를 받았음을 관련 컴포넌트들에게 알림

            if(CurrentHP <= 0) onDead?.Invoke();

            if (GameManager.Instance.IsPlayer(gameObject)) 
            {
                if (lifeCount >= 0) HUDManager.Instance.SetLifeCount(lifeCount);
            } 
        }

        if(!gameObject.CompareTag("Player"))
        {
            // 점수 오르는 메서드
            GameManager.Instance.AddScore(100);
        }
    }

    // 데미지를 받았을 때 관련 컴포넌트들에게 알리는 메서드
    private void NotifyDamageWasTaken(ProjectileProperties proj)
    {
        if (GameManager.Instance.IsPlayer(gameObject))
        {
            lifeCount--;
        } 
        else
        {
            // enemycontroller, soliderController OnHIt()

            if(gameObject.name.Contains("Crab") && enemyController != null)
            {
                enemyController.OnHit();
            }
        }

        for (int i = 0; i < componInterestedInDamages.Length; i++)
        {
            componInterestedInDamages[i].OnDamageReceived(proj, lifeCount);
        }
    }

    public bool IsAlive()
    {
        return CurrentHP > 0;
    }
}
