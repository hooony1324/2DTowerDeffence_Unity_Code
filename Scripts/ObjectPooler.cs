using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ObjectPooler))]
public class ObjectPoolEditor : Editor
{
    const string INFO =
        "풀링할 오브젝트는 다음의 코드를 포함해야한다\n" +
        "void OnDisable()\n" +
        "{\n " +
        "ObjectPooler.ReturnToPool(gameObject); \n" +
        "CancelInvoke(); \n" +
        "}";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(INFO, MessageType.Info);
        base.OnInspectorGUI();
    }
}

#endif

public class ObjectPooler : Singleton<ObjectPooler>
{
    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] Pool[] pools;
    List<GameObject> spawnObjects;
    Dictionary<string, Queue<GameObject>> poolDictionary;
    readonly string INFO =
        "풀링할 오브젝트는 다음의 코드를 포함해야한다\n" +
        "void OnDisable()\n" +
        "{\n " +
        "ObjectPooler.ReturnToPool(gameObject); \n" +
        "CancelInvoke(); \n" +
        "}";

    private void Awake()
    {
        spawnObjects = new List<GameObject>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            poolDictionary.Add(pool.tag, new Queue<GameObject>());
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreateNewObject(pool.tag, pool.prefab);
                ArrangePool(obj);
            }

            // OnDisable(), ReturnToPool() 구현여부 및 중복여부 검사
            if (poolDictionary[pool.tag].Count <= 0)
            {
                Debug.LogError($"{pool.tag}{INFO}");
            }
            else if (poolDictionary[pool.tag].Count != pool.size)
            {
                Debug.LogError($"{pool.tag}에 ReturnToPool()중복발생"); 
            }
        }
    }

    public static GameObject SpawnFromPool(string tag, Vector3 position) 
    {
        return Instance._SpawnFromPool(tag, position, Quaternion.identity);
    }

    public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        return Instance._SpawnFromPool(tag, position, rotation);
    }

    // where T : Component (컴포넌트만 리턴 허용)
    public static T SpawnFromPool<T>(string tag, Vector3 position) where T : Component
    {
        GameObject obj = Instance._SpawnFromPool(tag, position, Quaternion.identity);
        if (obj.TryGetComponent(out T component))
        {
            return component;
        }
        else
        {
            obj.SetActive(false);
            throw new Exception("Component not found");
        }
    }

    public static T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = Instance._SpawnFromPool(tag, position, rotation);
        if (obj.TryGetComponent(out T component))
        {
            return component;
        }
        else
        {
            obj.SetActive(false);
            throw new Exception("Component not found");
        }
    }

    private GameObject _SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // Pooler에 등록하지 않았으면 오류
        if (!poolDictionary.ContainsKey(tag))
        {
            throw new Exception($"Pool wit tag {tag} doesn't exist");
        }

        // 큐에 없으면 추가
        Queue<GameObject> poolQueue = poolDictionary[tag];
        if (poolQueue.Count <= 0)
        {
            Pool pool = Array.Find(pools, x => x.tag == tag);
            GameObject obj = CreateNewObject(pool.tag, pool.prefab);
            ArrangePool(obj);
        }

        // 큐에서 꺼내고 사용
        GameObject objectToSpawn = poolQueue.Dequeue();
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public static List<GameObject> GetAllPools(string tag)
    {
        if (!Instance.poolDictionary.ContainsKey(tag))
        {
            throw new Exception($"Pool with tag {tag} doesn't exist");
        }

        return Instance.spawnObjects.FindAll(x => x.tag == tag);
    }

    public static List<T> GetAllPools<T>(string tag) where T : Component
    {
        List<GameObject> objects = GetAllPools(tag);

        if (!objects[0].TryGetComponent(out T component))
        {
            throw new Exception("Component not found");
        }

        return objects.ConvertAll(x => x.GetComponent<T>());
    }

    public static void ReturnToPool(GameObject obj)
    {
        if (!Instance.poolDictionary.ContainsKey(obj.name))
        {
            throw new Exception($"Pool with tag {obj.name} doesn't exist");
        }

        Instance.poolDictionary[obj.name].Enqueue(obj);
    }

    [ContextMenu("GetSpawnObjectsInfo")]
    void GetSpawnObjectsInfo()
    {
        foreach (Pool pool in pools)
        {
            int count = spawnObjects.FindAll(x => x.name == pool.tag).Count;
            Debug.Log($"{pool.tag} count : {count}");
        }
    }

    private GameObject CreateNewObject(string tag, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = tag;
        obj.SetActive(false); // 비활성화 > ReturnToPool(Pool에 Enqueue됨)
        return obj;
    }

    // 순서에 맞게 새로운 오브젝트 삽입
    private void ArrangePool(GameObject obj)
    {
        bool isFind = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            // 마지막 Child까지 가도 없으면 최하단에 삽입
            if (i == transform.childCount - 1)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }
            // 자식들 중에서 풀링된 다른 종류의 obj로 바뀌는 지점 찾음
            else if (transform.GetChild(i).name == obj.name)
            {
                isFind = true;
            }
            // 바뀌는 지점 찾았으면
            else if (isFind)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }
        }
    }

}
