using UnityEngine;
using System.Collections.Generic;

public class StorageUI : MonoBehaviour, IUIPanel
{
    public static StorageUI Instance;
    public GameObject uiPanel;
    public InventorySlot[] storageSlots;
    private Storage _currentStorage;

    void Awake() => Instance = this;

    void Start()
    {
        // ✨ 게임 시작 시 상자 패널을 확실하게 끕니다.
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

        _currentStorage = null;
    }

    public void OpenStorage(Storage storage)
    {
        _currentStorage = storage;
        uiPanel.SetActive(true);

        for (int i = 0; i < storageSlots.Length; i++)
        {
            if (i < storage.storageData.Count)
            {
                storageSlots[i].item = storage.storageData[i].item;
                storageSlots[i].amount = storage.storageData[i].amount;
                storageSlots[i].UpdateSlotUI();
            }
        }

        // 인벤토리도 동시에 열기
        InventoryManager.Instance.OpenInventoryForce();
        InventoryManager.Instance.SetPlayerControl(false); // 조작 차단
    }

    public void CloseStorage()
    {
        if (_currentStorage == null) return;

        // UI -> 실제 상자로 데이터 복사(저장)
        for (int i = 0; i < storageSlots.Length; i++)
        {
            if (i < _currentStorage.storageData.Count)
            {
                _currentStorage.storageData[i].item = storageSlots[i].item;
                _currentStorage.storageData[i].amount = storageSlots[i].amount;
            }
        }
        uiPanel.SetActive(false);
        _currentStorage = null;
    }

    public void CloseUI()
    {
        CloseStorage();
    }
}