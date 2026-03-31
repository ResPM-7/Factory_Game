using UnityEngine;
using UnityEngine.InputSystem;
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

    private bool isInventoryOpen = false;

    // ✨ 클릭 이동을 위한 핵심 변수: 현재 선택된(들고 있는) 슬롯
    private InventorySlot _selectedSlot = null;

    void Awake() => Instance = this;

    void Start()
    {
        // ✨ 불필요해진 카메라/플레이어 컨트롤러 찾기 코드 싹 삭제!

        // 초기 상태 설정
        isInventoryOpen = false;
        if (inventoryUI != null) inventoryUI.SetActive(false);
    }

    void Update()
    {
        // I키로 인벤토리 열고 닫기 (이제 UIManager 눈치 안 보고 내 창만 껐다 켭니다!)
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            SetInventoryState(!inventoryUI.activeSelf);
        }

        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            ToggleCraftingUI();
        }

        // 아이템을 들고(선택하고) 있을 때 G 키를 누르면 바닥에 버립니다.
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

        // 버린 후 슬롯을 비우고 선택 상태를 해제합니다.
        _selectedSlot.ClearSlot();
        ClearSelection();
    }

    private void ToggleCraftingUI()
    {
        if (CraftingUI.Instance == null) return;

        bool isCurrentlyOpen = CraftingUI.Instance.uiPanel.activeSelf;

        if (isCurrentlyOpen)
        {
            if (UIManager.Instance != null) UIManager.Instance.CloseAllMenus();
        }
        else
        {
            CraftingUI.Instance.OpenCraftingUI();
        }
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            // ✨ UpdateInputState(); 삭제됨 (UIManager가 알아서 함)
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

                if (dragIcon != null && _selectedSlot.item.iconTexture != null)
                {
                    dragIcon.texture = _selectedSlot.item.iconTexture;
                    dragIcon.enabled = true;

                    Color dragColor = dragIcon.color;
                    dragColor.a = 1.0f;
                    dragIcon.color = dragColor;
                }

                if (dragAmountText != null)
                {
                    dragAmountText.enabled = true;
                    dragAmountText.text = _selectedSlot.amount > 1 ? _selectedSlot.amount.ToString() : "";
                }

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
            _selectedSlot.UpdateSlotUI();
            _selectedSlot.HighlightSlot(false);
            _selectedSlot = null;
        }

        if (dragObject != null)
        {
            dragObject.SetActive(false);
        }
    }

    private void MoveOrMergeItem(InventorySlot sourceSlot, InventorySlot targetSlot)
    {
        if (sourceSlot.item == null) return;

        if (targetSlot.item == null)
        {
            targetSlot.item = sourceSlot.item;
            targetSlot.amount = sourceSlot.amount;
            sourceSlot.ClearSlot();
        }
        else if (targetSlot.item == sourceSlot.item)
        {
            int maxStack = targetSlot.item.maxStackSize;
            int spaceLeft = maxStack - targetSlot.amount;

            if (spaceLeft > 0)
            {
                int amountToMove = Mathf.Min(spaceLeft, sourceSlot.amount);
                targetSlot.amount += amountToMove;
                sourceSlot.amount -= amountToMove;

                if (sourceSlot.amount <= 0) sourceSlot.ClearSlot();
            }
        }
        else
        {
            ItemData tempItem = targetSlot.item;
            int tempAmount = targetSlot.amount;

            targetSlot.item = sourceSlot.item;
            targetSlot.amount = sourceSlot.amount;

            sourceSlot.item = tempItem;
            sourceSlot.amount = tempAmount;
        }

        sourceSlot.UpdateSlotUI();
        targetSlot.UpdateSlotUI();
    }

    public void SetInventoryState(bool isOpen)
    {
        isInventoryOpen = isOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isOpen);
        }

        if (!isOpen && _selectedSlot != null)
        {
            ClearSelection();
        }
    }

    // ✨ UpdateInputState(), SetCursorState(), SetPlayerControl() 함수 전부 삭제됨! (UIManager가 전담)

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

    public int GetItemCount(ItemData data)
    {
        int total = 0;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == data) total += slot.amount;
        }
        return total;
    }

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
        // ✨ UpdateInputState(); 삭제됨
    }

    public void CloseUI()
    {
        SetInventoryState(false);
    }
}