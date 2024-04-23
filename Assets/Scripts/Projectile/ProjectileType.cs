using UnityEngine;
using System;
using Random = UnityEngine.Random;

// 투사체 종류
[Serializable]
public enum ProjectileType
{
    Bullet,
    Knife,
    Grenade,
    Flame,
    Water
}

[Serializable]
public class ProjectileProperties
{
    public int strength = 1;                            // 투사체 강도
    public int speedInUnityUnitPerSec = 5;              // 초당 이동 속도
    public ProjectileType type;                         // 투사체 종류
    public RuntimeAnimatorController explosionAnimator; // 폭발 애니메이션 컨트롤러
    public AudioClip explosionSound;                    // 폭발 사운드

    [HideInInspector]
    public string victimTag = "Enemy";              // 피격 대상 태그
}

// 투사체 유틸리티 클래스
public class ProjectileUtils
{
    // 충돌한 콜라이더에게 투사체 피격을 알림
    public static void NotifyCollider(Collider2D col, ProjectileProperties projProp)
    {
        HealthManager healthManager = col.GetComponentInChildren<HealthManager>();
        if (healthManager != null)
        {
            healthManager.OnHitByProjectile(projProp);
        }
    }

    // 직선으로 이동하는 투사체의 위치를 업데이트
    public static void UpdatePositionStraightLine(Transform proj, ProjectileProperties projProp)
    {
        proj.Translate(Vector3.right * projProp.speedInUnityUnitPerSec * Time.deltaTime * 4);
    }

    // 폭발 애니메이션과 사운드를 재생
    public static void ImpactAnimationAndSound(Transform proj, Collider2D col, ProjectileProperties projProp)
    {
        Animator anim = AnimatorPool.GetPooledAnimator(); // 오브젝트 풀에서 애니메이터 가져오기
        anim.transform.right = Vector2.right; // 애니메이터의 방향을 오른쪽으로 설정
        anim.runtimeAnimatorController = projProp.explosionAnimator; // 폭발 애니메이션 설정
        anim.transform.position = (Vector2)proj.transform.position + Random.insideUnitCircle * 0.05f; // 랜덤한 위치에 애니메이터 배치
        anim.Play("Explosion"); // 애니메이션 재생

        if (projProp.explosionSound != null) // 폭발 사운드가 지정되어 있으면
        {
            AudioSource audio = anim.GetComponent<AudioSource>(); // 애니메이터에서 AudioSource 컴포넌트 가져오기
            audio.clip = projProp.explosionSound; // 폭발 사운드 설정
            audio.Play(); // 사운드 재생
        }
    }

    // 폭발 애니메이터를 가져오는 메서드
    public static Animator GetImpactAnimator(Transform proj, ProjectileProperties projProp)
    {
        Animator anim = AnimatorPool.GetPooledAnimator(); // 오브젝트 풀에서 애니메이터 가져오기
        anim.transform.right = Vector2.right; // 애니메이터의 방향을 오른쪽으로 설정
        anim.runtimeAnimatorController = projProp.explosionAnimator; // 폭발 애니메이션 설정
        anim.transform.position = (Vector2)proj.transform.position + Random.insideUnitCircle * 0.05f; // 랜덤한 위치에 애니메이터 배치
        return anim; // 애니메이터 반환
    }

    // 폭발 위치를 랜덤하게 조정
    public static void RandomizeImpactPosition(Transform proj, Transform impact)
    {
        impact.position = (Vector2)proj.position + Random.insideUnitCircle * 0.035f; // 랜덤한 위치에 설정
        impact.Translate(proj.right * 0.035f); // 폭발 위치를 오른쪽으로 이동
    }
}
