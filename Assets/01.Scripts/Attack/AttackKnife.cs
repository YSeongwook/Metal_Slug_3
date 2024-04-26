using UnityEngine;
using EventLibrary;
using EnumTypes;

public class AttackKnife : MonoBehaviour, IAttack
{
    public Animator anim;
    public AreaOfEffectProjectile knife;
    public AudioManager audioManager;
    private bool attackSwitch;
    public TimeUtils timeUtils;

    public void Execute(string victimTag, Vector3 unused, Vector3 unused2)
    {
        if (attackSwitch)
        {
            anim.SetTrigger("knife2");
            audioManager.PlaySound(4);
        }
        else
        {
            anim.SetTrigger("knife");
            audioManager.PlaySound(5);
        }
        attackSwitch = !attackSwitch;

        knife.CastAOE(victimTag, transform.position);
        EventManager.TriggerEvent(GlobalEvents.KnifeUsed);
        timeUtils.TimeDelay(0.2f, () => { anim.SetBool("knifing", false); });
    }

    public bool InProgress()
    {
        return anim.GetBool("knifing");
    }
}