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

    private Animator[] m3Animators;

    private bool isTitleActivated = true;

    void Awake()
    {
        fadeInOut.FadeIn();
        m3Animators = m3.GetComponentsInChildren<Animator>();
    }

    void Update()
    {
        if(isTitleActivated && Input.anyKeyDown)
        {
            isTitleActivated = false; // 한 번만 실행되도록 플래그 설정
            SoundManager.Instance.PlayInsertCoin();

            // 페이드 아웃 실행 후 title을 비활성화
            fadeInOut.FadeOut(() =>
            {
                SetActiveSoldierSelect();
            });
        }
    }

    void SetActiveSoldierSelect()
    {
        title.SetActive(false);
        // title이 비활성화된 후에 페이드인 실행
        fadeInOut.FadeIn();
        soldierSelect.SetActive(true);
        Invoke("PlayCharSelectSound", 1f);
        Invoke("OpenM3", 1f);
    }

    void PlayCharSelectSound()
    {
        SoundManager.Instance.PlayCharSelect();
    }

    void OpenM3()
    {
        foreach (Animator animator in m3Animators)
        {
            animator.SetBool("Open", true);
        }
    }
}
