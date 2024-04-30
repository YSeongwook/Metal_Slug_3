using UnityEngine;

public class TriggerTarget : MonoBehaviour
{
    bool isActive = true;
    Transform target;
    public GameObject prefabRoom;

    void Start()
    {
        target = GameManager.Instance.GetRunningTarget().transform;
        if (!target) isActive = false;
    }

    void Update()
    {
        if (isActive)
        {
            if (Mathf.Abs(transform.position.x - target.position.x) < 6.64f)
            {
                GameObject room = Instantiate(prefabRoom, transform.position, transform.rotation);
                isActive = false;
                prefabRoom = null; // 생성 후 프리팹을 null로 설정하여 다시 복제되지 않도록 함
            }
        }
    }
}
