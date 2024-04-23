using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FadeInOut : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 3f; // 페이드 인/아웃에 걸리는 시간 설정

    void Awake()
    {
        // canvasGroup.gameObject.SetActive(false);
    }

    public void FadeIn() //페이드 인 사용
    {
        StartCoroutine(Fade(true));
    }

    public void FadeOut(System.Action onComplete = null) //페이드 아웃 사용
    {
        StartCoroutine(Fade(false, onComplete));
    }

    private IEnumerator Fade(bool isFadeIn, System.Action onComplete = null)
    {
        if (isFadeIn)
        {
            canvasGroup.alpha = 0;
            canvasGroup.gameObject.SetActive(true);
            Tween tween = canvasGroup.DOFade(1f, fadeDuration - 1f);
            yield return tween.WaitForCompletion();
        }
        else
        {
            canvasGroup.alpha = 1;
            Tween tween = canvasGroup.DOFade(0f, fadeDuration);
            yield return tween.WaitForCompletion();
            canvasGroup.gameObject.SetActive(false);
        }
        // 페이드 아웃이 완료된 후에 onComplete 콜백 함수 호출
        onComplete?.Invoke();
    }
}
