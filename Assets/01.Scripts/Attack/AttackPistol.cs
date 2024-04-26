using UnityEngine;
using EnumTypes;
using EventLibrary;

public class AttackPistol : MonoBehaviour, IAttack
{
    public Animator anim;
    public RuntimeAnimatorController attackAnimatorController;
    public ObjectPool bullettPool;
    public AudioManager audioManager;

    public Transform bulletInitialPosition;
    public Transform bulletInitialPositionSitting;
    public Transform bulletInitialPositionRunning;
    public Transform bulletInitialPositionLookingUp;
    public Transform bulletInitialPositionLookingDown;

    private BodyPosture body;
    private Vector2 lookingDirection;

    public void Execute(string victimTag, Vector3 unused, Vector3 unused2)
    {
        body = PlayerController.Instance.body;
        lookingDirection = PlayerController.Instance.LookingDirection;

        anim.runtimeAnimatorController = attackAnimatorController;
        anim.SetTrigger("fire");
        GameObject bulletGameObject = bullettPool.GetPooledObject();

        if (bulletGameObject != null )
        {
            bulletGameObject.transform.position = GetProjPosInit();
            bulletGameObject.transform.right = lookingDirection;
            IProjectile bullet = bulletGameObject.GetComponentInChildren<IProjectile>();
            bullet.Launch(victimTag);
            audioManager.PlaySound(0);
            EventManager.TriggerEvent(GlobalEvents.GunUsed, 0);
        } 
    }

    // lookingDirection을 이용해서 발사체 위치 설정
    private Vector3 GetProjPosInit()
    {
        switch (body)
        {
            case BodyPosture.Stand:
                if (lookingDirection == Vector2.up)
                {
                    return bulletInitialPositionLookingUp.position;
                }
                else if(lookingDirection == Vector2.down)
                {
                    return bulletInitialPositionLookingDown.position;
                }
                else
                {
                    return bulletInitialPositionRunning.position;
                }

            case BodyPosture.Crouch:
                return bulletInitialPositionSitting.position;

            case BodyPosture.InTheAir:
                if (lookingDirection == Vector2.up)
                {
                    return bulletInitialPositionLookingUp.position;
                }
                else if (lookingDirection == Vector2.down)
                {
                    return bulletInitialPositionLookingDown.position;
                }
                else
                {
                    return bulletInitialPosition.position;
                }

            default:
                return bulletInitialPosition.position;
        }
    }
}