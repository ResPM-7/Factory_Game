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
        // 드래그 오브젝트 비활성화 및 원래 슬롯 복구
        InventoryManager.Instance.dragObject.SetActive(false);
        _mySlot.UpdateSlotUI();

        // 3. 기존의 Swap 로직 실행...
        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        if (target != null)
        {
            InventorySlot targetSlot = target.GetComponentInParent<InventorySlot>();
            if (targetSlot != null && targetSlot != _mySlot)
            {
                SwapItems(_mySlot, targetSlot);
            }
        }
    }

    void DropItem()
    {
        // 플레이어 앞 위치에 아이템 프리팹 소환
        Vector3 dropPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        GameObject droppedObject = Instantiate(_mySlot.item.prefab, dropPos, Quaternion.identity);

        // 소환된 오브젝트에 Pickup 스크립트가 있다면 정보 세팅
        ItemPickup pickup = droppedObject.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.item = _mySlot.item;
            pickup.amount = _mySlot.amount;
        }

        _mySlot.ClearSlot(); // 슬롯 비우기
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