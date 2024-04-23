using System.Collections;
using UnityEngine;

public class BlinkObject : MonoBehaviour
{
    public float blinkInterval = 1f; // 깜빡임 간격(초)

    private void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // 게임 오브젝트의 활성/비활성 상태를 변경하여 깜빡이는 효과를 구현
            yield return new WaitForSeconds(blinkInterval);
            gameObject.SetActive(false); // 비활성화
            yield return new WaitForSeconds(blinkInterval);
            gameObject.SetActive(true); // 활성화
        }
    }
}