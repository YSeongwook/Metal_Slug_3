using UnityEngine;
using EnumTypes;

public class GrenadeProjectile : MonoBehaviour, IProjectile
{
    public ProjectileProperties properties;
    private int bounceCount;
    public int bouncesToExplode = 2;
    private bool launched;
    private AreaOfEffectProjectile explosionWave;

    public float throwableForce = 6f;

    public LauncherType launcher = LauncherType.Player;
    public ThrowableType throwable = ThrowableType.Grenade;

    private Rigidbody2D rb;

    Vector2 playerDirection;
    Vector3 throwableDirection;

    void Awake()
    {
        explosionWave = GetComponent<AreaOfEffectProjectile>();
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        playerDirection = PlayerController.Instance.LookingDirection;

        if(playerDirection != null)
        {
            // 플레이어가 오른쪽으로 보고 있다면
            if (playerDirection == Vector2.right) 
            {
                throwableDirection = Quaternion.AngleAxis(45, Vector3.forward) * playerDirection;
            }
            // 플레이어가 왼쪽을 보고 있다면
            else
            {
                throwableDirection = Quaternion.AngleAxis(-45, Vector3.forward) * playerDirection;
            }
        }

        if (PlayerController.Instance.IsRunning)
        {
            rb.gravityScale = 3f;
            rb.rotation = 0;
            rb.AddForce(throwableDirection * throwableForce * 1.5f, ForceMode2D.Impulse);
        } 
        else
        {
            rb.gravityScale = 2f;
            rb.rotation = 0;
            rb.AddForce(throwableDirection * throwableForce, ForceMode2D.Impulse);
        }
    }

    public void Launch(string victimsTag, Vector2 destination)
    {
        launched = true;
        properties.victimTag = victimsTag;

        bounceCount = 0;
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        Collider2D collider = col.collider;

        if (collider.tag == properties.victimTag || col.gameObject.layer == (int)Layers.Enemy || col.gameObject.layer == (int)Layers.EnemySolid)
        {
            Explode(collider);
        }
        else if(collider.tag == "World" || collider.tag == "Walkable")
        {
            Bounce();
            bounceCount++;

            // Explode(collider);
            if (bounceCount >= bouncesToExplode) Explode(collider);
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == properties.victimTag || col.gameObject.layer == (int)Layers.Enemy || col.gameObject.layer == (int)Layers.EnemySolid)
        {
            Explode(col);
        }
        else
        {
            bounceCount++;

            // Explode(col);
            if (bounceCount >= bouncesToExplode) Explode(col);
        }
    }

    private void Bounce()
    {
        rb.gravityScale = 2f;
        rb.rotation = 0;
        rb.AddForce(throwableDirection * throwableForce, ForceMode2D.Impulse);
    }

    private void Explode(Collider2D col)
    {
        ProjectileUtils.ImpactAnimationAndSound(transform, col, properties);
        ProjectileUtils.NotifyCollider(col, properties);
        explosionWave.CastAOE("Enemy", transform.position);
        gameObject.SetActive(false);
    }

    void OnBecameInvisible()
    {
        gameObject.SetActive(false);
        launched = false;
    }
}