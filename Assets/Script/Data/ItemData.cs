using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("시스템 데이터 (절대 바꾸지 말 것)")]
    public string itemID; // "Wood", "Rock", "Iron"

    [Header("UI 데이터 (언제든 바꿔도 됨)")]
    public string itemName; // "나무", "조약돌", "철광석"

    public Texture iconTexture;
    public int maxStackSize = 64; // 마인크래프트처럼 최대 중첩 개수 (곡괭이는 1, 나머지는 64)
    public GameObject prefab;    // 바닥에 버리거나 설치할 때 소환될 오브젝트

    [Header("건축(Blueprint) 설정")]
    public bool isBuildable;
    public GameObject placeablePrefab; // 진짜 생성될 건물
    public string ghostObjectName;
    //새로 추가된 기능: 체크하면 무조건 거점 안에만 지을 수 있습니다!
    public bool requireBaseCamp = true;

    [Header("볼설정")]
    public bool isCaptureBall;

    [TextArea] public string description; // 아이템 설명

}