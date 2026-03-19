using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("모든 레시피 관리")]
    public List<CraftingRecipe> allRecipes; // 에디터에서 모든 레시피를 여기에 넣습니다.

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 데이터 검증 및 소모 로직 ---

    // 1. 제작 가능 여부 확인
    public bool CanCraft(CraftingRecipe recipe, int count = 1)
    {
        if (recipe == null || InventoryManager.Instance == null) return false;

        foreach (var ing in recipe.ingredients)
        {
            if (InventoryManager.Instance.GetItemCount(ing.item) < ing.amount * count)
            {
                return false;
            }
        }
        return true;
    }

    // 2. 인벤토리에서 재료 소모
    public void ConsumeIngredients(CraftingRecipe recipe, int count = 1)
    {
        if (recipe == null || InventoryManager.Instance == null) return;

        foreach (var ing in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(ing.item, ing.amount * count);
        }
    }

    // 3. 완성된 아이템 인벤토리에 추가 시도
    public bool TryCollectItem(ItemData item, int amount)
    {
        if (item == null || InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.AddItem(item, amount);
    }
}