using UnityEngine;
using UnityEngine.InputSystem;

public class DemolitionManager : MonoBehaviour
{
    public static DemolitionManager Instance;

    [Header("철거 설정")]
    public bool isDemolitionMode = false;
    public LayerMask placedBuildingLayer;
    public float maxDemolishDistance = 15f;

    private PlacedBuilding _targetToDemolish;
    private Camera _mainCamera;
    private PlayerInput _playerInput;

    void Awake()
    {
        Instance = this;
        _mainCamera = Camera.main;
        _playerInput = Object.FindAnyObjectByType<PlayerInput>();
    }

    void Update()
    {
        // 1. UI가 켜져 있으면 아무것도 안 함
        if (InputManager.Instance != null && InputManager.Instance.currentMode == InputMode.UI) return;

        // 2. [핵심] 건축 모드여도 상관없이 X키가 눌리면 철거 모드를 토글함
        if (_playerInput != null && _playerInput.actions["ToggleDemolition"].triggered)
        {
            ToggleDemolitionMode(!isDemolitionMode);
        }

        // 3. 철거 모드 실행
        if (isDemolitionMode)
        {
            HandleDemolitionMode();
        }
    }

    public void ToggleDemolitionMode(bool turnOn)
    {
        isDemolitionMode = turnOn;

        if (turnOn)
        {
            // 철거 모드가 켜지면 상호작용 방지를 위해 Building 인풋 맵으로 전환
            InputManager.Instance.ChangeInputMode(InputMode.Building);
            Debug.Log("<color=red> 철거 모드 ON</color>");
        }
        else
        {
            if (_targetToDemolish != null) _targetToDemolish.HighlightForDemolition(false);
            _targetToDemolish = null;

            // 철거 모드가 꺼졌을 때, 건축 중이었다면 Building 모드 유지, 아니면 Player 모드로 복구
            if (BuildManager.Instance.isBuildingMode)
            {
                InputManager.Instance.ChangeInputMode(InputMode.Building);
            }
            else
            {
                InputManager.Instance.ChangeInputMode(InputMode.Player);
            }
            Debug.Log("<color=white> 철거 모드 OFF</color>");
        }
    }

    private void HandleDemolitionMode()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // 우클릭/ESC로 철거 취소
        if (mouse.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleDemolitionMode(false);
            return;
        }

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDemolishDistance, placedBuildingLayer))
        {
            PlacedBuilding hitBuilding = hit.collider.GetComponentInParent<PlacedBuilding>();

            if (hitBuilding != null && hitBuilding != _targetToDemolish)
            {
                if (_targetToDemolish != null) _targetToDemolish.HighlightForDemolition(false);
                _targetToDemolish = hitBuilding;
                _targetToDemolish.HighlightForDemolition(true);
            }

            if (mouse.leftButton.wasPressedThisFrame && _targetToDemolish != null)
            {
                ExecuteDemolition(_targetToDemolish);
            }
        }
        else
        {
            if (_targetToDemolish != null)
            {
                _targetToDemolish.HighlightForDemolition(false);
                _targetToDemolish = null;
            }
        }
    }

    private void ExecuteDemolition(PlacedBuilding target)
    {
        if (target.originalBlueprint != null)
        {
            InventoryManager.Instance.AddItem(target.originalBlueprint, 1);
        }
        Destroy(target.gameObject);
        _targetToDemolish = null;
        Debug.Log("건물 철거 및 자원 환불 완료!");
    }
}