using EnumTypes;
using EventLibrary;
using UnityEngine;

public class HippieFreedom : MonoBehaviour, IDamaged, IObserver, ICheckCollision
{
    public AudioManager audioManager;
    private HippieAnimationManager animManager;
    private Items giftToPlayer;
    private bool itemOffered;
    private delegate void VoidNullFunction();
    private VoidNullFunction HippiesBrain;
    private VoidNullFunction HippiesBrainBackup;
    private float rayDistance = 0.4f;
    private HippieMove hippieMove;

    void Awake()
    {
        giftToPlayer = GetComponentInChildren<Items>(true);
        animManager = GetComponent<HippieAnimationManager>();
        hippieMove = GetComponent<HippieMove>();
        HippiesBrain = HippieTiedUp;
    }

    void Update()
    {
        HippiesBrain();
        if(HippiesBrain == HippieWalkAround) CheckCollision();
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

        // 왼쪽이나 오른쪽 방향으로 플레이어를 검출한 경우
        if (hitLeft.collider != null || hitRight.collider != null)
        {
            GameManager.Instance.RescuePow();

            // HippiesBrain = HippieOfferItem 메서드 호출
            HippiesBrain = HippieOfferItem;

            // 왼쪽에 플레이어가 있을 때
            if (hitLeft.collider != null)
            {
                // 왼쪽 방향 설정
                transform.right = Vector2.left;

                hitLeft.collider.gameObject.GetComponent<ItemManager>().CanGetItem();
            }
            // 오른쪽에 플레이어가 있을 때
            else if (hitRight.collider != null)
            {
                // 오른쪽 방향 설정
                transform.right = Vector2.right;

                hitRight.collider.gameObject.GetComponent<ItemManager>().CanGetItem();
            }
        }
    }

    private void HippieTiedUp() { }

    public void OnDamageReceived(ProjectileProperties projectileProp, int newHP)
    {
        animManager.PlayFreeAnim(EndOfHippieFreedAnim);
        gameObject.layer = (int)Layers.FreeMan;
        EventManager.TriggerEvent(GlobalEvents.PointsEarned, 100);
    }

    private void EndOfHippieFreedAnim()
    {
        HippiesBrain = HippieWalkAround;
        animManager.PlayWalkingAnim(EndOfHippieWalkAnim);
    }

    private void HippieWalkAround() { }

    private void EndOfHippieWalkAnim()
    {
        // 방향 전환
        transform.right = -transform.right;
    }

    private void HippieOfferItem()
    {
        if (!itemOffered)
        {
            animManager.PlayGiftAnim(EndOfHippieSalutAnim);
            audioManager.PlaySound(0);
        }

        itemOffered = true;
    }

    public void HippieOfferItemAnimEvent()
    {
        giftToPlayer.gameObject.SetActive(true);
        giftToPlayer.transform.parent = transform.parent;
    }

    private void EndOfHippieSalutAnim()
    {
        HippiesBrain = HippieRunAway;
    }

    private bool runAwayStarted;
    private void HippieRunAway()
    {
        if (!runAwayStarted) runAwayStarted = true;

        transform.right = Vector2.left;
        hippieMove.isMovable = true;
    }

    void OnBecameInvisible()
    {
        if (itemOffered) Destroy(gameObject);
    }

    public void Observe(SlugEvents ev)
    {
        if (ev == SlugEvents.Fall)
        {
            HippiesBrainBackup = HippiesBrain;
            HippiesBrain = HippieTiedUp;
        }
        else if (ev == SlugEvents.HitGround)
        {
            HippiesBrain = HippiesBrainBackup;
        }
    }

    public void OnDamageReceived(int newHp)
    {
        throw new System.NotImplementedException();
    }
}
