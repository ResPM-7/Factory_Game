using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("카메라 설정")]
    public CinemachineCamera thirdPersonCamera;
    public CinemachineCamera firstPersonCamera;

    [Header("입력 설정")]
    public InputActionReference toggleCameraAction;

    public bool isAiming { get; private set; }
    private bool isFirstPersonToggle = false; // V키 상태 저장용

    void Awake() => Instance = this;

    private void OnEnable() => toggleCameraAction.action.Enable();
    private void OnDisable() => toggleCameraAction.action.Disable();

    void Start()
    {
        // 마우스 커서 숨기기 및 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. V키를 누를 때마다 토글 상태(true/false)를 뒤집습니다.
        if (toggleCameraAction != null && toggleCameraAction.action.WasPressedThisFrame())
        {
            isFirstPersonToggle = !isFirstPersonToggle;
        }

        // 2. 우클릭이 눌려있는지 매 프레임 확인합니다.
        if (Mouse.current != null)
        {
            isAiming = Mouse.current.rightButton.isPressed;
        }

        // 둘 다 아니면 3인칭으로 자동 복귀!
        bool shouldUseFirstPerson = isAiming || isFirstPersonToggle;

        firstPersonCamera.Priority = shouldUseFirstPerson ? 20 : 10;
        thirdPersonCamera.Priority = shouldUseFirstPerson ? 10 : 20;
    }
}