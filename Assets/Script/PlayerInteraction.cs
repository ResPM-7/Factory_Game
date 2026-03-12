using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;
    public float offsetHeight = 1.0f;

    void Update()
    {
        // 1. 현재 활성화된 맵에서 Interaction 키(E)를 가져옵니다.
        var interact = GetComponent<PlayerInput>().actions.FindAction("Interaction");

        if (interact != null && interact.WasPressedThisFrame())
        {
            // 2. ✨ 현재 UI가 하나라도 켜져 있는지 체크합니다.
            bool isUIActive = (StorageUI.Instance != null && StorageUI.Instance.uiPanel.activeSelf) ||
                              (InventoryManager.Instance != null && InventoryManager.Instance.inventoryUI.activeSelf);

            if (isUIActive)
            {
                // 3. ✨ UI가 켜져 있다면? UIManager를 시켜서 몽땅 닫습니다.
                UIManager.Instance.CloseAllMenus();
            }
            else
            {
                // 4. UI가 다 닫혀 있다면? 원래 하던 대로 상호작용(레이저) 실행!
                CheckInteract();
            }
        }
    }

    void CheckInteract()
    {
        Vector3 origin = transform.position + (Vector3.up * offsetHeight);
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent(out Storage storage))
            {
                storage.Interact();
            }
        }
    }
}