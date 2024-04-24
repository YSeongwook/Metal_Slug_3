using System.Collections;
using UnityEngine;

public class SunkBoatController : MonoBehaviour
{
    public Animator doorAnimator;
    public Animator[] normalExplosion;

    void OnFinish()
    {
        StartCoroutine(WaitExplosion());
    }

    public void OpenDoor()
    {
        doorAnimator.SetTrigger("open");

        GetComponent<EventSpawn>().onFinish += OnFinish;
        GetComponent<EventSpawn>().Trigger();
    }

    private IEnumerator WaitExplosion()
    {
        yield return new WaitForSeconds(1f);
        foreach (Animator anim in normalExplosion)
        {
            anim.SetBool("isExploding", true);
        }
        // explosion.SetBool("isExploding", true);
        SoundManager.Instance.PlayMetalSlugDestroy2();
        yield return new WaitForSeconds(1.6f);
        this.gameObject.SetActive(false);
        CameraManager.Instance.AfterSunkBoat();
    }
}
