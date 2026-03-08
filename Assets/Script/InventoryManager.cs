using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    [Header("UI 오브젝트")]
    public GameObject inventoryUI; // Inspector에서 'Inventory' 오브젝트 드래그

    [Header("입력 설정")]
    public InputActionReference inventoryAction;

    private bool isInventoryOpen = false;

    private void OnEnable() => inventoryAction.action.Enable();
    private void OnDisable() => inventoryAction.action.Disable();

    void Start()
    {
        isInventoryOpen = false;
        inventoryUI.SetActive(false); // 인벤토리 끄기

        // 게임 시작 시에는 마우스를 가두고 숨깁니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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

        // 1. 인벤토리 창 켜기/끄기
        inventoryUI.SetActive(isInventoryOpen);

        // 2. 마우스 커서 상태 제어 (중요!)
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None; // 커서 고정 해제
            Cursor.visible = true;                 // 커서 보이기
            // 여기서 플레이어 이동이나 카메라 회전을 멈추는 로직을 추가하면 더 좋습니다.
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // 다시 커서 가두기
            Cursor.visible = false;                  // 커서 숨기기
        }
    }
}