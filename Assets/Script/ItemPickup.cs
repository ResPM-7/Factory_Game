using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item; // 이 오브젝트가 어떤 아이템인지 (Branch, Coal 등)
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            Pickup();
    }

    void Pickup()
    {
        // 씬에 있는 InventoryManager를 찾아 아이템 추가를 요청합니다.
        bool isPickedUp = InventoryManager.Instance.AddItem(item, amount);

        if (isPickedUp)
        {
            Debug.Log($"{item.itemName}을(를) 획득했습니다!");
            ItemPoolManager.Instance.ReturnItem(item, gameObject);
        }
    }
}