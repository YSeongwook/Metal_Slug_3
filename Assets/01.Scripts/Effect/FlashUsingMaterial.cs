using UnityEngine;
using System.Collections;
using Utils;

public class FlashUsingMaterial : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;

    // 기본 Material
    private Material baseMaterial;

    // flash에 사용할 Material
    public Material material;

    public float flashDuration = 1 / 25f;

    void Awake()
    {
        baseMaterial = spriteRenderers[0].material;
    }

    // 3번 flash
    public void FlashThreeTime(RetVoidTakeVoid cb = null)
    {
        if (cb == null)
        {
            cb = () => { };
        }
        StartCoroutine("FlashThreeTimeCoroutine", cb);
    }

    // 지정된 시간 동안 flash
    public void FlashForDuration(float duration)
    {
        StartCoroutine("FlashForDurationCoroutine", duration);
    }

    // 한 프레임 동안 flash
    public void FlashForSingleFrame(RetVoidTakeVoid cb = null)
    {
        if (cb == null)
        {
            cb = () => { };
        }
        StartCoroutine("FlashForSingleFrameCoroutine", cb);
    }

    // Material 초기화
    public void ResetMaterial()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = baseMaterial;
        }
    }

    // 3번 flash 하는 Corutine
    private IEnumerator FlashThreeTimeCoroutine(RetVoidTakeVoid cb = null)
    {
        int numberOfFlashes = 3;
        int flashCount = 0;
        while (flashCount < numberOfFlashes)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].material = material;
            }
            yield return new WaitForSeconds(flashDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].material = baseMaterial;
            }

            yield return new WaitForSeconds(flashDuration);
            flashCount++;
        }
        if (cb != null)
        {
            cb();
        }
    }

    // 지정된 시간 동안 flash 하는 Coroutine
    private IEnumerator FlashForDurationCoroutine(float u)
    {
        float elapsed = 0;
        while (elapsed < u)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].material = material;
            }
            yield return new WaitForSeconds(flashDuration);
            elapsed += flashDuration;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].material = baseMaterial;
            }
            yield return new WaitForSeconds(flashDuration);
            elapsed += flashDuration;
            elapsed += flashDuration;
        }
    }

    // 1 프레임 동안 flash 하는 Coroutine
    private IEnumerator FlashForSingleFrameCoroutine(RetVoidTakeVoid cb = null)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = material;
        }
        yield return new WaitForSeconds(Time.deltaTime);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = baseMaterial;
        }
        if (cb != null)
        {
            cb();
        }
    }
}