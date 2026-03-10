using UnityEngine;
using System.Collections.Generic;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    [Header("아이템들이 담길 부모 오브젝트")]
    public Transform itemParent; // 유니티 인스펙터에서 'PickUpItem' 오브젝트를 드래그해서 넣어주세요.

    // 아이템 이름별로 큐(Queue)를 만들어 보관합니다.
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake() => Instance = this;

    public GameObject GetItem(ItemData data, Vector3 position, Quaternion rotation)
    {
        string key = data.itemName;
        if (!poolDictionary.ContainsKey(key))
            poolDictionary.Add(key, new Queue<GameObject>());

        GameObject obj;

        if (poolDictionary[key].Count > 0)
        {
            obj = poolDictionary[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(data.prefab, position, rotation);

            // ✨ 처음 생성될 때 부모를 지정해줍니다.
            if (itemParent != null)
                obj.transform.SetParent(itemParent);
        }

        return obj;
    }

    public void ReturnItem(ItemData data, GameObject obj)
    {
        obj.SetActive(false);
        poolDictionary[data.itemName].Enqueue(obj); // 다시 창고에 넣기
    }
}