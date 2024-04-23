using UnityEngine;

public class KnifeAnimationEvent : MonoBehaviour
{
    private Animator topAnimator;

    void Awake()
    {
        topAnimator = GetComponent<Animator>();
    }

    public void AEEndOfKnifeAnim()
    {
        topAnimator.SetBool("knifing", false);
    }

    public void AEStartOfKnifeAnim()
    {
        topAnimator.SetBool("knifing", true);
    }
}
