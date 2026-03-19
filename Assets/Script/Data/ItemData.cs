using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // 아이템 이름
    public Texture iconTexture;
    public int maxStackSize = 64; // 마인크래프트처럼 최대 중첩 개수 (곡괭이는 1, 나머지는 64)
    public GameObject prefab;    // 바닥에 버리거나 설치할 때 소환될 오브젝트

    [Header("건축(Blueprint) 설정")]
    public bool isBuildable;
    public GameObject placeablePrefab; // 진짜 생성될 건물

    // ✨ [수정됨] 프리팹 대신 하이어라키에 있는 '고스트 오브젝트 이름'을 적습니다.
    // 예: "Box_Ghost", "Workbench_Ghost"
    public string ghostObjectName;

    [TextArea] public string description; // 아이템 설명

}