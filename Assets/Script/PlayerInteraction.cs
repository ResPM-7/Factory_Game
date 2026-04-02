using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;
    public float offsetHeight = 1.0f;

    // ✨ 1. 성능을 위해 변수를 미리 빼둡니다 (캐싱)
    private PlayerInput _playerInput;
    private InputAction _interactAction;

    void Start()
    {
        // ✨ 2. 게임 시작 시 딱 한 번만 컴포넌트와 액션을 찾아서 저장합니다.
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput != null)
        {
            _interactAction = _playerInput.actions.FindAction("Interaction");
        }
    }

    void Update()
    {
        // ✨ 3. [가드 코드] 현재 InputManager가 'Player' 모드가 아니라면 아예 무시!
        // (건축 중이거나 UI가 켜져 있을 때 엉뚱한 상호작용이 일어나는 걸 원천 차단합니다)
        if (InputManager.Instance != null && InputManager.Instance.currentMode != InputMode.Player)
        {
            return;
        }

        // 4. 미리 저장해둔 액션이 방금 눌렸는지 확인
        if (_interactAction != null && _interactAction.WasPressedThisFrame())
        {
            // (참고: InputManager가 작동 중이라면 UI가 열렸을 때 모드가 'UI'로 바뀌므로 
            // 여기까지 코드가 내려오지 않지만, 이중 안전장치로 남겨두셔도 좋습니다.)
            if (UIManager.Instance != null && UIManager.Instance.IsAnyUIOpen())
            {
                UIManager.Instance.CloseAllMenus();
            }
            else
            {
                CheckInteract();
            }
        }
    }

    void CheckInteract()
    {
        Vector3 origin = transform.position + (Vector3.up * offsetHeight);

        // 디버깅용 레이저 그리기 (에디터 씬 뷰에서만 보임, 1.5초 유지)
        Debug.DrawRay(origin, transform.forward * interactDistance, Color.green, 1.5f);

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance, interactLayer))
        {
            Debug.Log($"레이저가 맞은 물체: {hit.collider.gameObject.name}");

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