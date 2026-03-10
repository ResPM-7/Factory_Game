using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventorySlot _mySlot;
    private CanvasGroup _canvasGroup;
    private Vector3 _originalPosition;
    private Transform _originalParent;

    void Awake()
    {
        _mySlot = GetComponent<InventorySlot>();
        // 인스펙터에서 넣은 Canvas Group을 가져옵니다.
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[클릭] {gameObject.name} 눌림!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_mySlot.item == null) return;

        // 1. 매니저의 드래그 오브젝트 활성화 및 데이터 세팅
        var manager = InventoryManager.Instance;
        manager.dragIcon.texture = _mySlot.item.iconTexture;
        manager.dragAmountText.text = _mySlot.amount > 1 ? _mySlot.amount.ToString() : "";
        manager.dragObject.SetActive(true);

        // 2. 현재 슬롯의 아이콘은 잠시 숨기기 (들고 있는 느낌을 주기 위해)
        _mySlot.iconRawImage.enabled = false;
        _mySlot.stackText.enabled = false;

        // 3. 레이캐스트 차단 (밑에 있는 슬롯이 마우스를 감지해야 하므로)
        manager.dragObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 매니저의 드래그 오브젝트가 마우스를 따라다니게 함
        InventoryManager.Instance.dragObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryManager.Instance.dragObject.SetActive(false);
        _mySlot.UpdateSlotUI();

        // 1. 마우스가 가리키는 곳에 UI가 있는지 확인
        GameObject target = eventData.pointerCurrentRaycast.gameObject;

        if (target != null)
        {
            // UI 위에 놓았을 때: 기존 Swap(교체) 로직
            InventorySlot targetSlot = target.GetComponentInParent<InventorySlot>();
            if (targetSlot != null && targetSlot != _mySlot)
            {
                SwapItems(_mySlot, targetSlot);
            }
        }
        else
        {
            // 2. UI가 없는 허공(월드)에 놓았을 때: 아이템 버리기 실행
            DropItem();
        }
    }

    private void DropItem()
    {
        if (_mySlot.item == null || _mySlot.item.prefab == null) return;

        // 1. "Player" 태그가 붙은 오브젝트 찾기
        GameObject player = GameObject.FindWithTag("Player");

        // 플레이어를 찾지 못했을 경우를 대비한 안전장치
        if (player == null)
        {
            Debug.LogError("'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        ItemData itemToDrop = _mySlot.item;
        int amountToDrop = _mySlot.amount;
        Transform playerTransform = player.transform;

        // 2. 플레이어 앞 1.2m, 높이 1m 지점에서 시작 (캐릭터 허리 높이 정도)
        Vector3 dropPosition = playerTransform.position + (playerTransform.forward * 1.2f) + (Vector3.up * 1.0f);

        // 3. 바닥 높이 감지 (Raycast)

        RaycastHit hit;
        if (Physics.Raycast(dropPosition, Vector3.down, out hit, 5f))
        {
            // 바닥을 찾았다면 그 지점보다 살짝 위(0.1m)에 배치
            dropPosition.y = hit.point.y + 0.1f;
        }

        // 4. 아이템 소환 (풀 매니저 사용)
        GameObject droppedItem = ItemPoolManager.Instance.GetItem(itemToDrop, dropPosition, Quaternion.identity);

        // 5. 정보 주입
        var pickupScript = droppedItem.GetComponent<ItemPickup>();
        if (pickupScript != null)
        {
            pickupScript.item = itemToDrop;
            pickupScript.amount = amountToDrop;
        }

        // 6. 물리 효과 (플레이어 정면 방향으로 살짝 밀어내기)
        if (droppedItem.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero; // Unity 6 최신 API 적용
            rb.AddForce(playerTransform.forward * 1.0f, ForceMode.Impulse);
        }

        // 7. 슬롯 비우기 및 로그
        _mySlot.ClearSlot();
        Debug.Log($"{itemToDrop.itemName}을(를) 플레이어 앞에 버렸습니다.");
    }

    private void SwapItems(InventorySlot a, InventorySlot b)
    {
        // 데이터 교체 로직
        ItemData tempItem = a.item;
        int tempAmount = a.amount;

        a.item = b.item;
        a.amount = b.amount;

        b.item = tempItem;
        b.amount = tempAmount;

        // UI 갱신
        a.UpdateSlotUI();
        b.UpdateSlotUI();
    }
}