using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ✨ UI 이벤트를 처리하기 위해 인터페이스를 추가할 필요 없이, 
// Button 컴포넌트를 활용하는 방식이 가장 간편합니다.
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;      // 현재 슬롯의 아이템 데이터
    public int amount;         // 아이템 개수

    [Header("UI 연결")]
    public RawImage iconRawImage;    // 자식 오브젝트 중 'Icon' 연결
    public Text stackText; // 자식 오브젝트 중 'Stack' 연결

    //선택 효과(Highlight)를 위한 UI 요소
    [Header("Selection UI")]
    // 슬롯의 가장 바탕이 되는 Image 컴포넌트를 연결하세요 (예: 테두리나 배경)
    public Image backgroundImage;
    public Color normalColor = Color.white;    // 평소 색상
    public Color selectedColor = Color.yellow; // 선택되었을 때 색상 (노란색으로 빛나게!)

    void Start()
    {
        UpdateSlotUI();
        HighlightSlot(false);
    }

    // 슬롯 UI를 최신화하는 함수
    public void UpdateSlotUI()
    {
        // 아이콘 이미지의 컬러/투명도 설정
        Color iconColor = iconRawImage.color;

        if (item != null && amount > 0)
        {
            // 1. 아이콘 활성화 및 데이터 세팅
            iconColor.a = 1.0f;
            iconRawImage.color = iconColor;
            iconRawImage.texture = item.iconTexture;
            iconRawImage.enabled = true;

            // 2. 텍스트 활성화
            if (stackText != null)
            {
                stackText.enabled = true;
                stackText.text = amount > 1 ? amount.ToString() : "";
            }
        }
        else
        {
            // 아이템이 없을 때 초기화
            iconColor.a = 0f;
            iconRawImage.color = iconColor;
            iconRawImage.texture = null;
            iconRawImage.enabled = false;

            // 텍스트도 비활성화
            if (stackText != null)
            {
                stackText.text = "";
                stackText.enabled = false;
            }

            // 아이템 데이터 초기화 (null check 시 문제 방지)
            item = null;
            amount = 0;
        }
    }

    // [추가] 유니티 Button 컴포넌트의 OnClick 이벤트에 연결할 함수입니다.
    public void OnClickSlot()
    {
       
        if (InventoryManager.Instance != null)
        {
            // 매니저로 신호 보내기
            InventoryManager.Instance.OnSlotClicked(this);
        }
        else
        {
            // 만약 매니저가 꺼져있거나 연결이 안 되어있다면 빨간색 에러를 띄웁니다.
            Debug.LogError("[에러] InventoryManager.Instance가 존재하지 않습니다! 매니저 스크립트가 켜져 있는지 확인하세요.");
        }
    }

    // [추가] 이 슬롯이 선택되었을 때 배경색을 바꿔주는 함수
    public void HighlightSlot(bool isSelected)
    {
        if (backgroundImage != null)
        {
            // 선택되면 노란색, 해제되면 하얀색(기본값)으로 바꿉니다.
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
        UpdateSlotUI();

        // 아이템을 버리거나 써서 슬롯이 비워질 때, 툴팁 잔상이 남지 않도록 꺼줍니다.
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 빈 슬롯이 아닐 때만 툴팁을 띄웁니다!
        if (item != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowTooltip(item.itemName, item.description);
        }
    }

    // 마우스 커서가 이 슬롯 영역 밖으로 빠져나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        // 인벤토리 창이 닫혀서 이 슬롯이 비활성화될 때, 
        // 켜져 있는 툴팁이 있다면 강제로 꺼버립니다!
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }
}