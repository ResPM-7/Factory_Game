using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IUIPanel
{
    public static InventoryManager Instance;

    [Header("모든 슬롯 리스트")]
    public InventorySlot[] allSlots;

    [Header("UI 오브젝트")]
    public GameObject inventoryUI;

    [Header("마우스 따라다니는 UI")]
    public GameObject dragObject;
    public RawImage dragIcon;
    public Text dragAmountText;

    private StarterAssetsInputs _playerInputs;
    private PlayerInput _playerInputComponent;
    private CinemachineInputAxisController _cameraInputController;
    private bool isInventoryOpen = false;

    // ✨ 클릭 이동을 위한 핵심 변수: 현재 선택된(들고 있는) 슬롯
    private InventorySlot _selectedSlot = null;

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
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            SetInventoryState(!inventoryUI.activeSelf);
            if (UIManager.Instance != null) UIManager.Instance.UpdateInputState();
        }

        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            ToggleCraftingUI();
        }

        // ✨ [추가된 부분] 아이템을 들고(선택하고) 있을 때 G 키를 누르면 바닥에 버립니다.
        if (_selectedSlot != null && Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropSelectedItem();
        }

        if (_selectedSlot != null && dragObject != null && dragObject.activeSelf)
        {
            // New Input System의 마우스 위치 값을 가져옵니다.
            dragObject.transform.position = Mouse.current.position.ReadValue();
        }
    }

    private void DropSelectedItem()
    {
        if (_selectedSlot == null || _selectedSlot.item == null || _selectedSlot.item.prefab == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        ItemData itemToDrop = _selectedSlot.item;
        int amountToDrop = _selectedSlot.amount;
        Transform playerTransform = player.transform;

        Vector3 dropPosition = playerTransform.position + (playerTransform.forward * 1.2f) + (Vector3.up * 1.0f);

        if (Physics.Raycast(dropPosition, Vector3.down, out RaycastHit hit, 5f))
        {
            dropPosition.y = hit.point.y + 0.1f;
        }

        GameObject droppedItem = ItemPoolManager.Instance.GetItem(itemToDrop, dropPosition, Quaternion.identity);

        if (droppedItem.TryGetComponent(out ItemPickup pickupScript))
        {
            pickupScript.item = itemToDrop;
            pickupScript.amount = amountToDrop;
        }

        if (droppedItem.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(playerTransform.forward * 1.0f, ForceMode.Impulse);
        }

        Debug.Log($"{itemToDrop.itemName}을(를) 플레이어 앞에 버렸습니다.");

        // ✨ 버린 후 슬롯을 비우고 선택 상태를 해제합니다.
        _selectedSlot.ClearSlot();
        ClearSelection();
    }

    private void ToggleCraftingUI()
    {
        if (CraftingUI.Instance == null) return;

        // 현재 제작창이 켜져 있는지 확인
        bool isCurrentlyOpen = CraftingUI.Instance.uiPanel.activeSelf;

        if (isCurrentlyOpen)
        {
            // 켜져 있다면? UIManager를 통해 안전하게 닫기 (조작 복구 포함)
            UIManager.Instance.CloseAllMenus();
        }
        else
        {
            // 꺼져 있다면? 제작창 열기
            CraftingUI.Instance.OpenCraftingUI();
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

    public void OnSlotClicked(InventorySlot clickedSlot)
    {
        // 1. 아무것도 안 들고 있을 때 (집어들기)
        if (_selectedSlot == null)
        {
            if (clickedSlot.item == null) return;

            _selectedSlot = clickedSlot;
            _selectedSlot.HighlightSlot(true);

            if (dragObject != null)
            {
                dragObject.SetActive(true);

                // 마우스 아이콘 이미지 세팅 및 불투명하게 만들기
                if (dragIcon != null && _selectedSlot.item.iconTexture != null)
                {
                    dragIcon.texture = _selectedSlot.item.iconTexture;
                    dragIcon.enabled = true;

                    Color dragColor = dragIcon.color;
                    dragColor.a = 1.0f;
                    dragIcon.color = dragColor;
                }

                // 텍스트 활성화 및 개수 표시
                if (dragAmountText != null)
                {
                    dragAmountText.enabled = true;
                    dragAmountText.text = _selectedSlot.amount > 1 ? _selectedSlot.amount.ToString() : "";
                }

                // ✨ [수정 1] 인벤토리 슬롯에 있는 원래 그림과 텍스트를 "완전히 안 보이게" 끕니다.
                if (_selectedSlot.iconRawImage != null)
                {
                    _selectedSlot.iconRawImage.enabled = false;
                }
                if (_selectedSlot.stackText != null)
                {
                    _selectedSlot.stackText.enabled = false;
                }
            }
        }
        // 2. 아이템을 들고 있을 때 (내려놓기 또는 합치기)
        else
        {
            if (_selectedSlot == clickedSlot)
            {
                ClearSelection();
            }
            else
            {
                MoveOrMergeItem(_selectedSlot, clickedSlot);
                ClearSelection();
            }
        }
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null)
        {
            // ✨ [핵심 추가 2] 중요! 아이템을 내려놓거나 병합했으니,
            // 원래 슬롯의 UI를 다시 켜줍니다. (남은 개수만큼 다시 그려짐)
            _selectedSlot.UpdateSlotUI();

            _selectedSlot.HighlightSlot(false); // 테두리 끄기
            _selectedSlot = null;
        }

        // 마우스 아이콘 끄기
        if (dragObject != null)
        {
            dragObject.SetActive(false);
        }
    }

    // ✨ [추가] 만능 아이템 이동/병합/스왑 함수
    private void MoveOrMergeItem(InventorySlot sourceSlot, InventorySlot targetSlot)
    {
        if (sourceSlot.item == null) return;

        // 1. 도착지가 [빈 슬롯]인 경우 -> 그대로 이동
        if (targetSlot.item == null)
        {
            targetSlot.item = sourceSlot.item;
            targetSlot.amount = sourceSlot.amount;
            sourceSlot.ClearSlot(); // 원래 자리는 비워줌
        }
        // 2. 도착지에 [같은 아이템]이 있는 경우 -> 병합(Merge)
        else if (targetSlot.item == sourceSlot.item)
        {
            int maxStack = targetSlot.item.maxStackSize;
            int spaceLeft = maxStack - targetSlot.amount;

            if (spaceLeft > 0)
            {
                int amountToMove = Mathf.Min(spaceLeft, sourceSlot.amount);
                targetSlot.amount += amountToMove;
                sourceSlot.amount -= amountToMove; // 남은 건 알아서 제자리에!

                if (sourceSlot.amount <= 0) sourceSlot.ClearSlot();
            }
        }
        // 3. 도착지에 [다른 아이템]이 있는 경우 -> 맞바꾸기(Swap)
        else
        {
            ItemData tempItem = targetSlot.item;
            int tempAmount = targetSlot.amount;

            targetSlot.item = sourceSlot.item;
            targetSlot.amount = sourceSlot.amount;

            sourceSlot.item = tempItem;
            sourceSlot.amount = tempAmount;
        }

        // 4. 두 슬롯의 그림과 텍스트 새로고침
        sourceSlot.UpdateSlotUI();
        targetSlot.UpdateSlotUI();
    }

    // ✨ 인벤토리 상태 변경과 UI 활성화/비활성화를 하나로 합칩니다.
    public void SetInventoryState(bool isOpen)
    {
        isInventoryOpen = isOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isOpen);
        }

        // ✨ [수정 2] 인벤토리를 닫을 때(!isOpen), 아이템을 들고 있다면(선택된 슬롯이 있다면)
        if (!isOpen && _selectedSlot != null)
        {
            // 선택을 취소하고 아이템을 원래 자리에 돌려놓습니다. (마우스 아이콘도 꺼짐)
            ClearSelection();
        }
    }

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

    public void CloseUI()
    {
        SetInventoryState(false);
    }
}