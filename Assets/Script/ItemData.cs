using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // 아이템 이름
    public Sprite icon;          // 인벤토리 슬롯에 보일 이미지
    public GameObject prefab;    // 건축할 때 실제로 소환될 오브젝트
    [TextArea] public string description; // 아이템 설명
}