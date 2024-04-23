using EnumTypes;
using UnityEngine;

public class BulletProjectile : MonoBehaviour, IProjectile
{
    public ProjectileProperties properties;
    private SpriteRenderer spriteRenderer;
    private bool launched;

    public GameObject impact;
    private Animator impactAnimator;
    private LayerMask colLayer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        impactAnimator = impact.GetComponent<Animator>();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        colLayer = col.gameObject.layer;

        if (col.tag == properties.victimTag || col.tag == "World" || colLayer == (int)Layers.Enemy || colLayer == (int)Layers.EnemySolid)
        {
            ProjectileUtils.RandomizeImpactPosition(transform, impactAnimator.transform);
            impactAnimator.gameObject.SetActive(true);

            if (col.tag == "World")
            {
                impactAnimator.transform.right = transform.right;
                impactAnimator.Play("2");
            }
            else
            {
                if(col.tag == "Building" && colLayer != (int)Layers.EnemySolid) impactAnimator.Play("2");
                else impactAnimator.Play("1");

                ProjectileUtils.NotifyCollider(col, properties);
            }

            // spriteRenderer.enabled = false;
            transform.position = Vector3.zero;

            //impact.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void OnBecameInvisible()
    {
        gameObject.SetActive(false);
        launched = false;
    }

    public void Launch(string victimsTag, Vector2 unusedDestination)
    {
        transform.localPosition = Vector3.zero;
        properties.victimTag = victimsTag;
        //impact.SetActive(true);
        launched = true;
    }

    void Update()
    {
        if (launched) ProjectileUtils.UpdatePositionStraightLine(transform, properties);
    }
}