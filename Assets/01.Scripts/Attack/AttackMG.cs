using UnityEngine;
using System.Collections;
using EnumTypes;

public class AttackMG : MonoBehaviour, IAttack
{
    public Animator anim;
    public RuntimeAnimatorController mgAnimatorController;
    public ObjectPool bullettPool;
    public AttackManager attackManager;
    public AudioManager audioManager;
    private TimeUtils timeUtils;

    public MGAnimationEvents mgAE;
    private float yOffset;
    private string vicTag;
    public Transform[] bulletDiagUpPos;
    public Transform[] bulletDiagDownPos;
    public Transform bulletHoriz;
    public Transform bulletUp;
    public Transform bulletSitting;
    public Transform bulletDown;

    private GameObject bulletGameObject;
    private BodyPosture body;
    private Vector2 lookingDirection;
    private int offsetIndex = 0;

    public void Start()
    {
        timeUtils = GetComponent<TimeUtils>();
        mgAE.SetMgHorizontalCB(() => Shoot(bulletHoriz));
        mgAE.SetMgDiagUp((int animIndex) => Shoot(bulletDiagUpPos[animIndex]));
        mgAE.SetMgDiagDown((int animIndex) => Shoot(bulletDiagDownPos[animIndex]));
        mgAE.SetMgUpCB(() => Shoot(bulletUp));
        mgAE.SetMgSittingCB(() => Shoot(bulletSitting));
        mgAE.SetMgDownCB(() => Shoot(bulletDown));
    }

    private void Shoot(Transform initTransform)
    {
        bulletGameObject = bullettPool.GetPooledObject();

        lookingDirection = PlayerController.Instance.LookingDirection;

        bulletGameObject.transform.position = initTransform.position;
        bulletGameObject.transform.rotation = initTransform.rotation;

        if (lookingDirection == Vector2.down) bulletGameObject.transform.rotation = new Quaternion(0f, 0f, -0.70711f, 0.70711f);

        // offsetIndex에 따라서 총알의 높이를 조절
        yOffset = 0.12f * offsetIndex;
        bulletGameObject.transform.Translate(0, 0.18f - yOffset, 0, Space.Self);

        offsetIndex = (offsetIndex + 1) % 3;

        IProjectile bullet = bulletGameObject.GetComponentInChildren<IProjectile>();
        bullet.Launch(vicTag);
        attackManager.UpdateBulletCount();
    }

    public void Execute(string victimTag, Vector3 unused, Vector3 unused2)
    {
        vicTag = victimTag;

        if (timeUtils != null) timeUtils.TimeDelay(0.15f, () => anim.SetBool("machineGunning", false));

        anim.runtimeAnimatorController = mgAnimatorController;
        anim.SetBool("machineGunning", true);

        audioManager.PlaySoundSameSource(3);
    }
}