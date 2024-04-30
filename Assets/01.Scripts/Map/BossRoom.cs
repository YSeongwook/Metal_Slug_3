using UnityEngine;

public class BossRoom : MonoBehaviour
{
    private void OnEnable()
    {
        // 생성된 순간에 모든 자식오브젝트 찾아서 활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
