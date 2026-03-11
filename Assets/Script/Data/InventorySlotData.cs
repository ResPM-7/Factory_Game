using UnityEngine;

[System.Serializable]
public class InventorySlotData // 상자의 '알맹이' 데이터만 담는 클래스
{
    public ItemData item;
    public int amount;

    public InventorySlotData(ItemData newItem = null, int newAmount = 0)
    {
        item = newItem;
        amount = newAmount;
    }
}