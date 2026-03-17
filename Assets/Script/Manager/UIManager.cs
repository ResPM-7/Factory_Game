using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Unity.Cinemachine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private PlayerInput _playerInput;
    private StarterAssetsInputs _playerInputs;
    private CinemachineInputAxisController _cameraController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 최신 유니티 6 방식의 컴포넌트 찾기
        _playerInput = Object.FindAnyObjectByType<PlayerInput>();
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _cameraController = Object.FindAnyObjectByType<CinemachineInputAxisController>();
    }

    // ✨ 지웅 님이 원하던 "어떤 UI든 닫아주는" 만능 함수
    public void CloseAllMenus()
    {
        // 1. 상자 데이터 저장 및 닫기
        if (StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf)
        {
            StorageUI.Instance.CloseStorage();
        }

        // 2. 인벤토리 닫기
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetInventoryState(false);
        }

        // 3. ✨ 제작창(CraftingUI) 닫기 (오타 및 변수명 수정)
        if (CraftingUI.Instance != null && CraftingUI.Instance.uiPanel.activeSelf)
        {
            // SetCraftingUIState 하나만 부르면 패널 끄기 + 게이지 초기화가 알아서 다 됩니다.
            CraftingUI.Instance.SetCraftingUIState(false);
        }

        // 4. 조작 복구
        RestorePlayerControl();
    }

    // 캐릭터 조작을 원래대로 돌리는 로직을 별도 함수로 분리
    public void RestorePlayerControl()
    {
        if (_playerInput == null) return;

        // 조작 맵을 Player로 변경
        _playerInput.SwitchCurrentActionMap("Player");

        // 커서 상태 복구
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 카메라 및 이동 입력 초기화
        if (_cameraController != null) _cameraController.enabled = true;
        if (_playerInputs != null)
        {
            _playerInputs.move = Vector2.zero;
            _playerInputs.look = Vector2.zero;
            _playerInputs.cursorLocked = true;
        }
    }
}