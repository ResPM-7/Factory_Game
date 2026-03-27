using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PalSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 수량(Amount) 변수 삭제! 팰은 무조건 1마리입니다.
    public PalData storedPal;

    [Header("UI 연결")]
    public RawImage iconRawImage;

    [Header("Selection UI")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    void Start()
    {
        HighlightSlot(false);
    }

    // Setup 함수에서도 수량(count)을 받지 않도록 수정했습니다.
    public void Setup(PalData pal)
    {
        storedPal = pal;

        if (pal != null && iconRawImage != null)
        {
            iconRawImage.texture = pal.palIcon;
            Color c = iconRawImage.color;
            c.a = 1f;
            iconRawImage.color = c;
            iconRawImage.enabled = true;
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
            Color c = iconRawImage.color;
            c.a = 0f;
            iconRawImage.color = c;
            iconRawImage.enabled = false;
        }
    }

    public void HighlightSlot(bool isSelected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    public void OnClickPalSlot()
    {
        if (storedPal != null && PalBoxManager.Instance != null)
        {
            PalBoxManager.Instance.OnPalClicked(storedPal);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedPal != null && TooltipUI.Instance != null)
        {
            string desc = "";
            foreach (var work in storedPal.workSuitabilities)
            {
                desc += $"{work.workType} Lv.{work.level}\n";
            }
            if (desc == "") desc = "작업 적성 없음";

            TooltipUI.Instance.ShowTooltip(storedPal.palName, desc.TrimEnd());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }
}