using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    public Text itemNameText;
    public RawImage itemIcon; // ✨ 추가: 아이템 아이콘을 보여줄 RawImage
    private CraftingRecipe _recipe;

    public void Setup(CraftingRecipe recipe)
    {
        _recipe = recipe;

        // 1. 아이템 이름 표시
        if (itemNameText != null)
            itemNameText.text = recipe.resultItem.itemName;

        // 2. ✨ 아이템 아이콘 표시
        if (itemIcon != null && recipe.resultItem != null)
        {
            // 레시피 결과물의 아이콘 텍스처를 RawImage에 할당합니다.
            itemIcon.texture = recipe.resultItem.iconTexture;
        }
    }

    public void OnClickRecipe()
    {
        CraftingUI.Instance.SelectRecipe(_recipe);
    }
}