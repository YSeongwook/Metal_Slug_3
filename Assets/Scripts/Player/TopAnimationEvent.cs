using UnityEngine;

public class TopAnimationEvent : MonoBehaviour
{

    public AnimationManager animManager;

    public void EndOfDeathAnim()
    {
        animManager.EndOfDeathAnim();
    }

    public void AEGrenadeOut()
    {
        animManager.grenadeCB();
    }
}
