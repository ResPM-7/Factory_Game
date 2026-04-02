using UnityEngine;
using UnityEngine.InputSystem;

// 1. 현재 내 게임의 조작 상태를 정의합니다.
public enum InputMode
{
    Player,
    Building,
    UI,
    Dialogue // 나중에 대화 기능 생기면 추가
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInput _playerInput;
    public InputMode currentMode = InputMode.Player; // 현재 상태

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _playerInput = Object.FindAnyObjectByType<PlayerInput>();
    }

    // 오직 이 함수 하나만 Action Map을 바꿀 권한을 가집니다!
    public void ChangeInputMode(InputMode newMode)
    {
        if (_playerInput == null) return;

        currentMode = newMode;

        switch (currentMode)
        {
            case InputMode.Player:
                _playerInput.SwitchCurrentActionMap("Player");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case InputMode.Building:
                _playerInput.SwitchCurrentActionMap("Building");
                // 건축 모드에서도 마우스를 움직여야 하니 커서 숨김
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case InputMode.UI:
                _playerInput.SwitchCurrentActionMap("UI");
                // UI 모드에서는 마우스가 보여야 함
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }

        Debug.Log($"<color=green>[InputManager]</color> 조작 모드 변경됨: {newMode}");
    }
}