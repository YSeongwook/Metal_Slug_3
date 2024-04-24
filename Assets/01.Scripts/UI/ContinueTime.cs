using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ContinueTime : MonoBehaviour
{
    public Sprite[] sprites; // 변경할 스프라이트 배열
    public float changeInterval = 2.5f; // 스프라이트 변경 간격(초)

    private Image image;
    private int currentImageIndex = 0;

    private void Start()
    {
        image = GetComponent<Image>();

        // 이미지 순차 변경 코루틴 시작
        StartCoroutine(ChangeImageRoutine());
    }

    IEnumerator ChangeImageRoutine()
    {
        // 이미지 변경을 계속 수행합니다.
        while (true)
        {
            // 현재 인덱스의 스프라이트로 변경
            image.sprite = sprites[currentImageIndex];

            // 다음 스프라이트 인덱스로 이동
            currentImageIndex = (currentImageIndex + 1) % sprites.Length;

            // 모든 스프라이트를 한 번씩 보여준 후 종료
            if (currentImageIndex == 0)
            {
                // 1초 후에 부모 오브젝트를 비활성화하는 함수를 호출
                Invoke("DisableParentObject", 2f);
                yield break; // 코루틴 종료
            }

            // 다음 변경까지 대기
            yield return new WaitForSeconds(changeInterval);
        }
    }

    // 부모 오브젝트를 비활성화하는 함수
    private void DisableParentObject()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
