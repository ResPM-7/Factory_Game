using UnityEngine;
using UnityEngine.UI;

public class IngredientSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public RawImage icon;
    public Text stackText;

    private void Awake()
    {
        SetAlpha(0);
    }

    // ✨ [수정] 제작 개수에 비례한 '총 필요량(requiredTotal)'을 같이 받습니다!
    public void Setup(CraftingRecipe.Ingredient ing, int totalAvailable, int requiredTotal)
    {
        if (icon != null)
        {
            icon.texture = ing.item.iconTexture;
            SetAlpha(1);
        }

        if (stackText != null)
        {
            // 예: "가진개수 / 총필요한개수"
            stackText.text = $"{totalAvailable}/{requiredTotal}";
            stackText.color = (totalAvailable >= requiredTotal) ? Color.white : Color.red;
        }
    }

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