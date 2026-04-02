using StarterAssets;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("등록된 모든 UI 패널")]
    public List<GameObject> allUIPanels = new List<GameObject>();

    private PlayerInput _playerInput;
    private StarterAssetsInputs _playerInputs;
    private CinemachineInputAxisController _cameraController;

    private bool _wasAnyUIOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _playerInput = Object.FindAnyObjectByType<PlayerInput>();
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _cameraController = Object.FindAnyObjectByType<CinemachineInputAxisController>();
    }

    void Update()
    {
        bool isCurrentlyOpen = IsAnyUIOpen();

        if (isCurrentlyOpen != _wasAnyUIOpen)
        {
            _wasAnyUIOpen = isCurrentlyOpen;
            UpdateInputState();
        }

        if (isCurrentlyOpen && _playerInput != null)
        {
            if (_playerInput.actions["Cancel"].triggered)
            {
                CloseAllMenus();
            }
        }
    }

    public bool IsAnyUIOpen()
    {
        foreach (GameObject panel in allUIPanels)
        {
            if (panel != null && panel.activeSelf) return true;
        }
        return false;
    }

    public void CloseAllMenus()
    {
        foreach (GameObject panel in allUIPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                IUIPanel uiManager = panel.GetComponentInParent<IUIPanel>();

                if (uiManager != null) uiManager.CloseUI();
                else panel.SetActive(false);
            }
        }
        RestorePlayerControl();
    }

    // ✨ 캐릭터 조작을 원래대로 돌리는 로직 (수정됨)
    public void RestorePlayerControl()
    {
        // 1. 직접 바꾸지 않고 InputManager에게 부탁합니다!
        if (InputManager.Instance != null)
        {
            // 🔥 [핵심] 만약 지금 건축 모드 중이었다면, Player가 아니라 Building 모드로 돌려놔야 합니다!
            if (BuildManager.Instance != null && BuildManager.Instance.isBuildingMode)
            {
                InputManager.Instance.ChangeInputMode(InputMode.Building);
            }
            else
            {
                InputManager.Instance.ChangeInputMode(InputMode.Player);
            }
        }

        // 2. 카메라 및 캐릭터 이동 초기화 (이건 UIManager가 해도 됨)
        if (_cameraController != null) _cameraController.enabled = true;
        if (_playerInputs != null)
        {
            _playerInputs.move = Vector2.zero;
            _playerInputs.look = Vector2.zero;
            // InputManager에서 마우스를 가리지만, StarterAssets의 상태도 동기화해줍니다.
            _playerInputs.cursorLocked = true;
        }
    }

    // ✨ UI가 켜지거나 꺼질 때 인풋 상태 변경 (수정됨)
    public void UpdateInputState()
    {
        if (IsAnyUIOpen())
        {
            // 1. UI 모드로 변경해달라고 InputManager에게 요청!
            if (InputManager.Instance != null)
            {
                InputManager.Instance.ChangeInputMode(InputMode.UI);
            }

            // 2. 카메라 움직임 및 이동 막기
            if (_cameraController != null) _cameraController.enabled = false;
            if (_playerInputs != null)
            {
                _playerInputs.move = Vector2.zero;
                _playerInputs.look = Vector2.zero;
            }
        }
        else
        {
            // 창이 다 닫히면 똑똑하게 이전 상태(Player or Building)로 돌아감
            RestorePlayerControl();
        }
    }
}