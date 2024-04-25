using System.Collections;
using UnityEngine;
using EnumTypes;

public class ThrowableMovement : MonoBehaviour
{
    [Header("Throwable Details")]
    private float throwableDamagePlayer = 300f;
    private float throwableDamageEnemy = 10f;
    private float throwableDamageBoss = 25f;
    private float throwableDamageHeavybomb = 50f;
    private float throwableDamageVomit = 25f;
    public float throwableForce = 2.5f;

    public LauncherType launcher = LauncherType.Enemy;
    public ThrowableType throwable = ThrowableType.BossBomb;
    public bool canExplode = true;

    private Animator throwableAnimator;
    private Rigidbody2D rb;

    Vector3 throwableDirection;

    private bool hasHit;
    private bool isSpawned;

    private void Start()
    {
        throwableAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        switch (rb.rotation)
        {
            case 0:
                throwableDirection = Quaternion.AngleAxis(45, Vector3.forward) * Vector3.right;
                break;
            case 180:
                throwableDirection = Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.left;
                break;
            case -90:
                throwableDirection = Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.left;
                break;
            case 90:
                throwableDirection = Quaternion.AngleAxis(45, Vector3.forward) * Vector3.right;
                break;
        }

        rb.gravityScale = .5f;
        rb.rotation = 0;
        rb.AddForce(throwableDirection * throwableForce, ForceMode2D.Impulse);
        hasHit = false;
        isSpawned = true;
    }

    private void Despawn()
    {
        if (!isSpawned) return;
        isSpawned = false;

        if (throwable == ThrowableType.Grenade) //Is a Grenade
        {
            // BulletManager.GetGrenadePool()?.Despawn(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Destroy the bulled when out of camera
    private void OnBecameInvisible()
    {
        Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasHit) return;

        if (GameManager.Instance.CanTriggerThrowable(collider) && !(launcher == LauncherType.Player && GameManager.Instance.IsPlayer(collider)) && !(launcher == LauncherType.Enemy && (collider.CompareTag("Enemy") || collider.CompareTag("EnemyBomb"))))
        {
            hasHit = true;

            if (canExplode)
            {
                if (throwable == ThrowableType.BossHeavyBomb)
                {
                    if (collider.CompareTag("Walkable"))
                    {
                        GameObject hittenTerrain = collider.gameObject;
                        StartCoroutine(DestroyHitten(hittenTerrain));
                    }
                }
                StartCoroutine(Explosion(collider));
            }
            else
            {
                ResetMovement(collider);
                Despawn();
            }
        }
    }

    private IEnumerator Explosion(Collider2D collision)
    {
        SoundManager.Instance.PlayGrenadeHitAudio();
        throwableAnimator.SetBool("hasHittenSth", true);

        ResetMovement(collision);

        yield return new WaitForSeconds(1.7f);
        throwableAnimator.SetBool("hasHittenSth", false);
        Despawn();
    }

    private void ResetMovement(Collider2D collider)
    {
        var target = collider.gameObject;
        if (GameManager.Instance.IsPlayer(collider)) target = GameManager.Instance.GetPlayer(collider);

        rb.angularVelocity = 0;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    private IEnumerator DestroyHitten(GameObject hittenTerrain)
    {
        yield return new WaitForSeconds(0.25f);
        hittenTerrain.GetComponent<Collider2D>().enabled = false;
        hittenTerrain.GetComponent<Animator>().SetBool("onDestroy", true);
        yield return new WaitForSeconds(1.2f);
        hittenTerrain.GetComponent<Animator>().SetBool("onDestroy", false);
        Destroy(hittenTerrain);
    }
}
