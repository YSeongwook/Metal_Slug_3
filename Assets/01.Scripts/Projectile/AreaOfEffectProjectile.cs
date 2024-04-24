using EnumTypes;
using UnityEngine;

public class AreaOfEffectProjectile : MonoBehaviour
{
    [Header("Raycast")]
    public Vector2 boxSize = new Vector2(0.2f, 0.2f);
    public Vector2 boxOffset = new Vector2();
    public float rayDistance = 1f;

    [Space(10)]
    public ProjectileProperties projectileProp;
    public LayerMask layerMask;

    public void CastAOE(string victimsTag, Vector2 pos)
    {
        Vector2 center = new Vector2(pos.x - transform.right.x * (boxSize.x / 2) + boxOffset.x * transform.right.x, pos.y - (boxSize.y / 2) + boxOffset.y);

        float duration = 1.5f;

        Vector2 centre = new Vector2(pos.x + boxOffset.x * transform.right.x, pos.y + boxOffset.y);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(centre, boxSize, 0, transform.right, rayDistance, layerMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.tag == victimsTag || hits[i].collider.gameObject.layer == (int)Layers.Enemy || hits[i].collider.gameObject.layer == (int)Layers.EnemySolid)
            {
                ProjectileUtils.NotifyCollider(hits[i].collider, projectileProp);
            }
        }
    }
}