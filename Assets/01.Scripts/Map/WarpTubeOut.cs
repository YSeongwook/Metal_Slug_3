using UnityEngine;
using System.Collections;
using EnumTypes;
using EventLibrary;

public class WarpTubeOut : MonoBehaviour
{
    public GameObject player;
    public SpriteRenderer bottom;
    public SpriteRenderer top;

    private Animator animator;
    private Animator marcoWarp;
    private GameObject warpTubeCover;

    private bool triggerEnter = false;
    private bool triggerExit = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        marcoWarp = transform.GetChild(0).gameObject.GetComponent<Animator>();
        warpTubeCover = transform.GetChild(1).gameObject;

        EventManager.StartListening(GlobalEvents.BossSpawn, DisableGameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        StartCoroutine(OnTriggerEnterCoroutine(col));
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // StartCoroutine(OnTriggerExitCoroutine(col));
    }

    private IEnumerator OnTriggerEnterCoroutine(Collider2D col)
    {
        if (!triggerEnter)
        {
            triggerEnter = true;

            // 충돌한 오브젝트가 플레이어인지 확인하고, 애니메이션이 재생되지 않았다면 실행
            if (col.CompareTag("Player"))
            {
                yield return new WaitForSeconds(1f);

                animator.SetTrigger("Out");

                yield return new WaitForSeconds(0.8f);

                marcoWarp.GetComponent<SpriteRenderer>().enabled = true;
                marcoWarp.GetComponent<Animator>().SetTrigger("Out");
                warpTubeCover.SetActive(true);

                yield return new WaitForSeconds(0.5f);
                SetSpriteRenderer(true);
            }
        }
    }

    private IEnumerator OnTriggerExitCoroutine(Collider2D col)
    {
        if (!triggerExit)
        {
            triggerExit = true;

            // 충돌한 오브젝트가 플레이어인지 확인하고, 애니메이션이 재생되지 않았다면 실행
            if (col.CompareTag("Player"))
            {
                yield return new WaitForSeconds(0.5f);

                GetComponent<Animator>().SetTrigger("Hide");
                warpTubeCover.SetActive(false);
            }
        }
    }

    void SetSpriteRenderer(bool isActive)
    {
        bottom.gameObject.SetActive(isActive);
        top.gameObject.SetActive(isActive);
        bottom.enabled = isActive;
        top.enabled = isActive;
    }

    void DisableGameObject()
    {
        // 5초 후에 gameObject를 비활성화합니다.
        Invoke("Deactivate", 5f);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}