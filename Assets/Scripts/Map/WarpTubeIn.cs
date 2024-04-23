using UnityEngine;

public class WarpTubeIn : MonoBehaviour
{
    public GameObject marco;
    public SpriteRenderer bottom;
    public SpriteRenderer top;

    private GameObject marcoWarp;
    private GameObject warpTubeCover;

    // 이동할 좌표
    public Transform warpDestination;

    private void Awake()
    {
        marcoWarp = transform.GetChild(0).gameObject;
        warpTubeCover = transform.GetChild(1).gameObject;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 충돌한 오브젝트가 플레이어인지 확인하고, 애니메이션이 재생되지 않았다면 실행
        if (col.CompareTag("Player"))
        {
            SetSpriteRenderer(false);

            marcoWarp.GetComponent<SpriteRenderer>().enabled = true;
            marcoWarp.GetComponent<Animator>().SetTrigger("In");

            Invoke("PlayWarpTubeInAnim", 0.5f);
            warpTubeCover.SetActive(false);

            Invoke("TeleportPlayer", 1f);
        }
    }

    // 플레이어를 순간 이동하는 함수
    private void TeleportPlayer()
    {
        marco.transform.position = warpDestination.position;
        CameraManager.Instance.SwitchZ2AtoZ3A();
    }

    void PlayWarpTubeInAnim()
    {
        GetComponent<Animator>().SetTrigger("In");
    }

    void SetSpriteRenderer(bool isActive)
    {
        top.enabled = isActive;
        bottom.enabled = isActive;
        top.gameObject.SetActive(isActive);
    }
}
