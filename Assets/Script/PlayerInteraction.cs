using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactDistance = 2.5f; // 상호작용 가능 거리
    public LayerMask interactLayer;     // 상호작용 가능한 레이어 (예: Interactable)
    public float offsetHeight = 1.0f;    // 레이저 발사 높이 보정

    void Update()
    {
        // 1. Input System에서 'Interaction' 액션(보통 E키)을 가져옵니다.
        var interact = GetComponent<PlayerInput>().actions.FindAction("Interaction");

        if (interact != null && interact.WasPressedThisFrame())
        {
            // 2. UIManager를 통해 현재 화면에 UI가 하나라도 떠 있는지 확인합니다.
            if (UIManager.Instance != null && UIManager.Instance.IsAnyUIOpen())
            {
                // UI가 있다면 상호작용 대신 모든 메뉴를 닫습니다.
                UIManager.Instance.CloseAllMenus();
            }
            else
            {
                // UI가 없다면 정면의 물체와 상호작용을 시도합니다.
                CheckInteract();
            }
        }
    }

    void CheckInteract()
    {
        Vector3 origin = transform.position + (Vector3.up * offsetHeight);

        // 1. 설정한 interactLayer(예: Building)에만 레이저가 반응합니다.
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance, interactLayer))
        {
            Debug.Log($"레이저가 맞은 물체: {hit.collider.gameObject.name}");

            // 2. 맞은 물체에 IInteractable(상호작용 기능)이 있는지 확인하고 바로 실행!
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                Debug.Log("🟢 IInteractable 인터페이스 확인됨. Interact 실행!");
                interactable.Interact();
            }
            else
            {
                Debug.Log("🔴 맞은 물체에 IInteractable 인터페이스가 없습니다.");
            }
        }
    }
}