using UnityEngine;

public class HideParachute : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 애니메이션이 종료되면 연결된 게임 오브젝트를 비활성화
        animator.gameObject.SetActive(false);
    }
}
