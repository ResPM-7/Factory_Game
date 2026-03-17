using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour, IInteractable
{
    // [System.Serializable]을 붙여야 인스펙터에서 보이고 저장됩니다.
    [System.Serializable]
    public class StorageSlotData
    {
        public ItemData item;
        public int amount;
    }

    public List<StorageSlotData> storageData = new List<StorageSlotData>();
    public int slotCount = 50; // UI 슬롯 개수와 맞춰주세요.

    void Awake()
    {
        // 리스트가 비어있다면 UI 슬롯 개수(50개)만큼 미리 빈 칸을 채워둡니다.
        if (storageData.Count < 50)
        {
            for (int i = storageData.Count; i < 50; i++)
            {
                storageData.Add(new StorageSlotData());
            }
        }
    }

    public void Interact()
    {
        StorageUI.Instance.OpenStorage(this);
    }
}