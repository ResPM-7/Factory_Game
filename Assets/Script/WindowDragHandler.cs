using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 처리하기 위해 필요합니다.

public class WindowDragHandler : MonoBehaviour, IDragHandler
{
    [Header("이동시킬 전체 창 (Inventory)")]
    [SerializeField] private RectTransform windowTransform;

    private Canvas _canvas;

    void Awake()
    {
        // 1. 만약 인스펙터에서 할당 안 했다면 부모 중 최상위인 Inventory를 자동으로 찾습니다.
        if (windowTransform == null)
            windowTransform = transform.parent.GetComponent<RectTransform>();

        // 2. 해상도 대응(ScaleFactor)을 위해 캔버스를 가져옵니다.
        _canvas = GetComponentInParent<Canvas>();
    }

    // 드래그 중일 때 매 프레임 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        if (windowTransform == null) return;

        // ✨ 핵심 로직: 마우스의 이동량(delta)을 캔버스 스케일에 맞춰 창 위치에 더해줍니다.
        // canvas.scaleFactor로 나눠주지 않으면 마우스 속도와 창 속도가 어긋납니다.
        windowTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }
}