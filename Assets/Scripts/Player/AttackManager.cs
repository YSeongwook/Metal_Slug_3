using UnityEngine;
using EnumTypes;
using EventLibrary;

public class AttackManager : MonoBehaviour
{
    private PlayerController playerController;

    private IAttack[] FireArmAttacks;
    private IAttack currentFireArmAttack;
    private IAttack grenadeAttack;
    private AttackKnife MeleeAttack;
    private int currentAttackID = 1;
    public int bulletCount;
    public int grenadeCount = 10;
    private RuntimeAnimatorController gunAnimController;
    private FlashUsingMaterial flashBlue;

    public string victimsTag = "Enemy";
    public LayerMask enemyLayer;
    public Animator topBodyAnimator;

    public Transform grenadeInitialPositionStand;
    public Transform grenadeInitialPositionCrouch;
    public AudioManager audioManager;

    private Vector2 rayDirection;

    void Awake()
    {
        gunAnimController = topBodyAnimator.runtimeAnimatorController;
        MeleeAttack = GetComponentInChildren<AttackKnife>();
        FireArmAttacks = GetComponentsInChildren<IAttack>(true);
        grenadeAttack = GetComponent<AttackGrenade>();
        playerController = GetComponentInParent<PlayerController>();
        flashBlue = GetComponent<FlashUsingMaterial>();

        currentFireArmAttack = FireArmAttacks[1];
    }

    private void Update()
    {
        if (PlayerController.Instance.LookingDirection == Vector2.right)
        {
            rayDirection = Vector2.right;
        }
        else if (PlayerController.Instance.LookingDirection == Vector2.left)
        {
            rayDirection = Vector2.left;
        }
    }

    public void PrimaryAttack()
    {
        Vector3 unUsed = Vector3.zero;
        if (!MeleeAttack.InProgress())
        {
            if (InRangeForKnife())
            {
                MeleeAttack.Execute(victimsTag, Vector3.zero, Vector3.zero);
            }
            else
            {
                //TODO remove this projectileInitialPos parameter from Execute
                currentFireArmAttack.Execute(victimsTag, unUsed, unUsed);
            }
        }
    }

    public void SecondaryAttack()
    {
        if (grenadeCount > 0)
        {
            grenadeCount--;
            EventManager.TriggerEvent(GlobalEvents.GrenadeUsed, grenadeCount);

            Vector3 grenadeInitialPos;
            if (playerController.body == BodyPosture.Crouch)
            {
                grenadeInitialPos = grenadeInitialPositionCrouch.position;
            }
            else
            {
                grenadeInitialPos = grenadeInitialPositionStand.position;
            }
            grenadeAttack.Execute(victimsTag, Vector3.zero, grenadeInitialPos);
        }
    }

    public void RestoreGrenade()
    {
        grenadeCount = 10;
        // To refresh UI 
        EventManager.TriggerEvent(GlobalEvents.GrenadeUsed, grenadeCount);
    }

    public void UpdateBulletCount(int newBulletCount = 0)
    {
        if (newBulletCount == 0)
        {
            bulletCount--;
        }
        else
        {
            bulletCount = bulletCount + newBulletCount;
        }
        if (bulletCount < 1)
        {
            SetDefaultAttack();
        }
        // To refresh UI
        EventManager.TriggerEvent(GlobalEvents.GunUsed, bulletCount);
    }

    public void SetAttack(int attackID, RuntimeAnimatorController attackAnimController)
    {
        currentFireArmAttack = FireArmAttacks[attackID];
        topBodyAnimator.runtimeAnimatorController = attackAnimController;
    }

    public void SetDefaultAttack()
    {
        currentFireArmAttack = FireArmAttacks[1];
        topBodyAnimator.runtimeAnimatorController = gunAnimController;
        EventManager.TriggerEvent(GlobalEvents.GunUsed, bulletCount);
    }

    private bool InRangeForKnife()
    {
        rayDirection = Vector2.zero;
        if(PlayerController.Instance.LookingDirection == Vector2.right)
        {
            rayDirection = Vector2.right;
        } 
        else if(PlayerController.Instance.LookingDirection == Vector2.left)
        {
            rayDirection = Vector2.left;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y + 0.1f), rayDirection, 0.7f, enemyLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.tag == victimsTag || hits[i].collider.gameObject.layer == enemyLayer)
            {
                return true;
            }
        }
        return false;
    }
}