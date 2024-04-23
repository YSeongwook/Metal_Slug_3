using UnityEngine;

public class PreventMovement : StateMachineBehaviour
{
    private PlayerController playerController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController == null)
        {
            playerController = animator.GetComponentInParent<PlayerController>();
        }
        // BlockMovement(); 속도 조절하는 메서드인듯
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController == null)
        {
            playerController = animator.GetComponentInParent<PlayerController>();
        }
        // AllowMovement();
    }

    /*
    public void BlockMovement()
    {
        physics.SetMovementFactor(0);
    }

    public void AllowMovement()
    {
        if (body == BodyPosture.Crouch)
        {
            physics.SetMovementFactor(crouchSpeedFactor);
        }
        else if (body == BodyPosture.Stand)
        {
            physics.SetMovementFactor(physics.groundMovementFactor);
        }
    }
    */
}
