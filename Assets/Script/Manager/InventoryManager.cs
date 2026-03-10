using StarterAssets;
using TMPro;
using Unity.Cinemachine; // 시네머신 제어를 위해 추가
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // 싱글톤 설정

    [Header("드래그 전용 UI (고스트 아이콘)")]
    public GameObject dragObject; // 이미지와 텍스트가 담긴 부모 오브젝트
    public RawImage dragIcon;
    public Text dragAmountText;

    [Header("모든 슬롯 리스트")]
    public InventorySlot[] allSlots; // 인스펙터에서 30개 슬롯을 모두 드래그해서 넣어주세요.

    [Header("UI 오브젝트")]
    public GameObject inventoryUI;

    [Header("입력 설정")]
    public InputActionReference inventoryAction;

    private StarterAssetsInputs _playerInputs;
    private PlayerInput _playerInputComponent;
    private CinemachineInputAxisController _cameraInputController; // 카메라 입력 제어용
    private bool isInventoryOpen = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 유니티 6 최신 방식 사용
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _playerInputComponent = Object.FindAnyObjectByType<PlayerInput>();

        // 씬에서 시네머신 입력 컨트롤러를 찾습니다.
        _cameraInputController = Object.FindAnyObjectByType<CinemachineInputAxisController>();

        isInventoryOpen = false;
        inventoryUI.SetActive(false);
        SetCursorState(false);
    }

    private void OnEnable() => inventoryAction.action.Enable();
    private void OnDisable() => inventoryAction.action.Disable();

    void Update()
    {
        if (inventoryAction.action.WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);
        SetCursorState(isInventoryOpen);

        if (_playerInputs != null && _playerInputComponent != null)
        {
            if (isInventoryOpen)
            {
                // 1. 플레이어 이동 입력 차단
                _playerInputComponent.actions.Disable();
                _playerInputs.move = Vector2.zero;

                // 2. 카메라 회전 입력 차단 (가장 중요!)
                if (_cameraInputController != null) _cameraInputController.enabled = false;

                // 3. 인벤토리 닫기 액션만 다시 활성화
                inventoryAction.action.Enable();
            }
            else
            {
                // 4. 다시 모든 조작 활성화
                _playerInputComponent.actions.Enable();
                if (_cameraInputController != null) _cameraInputController.enabled = true;
            }
        }
    }

    private void SetCursorState(bool isOpen)
    {
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    public bool AddItem(ItemData data, int amount)
    {
        int remainingAmount = amount; // 담아야 할 남은 수량

        // 1. 기존에 있는 같은 아이템 슬롯에 먼저 채우기
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == data && slot.amount < data.maxStackSize)
            {
                // 이 슬롯에 들어갈 수 있는 여유 공간 계산
                int canAdd = data.maxStackSize - slot.amount;

                // 넣을 수 있는 만큼만 넣음
                int amountToAdd = Mathf.Min(canAdd, remainingAmount);

                slot.amount += amountToAdd;
                remainingAmount -= amountToAdd;

                slot.UpdateSlotUI();

                // 다 담았으면 끝!
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
                    // 빈칸에 담을 수 있는 최대치 계산
                    int amountToAdd = Mathf.Min(data.maxStackSize, remainingAmount);

                    slot.item = data;
                    slot.amount = amountToAdd;
                    remainingAmount -= amountToAdd;

                    slot.UpdateSlotUI();

                    // 다 담았으면 끝!
                    if (remainingAmount <= 0) return true;
                }
            }
        }

        // 3. 인벤토리가 다 찼는데도 남았다면 (인벤토리 풀)
        if (remainingAmount > 0)
        {
            Debug.Log($"{remainingAmount}개를 담을 공간이 부족합니다!");
            // 여기서 바닥에 다시 버리게 하거나 알림을 띄울 수 있습니다.
            return false;
        }

        return true;
    }
}