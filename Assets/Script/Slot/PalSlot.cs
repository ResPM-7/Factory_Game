using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PalSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotType { Inventory, Camp }

    [Header("슬롯 설정")]
    public SlotType slotType;
    public int slotIndex;
    public PalData storedPal;

    [Header("UI 연결")]
    public RawImage iconRawImage;
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    void Start() => HighlightSlot(false);

    public void Setup(PalData pal)
    {
        storedPal = pal;
        if (pal != null && iconRawImage != null)
        {
            iconRawImage.texture = pal.palIcon;
            // 알파값을 강제로 1로 설정 (완전 불투명)
            iconRawImage.color = Color.white;
            iconRawImage.enabled = true;

            // UI가 즉시 갱신되지 않을 때를 대비해 레이아웃 강제 갱신 (선택 사항)
            // UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        storedPal = null;
        if (iconRawImage != null)
        {
            iconRawImage.color = new Color(1, 1, 1, 0f);
            iconRawImage.enabled = false;
        }
    }

    public void UpdateSlotUI()
    {
        if (storedPal != null) Setup(storedPal);
        else ClearSlot();
    }

    // ✨ 문제의 그 부분! 꽉 채워 넣었습니다.
    public void OnClickSlot()
    {
        PalData draggingPal = PalBoxManager.Instance.GetSelectedPal();

        if (slotType == SlotType.Inventory)
        {
            PalBoxManager.Instance.OnPalSlotClicked(this);
        }
        else if (slotType == SlotType.Camp)
        {
            HandleCampInteraction(draggingPal);
        }
    }

    // ✨ 자원 수거함 상호작용 실제 로직
    private void HandleCampInteraction(PalData draggingPal)
    {
        // 1. 손에 팰을 들고 있을 때 (배치 또는 교체)
        if (draggingPal != null)
        {
            // 1-1. 자원 슬롯에 이미 팰이 있다면? (서로 교체하기 위해 원래 있던 애를 내 손으로 가져옴)
            if (storedPal != null)
            {
                PalData tempPal = storedPal; // 원래 있던 팰을 임시 저장
                ResourceCollectionUI.Instance.AssignPalToCamp(slotIndex, draggingPal); // 들고 있던 애를 캠프에 배치

                // 원래 있던 애를 다시 내 손(드래그 상태)으로 쥐어줍니다.
                PalBoxManager.Instance.RemoveSelectedPal(); // 손을 비우고
                PalBoxManager.Instance.ForceSelectPal(tempPal); // 원래 캠프에 있던 애를 강제로 쥐어줌 (아래 추가할 함수)
            }
            // 1-2. 자원 슬롯이 비어있다면? (그냥 내려놓기)
            else
            {
                ResourceCollectionUI.Instance.AssignPalToCamp(slotIndex, draggingPal);
                PalBoxManager.Instance.RemoveSelectedPal(); // 내 손은 빈손이 됨
            }

            OnPointerEnter(null);
        }
        // 2. 빈 손일 때, 이미 들어있는 팰을 클릭했을 때 (집어 들기!)
        else if (storedPal != null)
        {
            // ❌ 이전 코드: 상자로 강제로 돌려보냄
            // PalBoxManager.Instance.ReturnPalToBox(storedPal);

            // ⭕ 수정 코드: 상자로 보내지 않고 마우스에 쥐어줍니다!
            PalBoxManager.Instance.ForceSelectPal(storedPal); // 팰을 집어들고
            ResourceCollectionUI.Instance.AssignPalToCamp(slotIndex, null); // 캠프 슬롯은 비워줍니다.
            HideTooltip();
        }
    }

    public void HighlightSlot(bool isSelected)
    {
        if (backgroundImage != null) backgroundImage.color = isSelected ? selectedColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedPal != null && TooltipUI.Instance != null)
        {
            string desc = "";
            foreach (var work in storedPal.workSuitabilities)
                desc += $"{work.workType} Lv.{work.level}\n";
            TooltipUI.Instance.ShowTooltip(storedPal.palName, string.IsNullOrEmpty(desc) ? "작업 적성 없음" : desc.TrimEnd());
        }
    }

    public void OnPointerExit(PointerEventData eventData) => HideTooltip();
    private void OnDisable() => HideTooltip();
    private void HideTooltip() { if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip(); }
}