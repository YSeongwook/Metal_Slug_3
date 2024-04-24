using EnumTypes;
using UnityEngine;
using Utils;

public class AnimationManager : MonoBehaviour, IObserver
{
    public Animator topAnimator;
    public Animator bottomAnimator;
    public Animator blood;
    public RuntimeAnimatorController deathAnimController;
    public RuntimeAnimatorController defaultAnimController;
    public GameObject deathWaterWave;
    public GameObject deathWaterMarco;

    private RetVoidTakeVoid EndOfDeathCB;
    private bool inExplosiveDeathAnim;

    public RetVoidTakeVoid grenadeCB;

    public void StartRunningAnim(bool isRunning)
    {
        if (topAnimator.runtimeAnimatorController.name != "MarcoDeath")
        {
            // 웅크리고 있지 않다면
            if (PlayerController.Instance.body != BodyPosture.Crouch)
            {
                topAnimator.SetBool("isRunning", isRunning);
                bottomAnimator.SetBool("isRunning", isRunning);
            }
            // 웅크리고 있다면
            else
            {
                topAnimator.SetBool("isRunning", isRunning);
                bottomAnimator.SetBool("isRunning", false);
            }
        }
    }

    public void StopRunningAnim()
    {
        if (topAnimator.runtimeAnimatorController.name != "MarcoDeath")
        {
            topAnimator.SetBool("isRunning", false);
            bottomAnimator.SetBool("isRunning", false);
        }
    }

    public void StartLowVelJumpAnim()
    {
        bottomAnimator.SetTrigger("jump_low_speed");
        topAnimator.SetTrigger("jump_trigger");
        topAnimator.SetBool("jump_low_speed", true);
    }

    public void StartHighVelJumpAnim()
    {
        topAnimator.SetTrigger("jump_trigger");
        bottomAnimator.SetTrigger("jump_high_speed");
        topAnimator.SetBool("jump_high_speed", true);
    }

    public void StartTurnAnim()
    {
        topAnimator.SetTrigger("turn");
    }

    public void StartLookUpAnim()
    {
        if (!topAnimator.GetBool("up_pressed"))
        {
            if (!topAnimator.GetBool("jump_low_speed")
                    && !topAnimator.GetBool("jump_high_speed")
                    && !topAnimator.GetBool("down_pressed"))
            {
                topAnimator.SetTrigger("look_up_trigger");
            }
        }
        topAnimator.SetBool("up_pressed", true);
    }

    public void StartLookStraightAnim()
    {
        topAnimator.SetBool("down_pressed", false);
        bottomAnimator.SetBool("down_pressed", false);
        topAnimator.SetBool("up_pressed", false);
    }

    public void StartCrouchAnim()
    {
        topAnimator.SetTrigger("sit");
        topAnimator.SetBool("down_pressed", true);
        bottomAnimator.SetBool("down_pressed", true);
        bottomAnimator.SetBool("isRunning", false);
    }

    public void StartLookDownAnim()
    {
        topAnimator.SetBool("down_pressed", true);
    }

    public void StartStandingUpAnim()
    {
        StartLookStraightAnim();
    }

    public void Observe(SlugEvents ev)
    {
        if (!topAnimator.isInitialized)
        {
            return;
        }

        if (ev == SlugEvents.Fall && !inExplosiveDeathAnim)
        {
            topAnimator.SetTrigger("jump_trigger");
            topAnimator.SetBool("jump_low_speed", true);
            bottomAnimator.SetTrigger("jump_low_speed");
        }
        else if (ev == SlugEvents.HitGround)
        {
            if (inExplosiveDeathAnim)
            {
                topAnimator.applyRootMotion = false;
                topAnimator.SetTrigger("touch_ground_death");
                return;
            }
            topAnimator.SetTrigger("hit_ground");
            bottomAnimator.SetTrigger("hit_ground");
            topAnimator.SetBool("jump_low_speed", false);
            topAnimator.SetBool("jump_high_speed", false);
        }
    }

    public void StartGrenadeAnim(RetVoidTakeVoid cb)
    {
        topAnimator.SetTrigger("grenade");
        grenadeCB = cb;
    }

    public void PlaySpawnAnim()
    {
        topAnimator.Play("marco-spawn");
    }

    public void PlayDeathAnimation(ProjectileProperties proj, RetVoidTakeVoid cb)
    {
        string trigger;

        topAnimator.runtimeAnimatorController = deathAnimController;

        if (proj.type == ProjectileType.Grenade)
        {
            trigger = "explo";
            inExplosiveDeathAnim = true;
        }
        else if (proj.type == ProjectileType.Knife)
        {
            trigger = "slash";
            blood.Play("1");
        }
        else if(proj.type == ProjectileType.Water)
        {
            topAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            bottomAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;

            deathWaterWave.SetActive(true);
            Invoke("ActiveDeathWaterMarco", 0.4f);
            return;
        }
        else
        {
            trigger = "slash";
        }
        EndOfDeathCB = cb;
        topAnimator.SetTrigger(trigger);
    }

    public void EndOfDeathAnim()
    {
        if (EndOfDeathCB != null)
        {
            EndOfDeathCB();
        }
    }

    public void MissionCompleteAnim()
    {
        topAnimator.SetTrigger("mission_complete");
    }

    public void ResetAnimators()
    {
        topAnimator.runtimeAnimatorController = defaultAnimController;
        topAnimator.Rebind();
        bottomAnimator.Rebind();
        inExplosiveDeathAnim = false;
    }

    void ActiveDeathWaterMarco()
    {
        deathWaterMarco.SetActive(true);
    }
}