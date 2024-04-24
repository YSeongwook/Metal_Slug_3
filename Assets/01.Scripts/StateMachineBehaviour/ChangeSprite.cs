using UnityEngine;

public class ChangeSprite : StateMachineBehaviour
{
    public Sprite sprite;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
