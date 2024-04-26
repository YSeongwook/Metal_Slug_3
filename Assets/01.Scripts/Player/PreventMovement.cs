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
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController == null)
        {
            playerController = animator.GetComponentInParent<PlayerController>();
        }
        // AllowMovement();
    }
}
