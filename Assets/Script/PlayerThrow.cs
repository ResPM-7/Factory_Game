using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    [Header("발사 설정")]
    public Transform throwPoint;
    public float throwForce = 15f;

    public Transform aimCameraTransform;

    private PlayerInput _playerInput;

    void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();

        // 조준 카메라를 따로 지정 안 했다면 메인 카메라로 자동 설정
        if (aimCameraTransform == null && Camera.main != null)
        {
            aimCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (ActionbarManager.Instance == null || InventoryManager.Instance == null) return;
        if (CameraManager.Instance == null) return;

        ItemData heldItem = ActionbarManager.Instance.SelectedItem;

        // 조준 중일 때만 작동!
        if (heldItem != null && heldItem.isCaptureBall && CameraManager.Instance.isAiming)
        {
            // 수정됨: "Throw" 대신 "Attack"(좌클릭) 신호를 받아옵니다!
            if (_playerInput.actions["Attack"].triggered)
            {
                if (InventoryManager.Instance.GetItemCount(heldItem) > 0)
                {
                    ThrowBall(heldItem);
                }
            }
        }
    }

    private void LateUpdate()
    {
        // 1. 조준 중(우클릭 유지)인지 확인
        if (CameraManager.Instance != null && CameraManager.Instance.isAiming)
        {
            if (Camera.main != null)
            {
                // 2. 메인 카메라가 바라보는 방향을 가져옵니다.
                Vector3 cameraForward = Camera.main.transform.forward;

                // 3. 캐릭터가 땅바닥을 보거나 하늘을 보며 기울어지지 않도록 Y축(높낮이)은 0으로 평평하게 깎아줍니다.
                cameraForward.y = 0f;

                // 4. 캐릭터의 회전(Rotation)을 카메라가 보는 방향으로 강제로 고정!
                if (cameraForward.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(cameraForward.normalized);
                }
            }
        }
    }

    private void ThrowBall(ItemData ballData)
    {
        if (ballData.placeablePrefab == null || throwPoint == null) return;

        InventoryManager.Instance.RemoveItem(ballData, 1);
        ActionbarManager.Instance.UpdateUI();

        GameObject ballObj = ItemPoolManager.Instance.GetPrefab(ballData.placeablePrefab, throwPoint.position, throwPoint.rotation);

        if (ballObj.TryGetComponent(out CaptureBall ball))
        {
            ball.originalPrefab = ballData.placeablePrefab;

            // [핵심] 조준한 곳으로 던지기
            // throwPoint.forward 대신 '카메라가 바라보는 방향(forward)'을 기준으로 쏩니다.
            Vector3 throwDirection = aimCameraTransform.forward + (Vector3.up * 0.1f);
            ball.Throw(throwDirection.normalized * throwForce);

        }
    }
}
