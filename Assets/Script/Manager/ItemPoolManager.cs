using UnityEngine;
using System.Collections.Generic;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    [Header("아이템들이 담길 최상위 부모 오브젝트")]
    public Transform itemParent; // 유니티 인스펙터에서 'PickUpItem' 드래그 앤 드롭!

    // 아이템 이름별 보관 창고 (Queue)
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // Wood, Rock 같은 하위 폴더 위치를 기억해둘 딕셔너리
    private Dictionary<string, Transform> folderDictionary = new Dictionary<string, Transform>();

    void Awake() => Instance = this;

    // 🌟 나무/돌을 캐서 바닥에 떨어뜨릴 때 (GetItem)
    public GameObject GetItem(ItemData data, Vector3 position, Quaternion rotation)
    {
        string key = data.itemID;

        // 1. 창고에 이 아이템(예: Wood)을 위한 큐와 폴더가 없다면 최초 1회 생성합니다.
        if (!poolDictionary.ContainsKey(key))
        {
            SetupNewItemType(key);
        }

        // 2. 창고에 비활성화된(주워간) 아이템이 있다면 꺼내 씁니다. (♻️ 재활용)
        if (poolDictionary[key].Count > 0)
        {
            GameObject reusedObj = poolDictionary[key].Dequeue();
            reusedObj.transform.position = position;
            reusedObj.transform.rotation = rotation;
            reusedObj.SetActive(true);
            return reusedObj;
        }
        // 3. 창고가 비어있다면 새로 만들어서 알맞은 폴더(Wood, Rock 등)에 넣습니다.
        else
        {
            GameObject newObj = Instantiate(data.prefab, position, rotation);
            newObj.name = key; // (Clone) 글자 제거

            if (folderDictionary.ContainsKey(key))
                newObj.transform.SetParent(folderDictionary[key]); // 전용 폴더 안에 쏙 넣기

            return newObj;
        }
    }

    // 플레이어가 아이템을 주웠을 때 (ReturnItem)
    public void ReturnItem(ItemData data, GameObject obj)
    {
        string key = data.itemID;

        // 안전장치: 에디터에 수작업으로 깔아둔 아이템을 주웠을 때 에러 방지!
        if (!poolDictionary.ContainsKey(key))
        {
            SetupNewItemType(key);
        }

        // 아이템을 주우면 해당 폴더(예: Wood) 아래로 위치를 옮기고 불을 끕니다.
        if (folderDictionary.ContainsKey(key))
            obj.transform.SetParent(folderDictionary[key]);

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj); // 큐에 넣어서 대기 상태로 만듦
    }

    // --- 폴더와 창고를 세팅해주는 내부 편의용 함수 ---
    private void SetupNewItemType(string key)
    {
        poolDictionary.Add(key, new Queue<GameObject>());

        Transform existingFolder = null;
        if (itemParent != null)
            existingFolder = itemParent.Find(key);

        // 하이어라키에 폴더가 없으면 알아서 만들어줍니다.
        if (existingFolder == null)
        {
            GameObject newFolder = new GameObject(key);
            if (itemParent != null) newFolder.transform.SetParent(itemParent);
            existingFolder = newFolder.transform;
        }

        folderDictionary[key] = existingFolder;
    }

    public GameObject GetBuilding(ItemData data, Vector3 position, Quaternion rotation)
    {
        // 드롭 아이템과 섞이지 않게 폴더 이름 뒤에 "_Building"을 붙여서 깔끔하게 정리합니다!
        string key = data.itemID + "_Building";

        if (!poolDictionary.ContainsKey(key))
        {
            SetupNewItemType(key);
        }

        if (poolDictionary[key].Count > 0)
        {
            GameObject reusedObj = poolDictionary[key].Dequeue();
            reusedObj.transform.position = position;
            reusedObj.transform.rotation = rotation;
            reusedObj.SetActive(true);
            return reusedObj;
        }
        else
        {
            // 💡 핵심: data.prefab이 아니라 건축용인 data.placeablePrefab을 생성합니다!
            GameObject newObj = Instantiate(data.placeablePrefab, position, rotation);
            newObj.name = key; // (Clone) 글자 제거

            if (folderDictionary.ContainsKey(key))
                newObj.transform.SetParent(folderDictionary[key]);

            return newObj;
        }
    }

    // 🌟 2. 나중에 건축물을 부수고 회수할 때 부르는 함수 (ReturnBuilding)
    public void ReturnBuilding(ItemData data, GameObject obj)
    {
        string key = data.itemID + "_Building";

        if (!poolDictionary.ContainsKey(key))
        {
            SetupNewItemType(key);
        }

        if (folderDictionary.ContainsKey(key))
            obj.transform.SetParent(folderDictionary[key]);

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }
}