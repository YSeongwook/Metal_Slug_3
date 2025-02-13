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
        if (_instance == null)
        {
            _instance = this as T;
            // 씬 전환 시 파괴되지 않도록 설정
            if (transform.parent != null && transform.root != null)
            {
                DontDestroyOnLoad(this.transform.root.gameObject);
            }
            else
            {
                DontDestroyOnLoad(this.gameObject);
            }
            Debug.Log("싱글톤 인스턴스 초기화 완료");
        }
        else if (_instance != this)
        {
            Debug.Log("중복된 싱글톤 인스턴스 감지되어 제거합니다.");
            Destroy(this.gameObject);
        }
    }
    
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.Log("싱글톤 인스턴스가 파괴됩니다.");
            _instance = null;
        }
    }
}