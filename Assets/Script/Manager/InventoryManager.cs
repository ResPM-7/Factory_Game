using StarterAssets;
using TMPro;
using Unity.Cinemachine; // 시네머신 제어를 위해 추가
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // ✨ 에러를 해결할 핵심 변수 3가지입니다.
    [Header("드래그 전용 UI (고스트 아이콘)")]
    public GameObject dragObject; // 마우스를 따라다닐 부모 오브젝트
    public RawImage dragIcon;     // 아이템 이미지가 표시될 곳
    public Text dragAmountText;   // 아이템 개수가 표시될 곳 (TMP라면 TextMeshProUGUI로 변경)

    [Header("모든 슬롯 리스트")]
    public InventorySlot[] allSlots;

    public GameObject inventoryUI;

    private StarterAssetsInputs _playerInputs;
    private PlayerInput _playerInputComponent;
    private CinemachineInputAxisController _cameraInputController;
    private bool isInventoryOpen = false;

    void Awake() => Instance = this;

    void Start()
    {
        // 유니티 6 방식의 컴포넌트 찾기
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _playerInputComponent = Object.FindAnyObjectByType<PlayerInput>();
        _cameraInputController = Object.FindAnyObjectByType<CinemachineInputAxisController>();

        // ✨ 시작할 때 인벤토리와 마우스 커서 상태를 초기화합니다.
        isInventoryOpen = false;
        if (inventoryUI != null) inventoryUI.SetActive(false);

        // 조작 가능 모드(Player)로 시작
        SetPlayerControl(true);
    }

    void Update()
    {
        var invAction = _playerInputComponent.actions.FindAction("Inventory");
        if (invAction != null && invAction.WasPressedThisFrame())
        {
            bool isStorageOpen = StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf;

            if (isInventoryOpen || isStorageOpen)
            {
                // ✨ InventoryManager 대신 UIManager에게 일을 시킵니다.
                UIManager.Instance.CloseAllMenus();
            }
            else
            {
                OpenInventory(); // 열기는 본인이 직접!
            }
        }
    }

    public bool AddItem(ItemData data, int amount)
    {
        int remainingAmount = amount; // 담아야 할 남은 수량

        // 1. 기존에 있는 같은 아이템 슬롯에 먼저 채우기 (Stacking)
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

        // 2. 남은 수량이 있다면 빈 슬롯 찾아서 채우기
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

        // 3. 인벤토리가 꽉 찼을 때
        if (remainingAmount > 0)
        {
            Debug.LogWarning("인벤토리가 가득 차서 아이템을 더 담을 수 없습니다!");
            return false;
        }

        return true;
    }

    // [열기] 인벤토리만 열 때
    public void OpenInventory()
    {
        isInventoryOpen = true;
        inventoryUI.SetActive(true);
        SetPlayerControl(false); // 조작 차단
    }

    // [열기] 상자 열 때 강제 호출용
    public void OpenInventoryForce()
    {
        isInventoryOpen = true;
        inventoryUI.SetActive(true);
    }
/*
    // [닫기] UI의 X 버튼에 연결할 함수 (가장 중요!)
    public void CloseAllMenus()
    {
        // 1. 상자 데이터 저장 및 창 닫기
        if (StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf)
        {
            StorageUI.Instance.CloseStorage();
        }

        // 2. 인벤토리 창 닫기
        isInventoryOpen = false;
        inventoryUI.SetActive(false);

        // 3. 조작 복구
        SetPlayerControl(true);
    }*/

    public void SetPlayerControl(bool isEnable)
    {
        if (isEnable)
        {
            _playerInputComponent.SwitchCurrentActionMap("Player");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (_cameraInputController != null) _cameraInputController.enabled = true;
        }
        else
        {
            _playerInputComponent.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 이동값 초기화 및 카메라 차단
            _playerInputs.move = Vector2.zero;
            _playerInputs.look = Vector2.zero;
            if (_cameraInputController != null) _cameraInputController.enabled = false;
        }
    }

    // InventoryManager.cs 안에 추가
public void SetInventoryState(bool isOpen)
{
    isInventoryOpen = isOpen;
}
}