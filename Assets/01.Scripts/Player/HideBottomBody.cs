using UnityEngine;

// 상태 머신으로 캐릭터 bottom SpriteRender 관리
public class HideBottomBody : StateMachineBehaviour
{
    private HideBottomBodyPart hideBottomScript;
    public bool leaveHiddenOnExit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hideBottomScript == null)
        {
            hideBottomScript = animator.GetComponent<HideBottomBodyPart>();
        }
        hideBottomScript.HideBottomBody();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hideBottomScript == null)
        {
            hideBottomScript = animator.GetComponent<HideBottomBodyPart>();
        }
        if (!leaveHiddenOnExit)
        {
            hideBottomScript.ShowBottomBody();
        }
    }
}