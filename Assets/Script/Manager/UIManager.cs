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
        // 1. 상자 데이터 저장 (상자가 켜져 있을 때만 실행)
        if (StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf)
        {
            StorageUI.Instance.CloseStorage();
        }

        // 2. 인벤토리 닫기
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.inventoryUI.SetActive(false);
            // InventoryManager 내부의 상태 변수도 직접 꺼줍니다.
            // (InventoryManager의 isInventoryOpen을 public으로 바꿔야 합니다)
            InventoryManager.Instance.SetInventoryState(false);
        }

        // 3. ✨ [추가 예정] 상점, 퀘스트 창 등이 있다면 여기에 추가하면 됩니다.
        // 예: if(ShopUI.Instance != null) ShopUI.Instance.CloseShop();

        // 4. 조작 복구 (가장 핵심)
        RestorePlayerControl();

        //Debug.Log("UIManager: 모든 UI 닫기 및 조작 복구 완료");
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