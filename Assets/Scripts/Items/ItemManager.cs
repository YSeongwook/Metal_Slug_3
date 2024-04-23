using UnityEngine;
using EventLibrary;

public class ItemManager : MonoBehaviour, ICheckCollision
{
    public AttackManager attackManager;
    public FlashUsingMaterial flashBlue;
    public AudioManager audioManager;
    public SpriteRenderer bottomSpriteRenderer;

    private float rayDistance = 0.5f;
    private bool collisionChecked = false;

    private void Update()
    {
        if (!collisionChecked) // 플래그가 false인 경우에만 실행
        {
            CheckCollision();
        }
    }

    public void CheckCollision()
    {
        // 콜라이더의 가운데 위치 계산
        Vector2 colliderCenter = GetComponent<Collider2D>().bounds.center;
        colliderCenter.y -= 0.2f;

        // 왼쪽 방향으로 레이캐스트를 쏨
        RaycastHit2D hitLeft = Physics2D.Raycast(colliderCenter, Vector2.left, rayDistance, LayerMask.GetMask("Item"));
        Debug.DrawRay(colliderCenter, Vector2.left * rayDistance, Color.red);

        // 오른쪽 방향으로 레이캐스트를 쏨
        RaycastHit2D hitRight = Physics2D.Raycast(colliderCenter, Vector2.right, rayDistance, LayerMask.GetMask("Item"));
        Debug.DrawRay(colliderCenter, Vector2.right * rayDistance, Color.red);

        // 왼쪽이나 오른쪽 방향으로 플레이어와 충돌한 경우
        if (hitLeft.collider != null || hitRight.collider != null)
        {
            // 충돌한 아이템을 찾아서 획득 처리
            Items item = hitLeft.collider != null ? hitLeft.collider.GetComponent<Items>() : hitRight.collider.GetComponent<Items>();
            if (item != null)
            {
                PickUpItem(item);

                // 플래그를 true로 설정하여 중복 호출 방지, 이것 때문에 아이템이 한번 밖에 획득이 안되는 것 같음
                collisionChecked = true;
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Item")
        {
            Items item = col.gameObject.GetComponent<Items>();
            if (item != null)
            {
                PickUpItem(item);

                // 충돌한 아이템을 비활성화하거나 삭제
                Destroy(col.gameObject); // 또는 col.gameObject.SetActive(false);
            }
        }
    }

    private void PickUpItem(Items item)
    {
        if (item.attackID > 0)
        {
            EventManager.TriggerEvent(GlobalEvents.ItemPickedUp);
            attackManager.SetAttack(item.attackID, item.animController);
            attackManager.UpdateBulletCount(item.bulletCount);
        }

        flashBlue.FlashForDuration(0.18f);
        // audioManager.PlaySoundByClip(item.weaponNameAudio);
        SoundManager.Instance.PlayHeavyMachineGunVoice();

        if(!bottomSpriteRenderer.enabled)
        {
            bottomSpriteRenderer.enabled = true;
        }
    }

    public void CanGetItem()
    {
        collisionChecked = false;
    }
}