using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("드래그 전용 UI")]
    public GameObject dragObject;
    public RawImage dragIcon;
    public Text dragAmountText;

    [Header("모든 슬롯 리스트")]
    public InventorySlot[] allSlots; // 여기에 30개 슬롯이 연결되어야 합니다.

    [Header("UI 오브젝트")]
    public GameObject inventoryUI;

    // ✨ 에러의 주범! 사라졌던 필수 변수들입니다.
    private StarterAssetsInputs _playerInputs;
    private PlayerInput _playerInputComponent;
    private CinemachineInputAxisController _cameraInputController;
    private bool isInventoryOpen = false;

    void Awake() => Instance = this;

    void Start()
    {
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _playerInputComponent = Object.FindAnyObjectByType<PlayerInput>();
        _cameraInputController = Object.FindAnyObjectByType<CinemachineInputAxisController>();

        // 초기 상태 설정
        isInventoryOpen = false;
        if (inventoryUI != null) inventoryUI.SetActive(false);
        SetCursorState(false);
    }

    void Update()
    {
        // 1. 현재 Action Map에서 "Inventory" 키(I) 입력을 찾습니다.
        var invAction = _playerInputComponent.actions.FindAction("Inventory");

        if (invAction != null && invAction.WasPressedThisFrame())
        {
            // 2. 현재 어떤 UI든 하나라도 열려 있는지 체크합니다.
            bool isAnyUIOpen = isInventoryOpen ||
                               (StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf) ||
                               (CraftingUI.Instance != null && CraftingUI.Instance.uiPanel.activeSelf);

            if (isAnyUIOpen)
            {
                // 3. ✨ [닫기] 이미 창이 열려 있다면 UIManager를 시켜서 싹 닫습니다.
                // 인벤토리뿐만 아니라 상자나 제작창이 열려 있어도 'I'를 누르면 닫힙니다.
                UIManager.Instance.CloseAllMenus();
                Debug.Log("I 키로 모든 메뉴 닫기");
            }
            else
            {
                // 4. [열기] 아무것도 안 열려 있다면 인벤토리를 엽니다.
                OpenInventory();
                Debug.Log("I 키로 인벤토리 열기");
            }
        }
    }

    // OpenInventory 함수가 없다면 이것도 추가해주세요.
    public void OpenInventory()
    {
        isInventoryOpen = true;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            UpdateInputState(); // 마우스 커서 활성화 및 조작 차단
        }
    }

    public void SetInventoryState(bool isOpen) => isInventoryOpen = isOpen;

    // ✨ [조작 모드 전환]
    public void UpdateInputState()
    {
        bool isStorageOpen = StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf;
        bool isCraftingOpen = CraftingUI.Instance != null && CraftingUI.Instance.uiPanel.activeSelf;

        bool anyUIOpen = isInventoryOpen || isStorageOpen || isCraftingOpen;

        if (anyUIOpen)
        {
            _playerInputComponent.SwitchCurrentActionMap("UI");
            SetCursorState(true);
            if (_cameraInputController != null) _cameraInputController.enabled = false;
        }
        else
        {
            _playerInputComponent.SwitchCurrentActionMap("Player");
            SetCursorState(false);
            if (_cameraInputController != null) _cameraInputController.enabled = true;
        }
    }

    // ✨ [마우스 상태 제어]
    private void SetCursorState(bool isOpen)
    {
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    // ✨ [아이템 추가]
    public bool AddItem(ItemData data, int amount)
    {
        int remainingAmount = amount;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == data && slot.amount < data.maxStackSize)
            {
                int canAdd = data.maxStackSize - slot.amount;
                int amountToAdd = Mathf.Min(canAdd, remainingAmount);
                slot.amount += amountToAdd;
                remainingAmount -= amountToAdd;
                slot.UpdateSlotUI();
                if (remainingAmount <= 0) return true;
            }
        }
        if (remainingAmount > 0)
        {
            foreach (InventorySlot slot in allSlots)
            {
                if (slot.item == null)
                {
                    int amountToAdd = Mathf.Min(data.maxStackSize, remainingAmount);
                    slot.item = data;
                    slot.amount = amountToAdd;
                    remainingAmount -= amountToAdd;
                    slot.UpdateSlotUI();
                    if (remainingAmount <= 0) return true;
                }
            }
        }
        return remainingAmount <= 0;
    }

    // ✨ StorageUI나 UIManager가 호출할 수 있도록 함수를 다시 만들어줍니다.
    public void SetPlayerControl(bool isEnable)
    {
        // 이미 만들어둔 똑똑한 함수(UpdateInputState)를 대신 실행시킵니다.
        UpdateInputState();
    }

    // ✨ [아이템 개수 확인] 제작용
    public int GetItemCount(ItemData data)
    {
        int total = 0;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == data) total += slot.amount;
        }
        return total;
    }

    // ✨ [아이템 소모] 제작용
    public void RemoveItem(ItemData data, int amount)
    {
        int remainingToRemove = amount;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == data)
            {
                if (slot.amount >= remainingToRemove)
                {
                    slot.amount -= remainingToRemove;
                    remainingToRemove = 0;
                }
                else
                {
                    remainingToRemove -= slot.amount;
                    slot.amount = 0;
                }
                if (slot.amount <= 0) slot.ClearSlot();
                else slot.UpdateSlotUI();

                if (remainingToRemove <= 0) break;
            }
        }
    }

    public void OpenInventoryForce()
    {
        isInventoryOpen = true;
        inventoryUI.SetActive(true);
        UpdateInputState();
    }
}