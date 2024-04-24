using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkText : MonoBehaviour
{
    private Image image;
    public float blinkInterval = 1f; // 깜빡임 간격(초)

    void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);

            // 이미지의 투명도를 변경하여 깜빡이는 효과를 구현
            image.enabled = !image.enabled;
        }
    }
}
