using UnityEngine;
using System.Collections.Generic;

public class StorageUI : MonoBehaviour
{
    public static StorageUI Instance;
    public GameObject uiPanel; // ChestInven 오브젝트
    public InventorySlot[] storageSlots; // UI에 있는 50개 슬롯

    private Storage _currentStorage;

    void Awake() => Instance = this;

    void Start()
    {
        // ✨ 게임 시작 시 UI창을 비활성화합니다.
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    // 상자 열기
    public void OpenStorage(Storage storage)
    {
        _currentStorage = storage;
        uiPanel.SetActive(true);

        // 데이터 동기화
        for (int i = 0; i < storageSlots.Length; i++)
        {
            if (i < storage.storageData.Count)
            {
                storageSlots[i].item = storage.storageData[i].item;
                storageSlots[i].amount = storage.storageData[i].amount;
                storageSlots[i].UpdateSlotUI();
            }
        }
        // 상자 열 때 플레이어 인벤토리 UI도 같이 켜주기
        if (!InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.ToggleInventory();
        }
    }

    // 상자 닫기
    public void CloseStorage()
    {
        if (_currentStorage == null) return;

        // UI -> 실제 데이터 저장
        for (int i = 0; i < storageSlots.Length; i++)
        {
            _currentStorage.storageData[i].item = storageSlots[i].item;
            _currentStorage.storageData[i].amount = storageSlots[i].amount;
        }

        uiPanel.SetActive(false);
        _currentStorage = null;

        // ✨ 중요: 상자를 닫을 때 다시 플레이어 조작을 활성화하고 커서를 가립니다.
        if (InventoryManager.Instance != null)
        {
            // 인벤토리 매니저의 상태를 '닫힘'으로 강제 호출하여 조작을 복구합니다.
            InventoryManager.Instance.ToggleInventory();
        }
    }
}