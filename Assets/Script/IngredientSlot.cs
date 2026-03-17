using UnityEngine;
using UnityEngine.UI;

public class IngredientSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public RawImage icon;
    public Text stackText;

    // ✨ 오브젝트가 처음 로드될 때 투명하게 초기화
    private void Awake()
    {
        SetAlpha(0);
    }

    public void Setup(CraftingRecipe.Ingredient ing)
    {
        // 1. 아이콘 설정 및 불투명하게(1) 변경
        if (icon != null)
        {
            icon.texture = ing.item.iconTexture;
            SetAlpha(1);
        }

        // 2. 가방에서 아이템 개수 가져오기
        int myCount = InventoryManager.Instance.GetItemCount(ing.item);

        // 3. 텍스트 설정
        if (stackText != null)
        {
            stackText.text = $"{myCount}/{ing.amount}";
            stackText.color = (myCount >= ing.amount) ? Color.white : Color.red;
        }
    }

    // ✨ 투명도 조절용 내부 함수
    private void SetAlpha(float alpha)
    {
        if (icon != null)
        {
            Color c = icon.color;
            c.a = alpha;
            icon.color = c;
        }
    }
}