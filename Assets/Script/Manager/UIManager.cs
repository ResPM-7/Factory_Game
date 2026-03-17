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

    // ✨ 지웅 님이 원하던 "어떤 UI든 닫아주는" 만능 함수
    public void CloseAllMenus()
    {
        foreach (GameObject panel in allUIPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                // 패널 오브젝트나 그 부모에 있는 IUIPanel 인터페이스를 찾습니다.
                IUIPanel uiManager = panel.GetComponentInParent<IUIPanel>();

                if (uiManager != null)
                {
                    // 인터페이스가 존재하면 각 UI의 고유 닫기 로직을 실행합니다.
                    uiManager.CloseUI();
                }
                else
                {
                    // 인터페이스가 없는 단순 UI라면 패널만 비활성화합니다.
                    panel.SetActive(false);
                }
            }
        }

        // 조작 복구
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

    public void UpdateInputState()
    {
        // IsAnyUIOpen()은 allUIPanels 리스트를 알아서 뒤져서 하나라도 켜져있는지 확인해 줍니다.
        if (IsAnyUIOpen())
        {
            // UI가 하나라도 켜져 있다면 -> UI 조작 모드 (마우스 등장)
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (_cameraController != null) _cameraController.enabled = false;
        }
        else
        {
            // UI가 모두 꺼져 있다면 -> 플레이어 조작 모드 (마우스 숨김)
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("Player");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (_cameraController != null) _cameraController.enabled = true;
        }
    }

}