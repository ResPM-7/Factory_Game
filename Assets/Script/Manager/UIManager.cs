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
        // 최신 유니티 6 방식의 컴포넌트 찾기
        _playerInput = Object.FindAnyObjectByType<PlayerInput>();
        _playerInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        _cameraController = Object.FindAnyObjectByType<CinemachineInputAxisController>();
    }

    void Update()
    {
        bool isCurrentlyOpen = IsAnyUIOpen();

        // UI 상태가 '꺼짐 -> 켜짐' 또는 '켜짐 -> 꺼짐'으로 방금 변했다면?
        if (isCurrentlyOpen != _wasAnyUIOpen)
        {
            _wasAnyUIOpen = isCurrentlyOpen; // 상태 업데이트
            UpdateInputState();              // 즉시 플레이어 조작 상태 변경!
        }

        if (isCurrentlyOpen && _playerInput != null)
        {
            // 우리가 에디터에서 만든 Action 이름이 "Cancel"일 경우
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
            if (panel != null && panel.activeSelf)
            {
                return true; // 하나라도 켜져 있으면 true 반환
            }
        }
        return false; // 모두 꺼져 있으면 false 반환
    }

    public void CloseAllMenus()
    {
        foreach (GameObject panel in allUIPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                IUIPanel uiManager = panel.GetComponentInParent<IUIPanel>();

                if (uiManager != null)
                {
                    uiManager.CloseUI();
                }
                else
                {
                    panel.SetActive(false);
                }
            }
        }

        // 조작 복구
        RestorePlayerControl();
    }

    // 캐릭터 조작을 원래대로 돌리는 로직
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

    public void UpdateInputState()
    {
        if (IsAnyUIOpen())
        {
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (_cameraController != null) _cameraController.enabled = false;

            if (_playerInputs != null)
            {
                _playerInputs.move = Vector2.zero;
                _playerInputs.look = Vector2.zero;
            }
        }
        else
        {
            RestorePlayerControl();
        }
    }
}