using _01.Scripts.Utils;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject title;
    public GameObject soldierSelect;
    public GameObject marco;
    public GameObject eri;
    public GameObject tarma;
    public GameObject fio;
    public GameObject m3;
    public GameObject p1;
    public FadeInOut fadeInOut;

    private Animator[] _m3Animators;
    private bool _isTitleActivated = true;
    
    // 애니메이터 파라미터 캐싱
    private static readonly int Open = Animator.StringToHash("Open");

    private void Awake()
    {
        fadeInOut.FadeIn();
        _m3Animators = m3.GetComponentsInChildren<Animator>();
    }

    private void Update()
    {
        if(_isTitleActivated && Input.anyKeyDown)
        {
            _isTitleActivated = false; // 한 번만 실행되도록 플래그 설정
            EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayEffect, "insertCoin");

            // 페이드 아웃 실행 후 title을 비활성화
            fadeInOut.FadeOut(() =>
            {
                SetActiveSoldierSelect();
            });
        }
    }

    private void SetActiveSoldierSelect()
    {
        title.SetActive(false);
        // title이 비활성화된 후에 페이드인 실행
        fadeInOut.FadeIn();
        soldierSelect.SetActive(true);
        Invoke(nameof(PlayCharSelectSound), 1f);
        Invoke(nameof(OpenM3), 1f);
    }

    private void PlayCharSelectSound()
    {
        EventManager<SoundEventType>.TriggerEvent(SoundEventType.PlayMusic, "charSelect");
    }

    private void OpenM3()
    {
        foreach (var animator in _m3Animators)
        {
            animator.SetBool(Open, true);
        }
    }
}
