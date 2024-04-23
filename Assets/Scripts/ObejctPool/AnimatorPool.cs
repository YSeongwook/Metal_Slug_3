using UnityEngine;

public class AnimatorPool : MonoBehaviour
{
    private static ObjectPool objectPool;

    void Awake()
    {
        objectPool = GetComponent<ObjectPool>();
    }

    public static Animator GetPooledAnimator(RuntimeAnimatorController animatorController = null)
    {
        GameObject gameObject = objectPool.GetPooledObject();
        Animator animator = gameObject.GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        return animator;
    }
}