using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine("ActivateAndDeactivateSpawnIndicator", 5f);
    }

    private IEnumerator ActivateAndDeactivateSpawnIndicator(float duration)
    {
        gameObject.SetActive(true); // spawnIndicator 활성화
        yield return new WaitForSeconds(duration); // 일정 시간 대기
        gameObject.SetActive(false); // 일정 시간이 지난 후 spawnIndicator 비활성화
    }
}
