using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item; // 이 오브젝트가 어떤 아이템인지 (Branch, Coal 등)
    public int amount = 1;

    // 마우스로 아이템을 클릭했을 때 호출됩니다.
    private void OnMouseDown()
    {
        Pickup();
    }

    void Pickup()
    {
        // 씬에 있는 InventoryManager를 찾아 아이템 추가를 요청합니다.
        bool isPickedUp = InventoryManager.Instance.AddItem(item, amount);

        if (isPickedUp)
        {
            Debug.Log($"{item.itemName}을(를) 획득했습니다!");
            Destroy(gameObject); // 가방에 넣었으니 월드에서는 삭제
        }
    }
}