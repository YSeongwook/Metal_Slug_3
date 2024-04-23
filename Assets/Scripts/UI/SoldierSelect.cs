using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoldierSelect : MonoBehaviour
{
    public GameObject[] soldiers;
    public GameObject m3;
    public GameObject p1;
    private Animator[] m3Animators;
    private int currentIndex = 0;
    private bool isInputEnabled = true;

    void Awake()
    {
        m3Animators = m3.GetComponentsInChildren<Animator>();

        // 초기에 모든 병사의 첫 번째 이미지만 활성화
        for (int i = 0; i < soldiers.Length; i++)
        {
            soldiers[i].transform.GetChild(0).gameObject.SetActive(true);
            soldiers[i].transform.GetChild(1).gameObject.SetActive(false);

            soldiers[0].transform.GetChild(0).gameObject.SetActive(false);
            soldiers[0].transform.GetChild(1).gameObject.SetActive(true);

            Invoke("DelayedActivateP1Child", 2.1f);
        }
    }

    void Update()
    {
        InputKey();
    }

    void InputKey()
    {
        // 입력을 받을 수 있는 상태인 경우에만 키 입력 처리
        if (isInputEnabled)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentIndex == 0) return;

                SetActiveSoldier((currentIndex - 1 + soldiers.Length) % soldiers.Length);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentIndex == 3) return;

                SetActiveSoldier((currentIndex + 1) % soldiers.Length);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                SoundManager.Instance.PlayPreSelect();
                SelectSoldier(currentIndex);
                SoundManager.Instance.PlaySelectMarco();

                // 입력을 받을 수 없도록 플래그 설정
                isInputEnabled = false;

                // 2초 뒤에 다른 scene으로 변경, 페이드 아웃, 페이드 인 효과 있어야함
            }
        }
    }

    // 지정된 인덱스의 병사만 두 번째 이미지 활성화
    void SetActiveSoldier(int index)
    {
        // 이전 병사의 이미지를 비활성화하고 새로운 병사의 이미지를 활성화
        soldiers[currentIndex].transform.GetChild(0).gameObject.SetActive(true);
        soldiers[currentIndex].transform.GetChild(1).gameObject.SetActive(false);
        ActivateP1Child(currentIndex, false);

        soldiers[index].transform.GetChild(0).gameObject.SetActive(false);
        soldiers[index].transform.GetChild(1).gameObject.SetActive(true);
        ActivateP1Child(index, true);

        currentIndex = index;
    }

    void SelectSoldier(int index)
    {
        soldiers[index].transform.GetChild(1).gameObject.SetActive(false);
        soldiers[index].transform.GetChild(2).gameObject.SetActive(true);
        Invoke("CloseM3", 0.5f);
        Invoke("LoadScene", 4f);
    }

    void CloseM3()
    {
        m3Animators[currentIndex].SetBool("Close", true);
        m3.transform.GetChild(currentIndex).GetChild(0).gameObject.SetActive(true);
    }

    // p1의 자식 오브젝트를 활성화하는 함수
    void ActivateP1Child(int index, bool b)
    {
        p1.transform.GetChild(index).gameObject.SetActive(b);
    }

    void DelayedActivateP1Child()
    {
        ActivateP1Child(currentIndex, true);
    }

    // 특정 씬을 로드하는 함수
    public void LoadScene()
    {
        SceneManager.LoadScene("Mission1");
    }

    // 현재 씬을 다시 로드하는 함수
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
