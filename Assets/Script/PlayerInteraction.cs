using UnityEngine;
using UnityEngine.InputSystem; // 1. 네임스페이스 추가

public class PlayerInteraction : MonoBehaviour
{
    [Header("입력 설정)")]
    public InputActionReference interactAction; // 2. KeyCode 대신 Action 사용

    [Header("레이캐스트 설정")]
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;
    public float offsetHeight = 1.0f;

    // 3. 액션 활성화/비활성화 처리
    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    void Update()
    {
        // 4. WasPressedThisFrame()으로 키 눌림 감지
        if (interactAction.action.WasPressedThisFrame())
        {
            CheckInteract();
        }

        // 디버그용 레이저 (에디터 전용)
        Vector3 origin = transform.position + (Vector3.up * offsetHeight);
        Debug.DrawRay(origin, transform.forward * interactDistance, Color.red);
    }

    void CheckInteract()
    {
        Vector3 origin = transform.position + (Vector3.up * offsetHeight);
        Vector3 direction = transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent(out Storage storage))
            {
                storage.Interact();
                Debug.Log($"[Unity 6 Input] {storage.storageName} 오픈!");
            }
        }
    }
}