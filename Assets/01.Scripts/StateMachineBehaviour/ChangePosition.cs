using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePosition : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // animator.transform.position = new Vector2(animator.transform.position.x, animator.transform.position.y + 5f);
        animator.SetBool("isWalking", true);
    }
}
