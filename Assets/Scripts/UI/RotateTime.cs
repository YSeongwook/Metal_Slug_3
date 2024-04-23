using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RotateTime : MonoBehaviour
{
    public Sprite[] sprites; // 변경할 스프라이트 배열
    public float rotationDuration = 2f; // 회전하는데 걸리는 시간(초)
    public float rotationAngle = 90f; // 회전 각도

    private Image image;
    private int currentImageIndex = 0;

    private void Start()
    {
        image = GetComponent<Image>();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.2f);
        StartCoroutine(ChangeImageAndRotateRoutine());
    }

    IEnumerator ChangeImageAndRotateRoutine()
    {
        float elapsedTime = 0f;
        float halfRotationDuration = rotationDuration / 2f;

        while (true)
        {
            // 이미지 회전 및 스프라이트 변경
            while (elapsedTime < halfRotationDuration)
            {
                float t = elapsedTime / halfRotationDuration;
                float angle = Mathf.Lerp(0f, rotationAngle, t);

                // 회전
                image.transform.eulerAngles = new Vector3(0f, angle, 0f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 마지막 배열 원소인 경우 루프 종료
            if (currentImageIndex == sprites.Length - 1)
            {
                // 부모 오브젝트 비활성화
                transform.parent.gameObject.SetActive(false);
                break;
            }

            // 스프라이트 변경
            currentImageIndex = (currentImageIndex + 1) % sprites.Length;
            image.sprite = sprites[currentImageIndex];

            // 이미지 회전
            while (elapsedTime < rotationDuration)
            {
                float t = (elapsedTime - halfRotationDuration) / halfRotationDuration;
                float angle = Mathf.Lerp(rotationAngle, 0f, t);

                // 회전
                image.transform.eulerAngles = new Vector3(0f, angle, 0f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 보정
            image.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            // 초기화
            elapsedTime = 0f;

            // 추가된 대기 시간
            yield return new WaitForSeconds(0.5f);
        }
    }
}
