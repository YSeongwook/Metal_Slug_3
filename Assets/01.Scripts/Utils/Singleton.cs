using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null) // 만약 인스턴스가 null이라면
            {
                _instance = (T)FindObjectOfType(typeof(T)); // 어딘가에 인스턴스가 있을 수 있으니 인스턴스를 FindObjectOfType으로 탐색

                if (_instance == null) // 없다면, 게임 오브젝트를 생성
                {
                    GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                    _instance = obj.GetComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // DontDestroyOnLoad가 다른 오브젝트의 하위에 있다면 작동 X, 매니저가 부모이거나, 자식일 때 작동
        if (transform.parent != null && transform.root != null)
        {
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject); // 씬이 전환되도 오브젝트가 파괴되지 않는다.
        }
    }
}