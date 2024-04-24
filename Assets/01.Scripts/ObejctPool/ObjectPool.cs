using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ObjectPool : MonoBehaviour
{
    public GameObject pooledObject;

    public int pooledAmount = 100;

    // 풀이 꽉 찼을 때 새로운 오브젝트 생성 여부
    public bool willGrow = false;

    // 생성된 오브젝트들을 담을 리스트
    public List<GameObject> pooledObjects;

    void Awake()
    {
        pooledObjects = new List<GameObject>();

        if (pooledObject == null)
        {
            return;
        }

        // 초기 생성할 오브젝트들을 생성하고 리스트에 추가
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Instantiate(pooledObject); // 게임 오브젝트 생성
            obj.transform.parent = this.transform;      // 풀 오브젝트의 자식으로 설정
            obj.SetActive(false);                       // 초기에 비활성화 상태로 설정
            pooledObjects.Add(obj);                     // 생성된 오브젝트를 리스트에 추가
        }
    }

    // 비활성화된 오브젝트 가져오기
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            // 오브젝트가 없다면 새로 생성해서 리스트에 추가하고 반환
            if (pooledObjects[i] == null)
            {
                GameObject obj = Instantiate(pooledObject);
                obj.transform.parent = this.transform;
                obj.SetActive(false);
                pooledObjects[i] = obj;
                return pooledObjects[i];
            }

            // 오브젝트가 비활성화 상태라면 해당 오브젝트를 반환
            if (!pooledObjects[i].activeInHierarchy)
            {
                pooledObjects[i].SetActive(true);
                return pooledObjects[i];
            }
        }

        // 풀이 꽉 찼고, 새로 생성할 수 있도록 설정되어 있다면 새 오브젝트 생성 후 반환
        if (willGrow)
        {
            GameObject obj = Instantiate(pooledObject);
            pooledObjects.Add(obj);
            obj.transform.parent = this.transform;
            return obj;
        }

        // 꽉 찼고, 새로 생성할 수 없도록 설정되어 있다면 null 반환
        Debug.Log("탄창이 비었습니다.");
        return null;
    }
}