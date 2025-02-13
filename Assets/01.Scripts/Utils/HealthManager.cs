using _01.Scripts.UI;
using UnityEngine;

// 체력을 관리하는 클래스
namespace _01.Scripts.Utils
{
    public class HealthManager : MonoBehaviour
    {
        public int maxHP = 100; // 최대 체력
        public int currentHP = 0;
        public int lifeCount = 2;
        private IDamaged[] _componInterestedInDamages;   // 데미지를 받는 컴포넌트들의 배열
        public int interestedInDamagesCount = 0;        // 데미지를 받는 컴포넌트의 개수

        public int MaxHp => maxHP;
        public int CurrentHp { get; set; }      // 현재 체력
        public bool IgnoreDamages { get; set; } // 데미지를 무시하는지 여부
 
        public delegate void OnDamageEvent();
        public OnDamageEvent onDead;

        public delegate void OnDestroyEvent();
        public OnDestroyEvent OnDestroy;

        private EnemyController _enemyController;

        private void Start()
        {
            _componInterestedInDamages = GetComponents<IDamaged>();
            interestedInDamagesCount = _componInterestedInDamages.Length;
            CurrentHp = maxHP;
            currentHP = CurrentHp;
        }

        private void OnEnable()
        {
            CurrentHp = maxHP;

            if (gameObject.name.Contains("Crab"))
            {
                _enemyController = gameObject.GetComponent<EnemyController>();
            }
        }

        // 투사체에 의해 공격을 받은 경우
        public void OnHitByProjectile(ProjectileProperties projectile)
        {
            // 데미지를 무시하거나 현재 체력이 0 이하이면 처리 중단
            if (IgnoreDamages || CurrentHp <= 0)
            {
                OnDestroy?.Invoke();
                onDead?.Invoke();

                return;
            }

            CurrentHp -= projectile.strength; // 투사체의 강도만큼 체력 감소
            currentHP = CurrentHp;
            NotifyDamageWasTaken(projectile); // 데미지를 받았음을 관련 컴포넌트들에게 알림

            if(CurrentHp <= 0) onDead?.Invoke();

            if (GameManager.Instance.IsPlayer(gameObject)) 
            {
                if (lifeCount >= 0) HUDManager.Instance.SetLifeCount(lifeCount);
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
                if(gameObject.name.Contains("Crab") && _enemyController != null)
                {
                    _enemyController.OnHit();
                }
            }

            foreach (var t in _componInterestedInDamages)
            {
                t.OnDamageReceived(proj, lifeCount);
            }
        }

        public bool IsAlive()
        {
            return CurrentHp > 0;
        }
    }
}
