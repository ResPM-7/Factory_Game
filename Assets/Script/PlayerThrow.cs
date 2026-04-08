using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    [Header("발사 설정")]
    public Transform throwPoint;
    public float throwForce = 15f;

    private PlayerInput _playerInput;

    void Awake()
    {
        // 동일 오브젝트 혹은 부모에게 있는 PlayerInput 컴포넌트를 가져옵니다.
        _playerInput = GetComponentInParent<PlayerInput>();
    }

    void Update()
    {
        if (ActionbarManager.Instance == null || InventoryManager.Instance == null) return;

        // 1. 현재 액션바에서 선택된 아이템 확인
        ItemData heldItem = ActionbarManager.Instance.SelectedItem;

        // 2. 포획 공이고, 인벤토리에 개수가 남아있는지 확인
        if (heldItem != null && heldItem.isCaptureBall)
        {
            if (_playerInput.actions["Throw"].triggered)
            {
                // 인벤토리에 실제 개수가 있는지 한 번 더 체크 (방어 코드)
                if (InventoryManager.Instance.GetItemCount(heldItem) > 0)
                {
                    ThrowBall(heldItem);
                }
            }
        }
    }

    private void ThrowBall(ItemData ballData)
    {
        if (ballData.placeablePrefab == null || throwPoint == null) return;

        // [핵심] 인벤토리 매니저의 기존 로직을 재사용하여 아이템 1개 차감!
        // 이 함수 내부에서 아이템 개수가 0이 되면 자동으로 ClearSlot()을 호출합니다.
        InventoryManager.Instance.RemoveItem(ballData, 1);

        // 액션바 UI도 즉시 갱신 (개수 텍스트 업데이트 및 0개 시 아이콘 제거)
        ActionbarManager.Instance.UpdateUI();

        // 오브젝트 풀링을 이용해 공 생성 및 발사
        GameObject ballObj = ItemPoolManager.Instance.GetPrefab(ballData.placeablePrefab, throwPoint.position, throwPoint.rotation);

        if (ballObj.TryGetComponent(out CaptureBall ball))
        {
            ball.originalPrefab = ballData.placeablePrefab;
            Vector3 throwDirection = throwPoint.forward + (Vector3.up * 0.15f);
            ball.Throw(throwDirection.normalized * throwForce);

            Debug.Log($"<color=yellow>{ballData.itemName}</color> 투척! (인벤토리에서 1개 소모)");
        }
    }
}