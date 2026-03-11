using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour
{
    public string storageName = "철제 상자";
    public int size = 50; // 50칸 상자
    public List<InventorySlotData> storageData = new List<InventorySlotData>();

    void Awake()
    {
        // 50개의 빈 데이터를 미리 채워둡니다.
        for (int i = 0; i < size; i++)
        {
            storageData.Add(new InventorySlotData());
        }
    }

    public void Interact()
    {
        StorageUI.Instance.OpenStorage(this);
    }
}