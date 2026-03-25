using UnityEngine;
using System.Collections.Generic;

public class StorageChest : MonoBehaviour, IInteractable
{
    public static List<StorageChest> allChests = new List<StorageChest>();

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
        if (storageData.Count < slotCount)
        {
            for (int i = storageData.Count; i < slotCount; i++)
            {
                storageData.Add(new StorageSlotData());
            }
        }
    }
    private void Start() => allChests.Add(this);
    private void OnDestroy() => allChests.Remove(this);

    public void Interact()
    {
        StorageUI.Instance.OpenStorage(this);
    }

    // ✨ 특정 아이템이 몇 개 있는지 확인하는 함수
    public int GetItemCount(ItemData targetItem)
    {
        int count = 0;
        foreach (var slot in storageData)
        {
            if (slot.item == targetItem) count += slot.amount;
        }
        return count;
    }

    // ✨ 특정 아이템을 원하는 만큼 빼는 함수
    public void RemoveItem(ItemData targetItem, int amountToRemove)
    {
        for (int i = 0; i < storageData.Count; i++)
        {
            if (storageData[i].item == targetItem)
            {
                if (storageData[i].amount >= amountToRemove)
                {
                    storageData[i].amount -= amountToRemove;
                    if (storageData[i].amount == 0) storageData[i].item = null;
                    break;
                }
                else
                {
                    amountToRemove -= storageData[i].amount;
                    storageData[i].amount = 0;
                    storageData[i].item = null;
                }
            }
        }
    }
}