using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // 시네머신 제어를 위해 추가
using StarterAssets;

public class InventoryManager : MonoBehaviour
{
    [Header("UI 오브젝트")]
    public GameObject inventoryUI;

    [Header("입력 설정")]
    public InputActionReference inventoryAction;

    private StarterAssetsInputs _playerInputs;
    private PlayerInput _playerInputComponent;
    private CinemachineInputAxisController _cameraInputController; // 카메라 입력 제어용
    private bool isInventoryOpen = false;

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
}