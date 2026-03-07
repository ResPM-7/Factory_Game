using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // 필수

public class CameraManager : MonoBehaviour
{
    [Header("카메라 설정")]
    public CinemachineCamera thirdPersonCamera;
    public CinemachineCamera firstPersonCamera;

    [Header("입력 설정")]
    // 인스펙터에서 지웅님이 만든 'CameraControll' 액션을 드래그해서 넣을 칸입니다.
    public InputActionReference toggleCameraAction;

    private bool isFirstPerson = false;

    // 스크립트가 켜질 때 액션 활성화
    private void OnEnable() => toggleCameraAction.action.Enable();
    // 스크립트가 꺼질 때 액션 비활성화
    private void OnDisable() => toggleCameraAction.action.Disable();

    void Update()
    {
        // 지웅님이 설정한 'V' 키가 눌렸는지 확인
        if (toggleCameraAction.action.WasPressedThisFrame())
        {
            ToggleCamera();
        }
    }

    void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;
        thirdPersonCamera.Priority = isFirstPerson ? 10 : 20;
        firstPersonCamera.Priority = isFirstPerson ? 20 : 10;
        Debug.Log(isFirstPerson ? "1인칭 시점" : "3인칭 시점");
    }
}