using System.Collections;
using UnityEngine;

public class SimpleAnimator : MonoBehaviour
{
    // 일정 시간 이후에 비활성화할 대기 시간
    public float disableDelay = 1.2f;

    void OnEnable()
    {
        DisableObjectAfterDelay();
    }

    // 오브젝트를 비활성화하는 메서드
    private void DisableObjectAfterDelay()
    {
        StartCoroutine(DisableObjectCoroutine());
    }

    // 코루틴을 사용하여 대기 후 오브젝트를 비활성화하는 메서드
    private IEnumerator DisableObjectCoroutine()
    {
        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(disableDelay);

        // 대기 시간이 지난 후에 오브젝트를 비활성화
        gameObject.SetActive(false);
    }
}
