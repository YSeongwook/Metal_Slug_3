using UnityEngine;

public class Items : MonoBehaviour, ICheckCollision
{
    private FlashUsingMaterial flash;
    private Animator animator;
    private Rigidbody2D rb;

    public RuntimeAnimatorController animController;
    public int bulletCount;
    public int attackID;
    public int points;
    public AudioClip weaponNameAudio;
    public string collectibleAnimationName;

    private float rayDistance = 0.4f;

    void Awake()
    {
        flash = GetComponent<FlashUsingMaterial>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckCollision();
    }

    public void OnEnable()
    {
        animator.Play(collectibleAnimationName);
    }

    private void DisableItem()
    {
        // 아이템 오브젝트를 비활성화
        gameObject.SetActive(false);
    }

    public void CheckCollision()
    {
        // 콜라이더의 가운데 위치 계산
        Vector2 colliderCenter = GetComponent<Collider2D>().bounds.center;

        // 왼쪽 방향으로 레이캐스트를 쏨
        RaycastHit2D hitLeft = Physics2D.Raycast(colliderCenter, Vector2.left, rayDistance, LayerMask.GetMask("Player"));
        Debug.DrawRay(colliderCenter, Vector2.left * rayDistance, Color.red);

        // 오른쪽 방향으로 레이캐스트를 쏨
        RaycastHit2D hitRight = Physics2D.Raycast(colliderCenter, Vector2.right, rayDistance, LayerMask.GetMask("Player"));
        Debug.DrawRay(colliderCenter, Vector2.right * rayDistance, Color.red);

        // 왼쪽이나 오른쪽 방향으로 플레이어와 충돌한 경우
        if (hitLeft.collider != null || hitRight.collider != null)
        {
            Invoke("DisableItem", 0.1f);

            flash.FlashForSingleFrame(() => animator.SetTrigger("picked_up"));
        }
    }
}