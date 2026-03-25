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

    public BaseCamp GetBaseCampAt(Vector3 position)
    {
        foreach (var camp in BaseCamp.activeBaseCamps)
        {
            // 제작대 위치가 특정 거점의 중심(position)에서 반지름(buildRadius) 안에 있다면?
            if (Vector3.Distance(position, camp.transform.position) <= camp.buildRadius)
            {
                return camp; // 그 거점을 반환합니다!
            }
        }
        return null; // 거점 밖이라면 null 반환
    }

    public int GetTotalAvailableItem(ItemData item, Vector3 craftingPosition)
    {
        int total = 0;

        // 1. 플레이어 가방 검사
        if (InventoryManager.Instance != null)
            total += InventoryManager.Instance.GetItemCount(item);

        // ✨ 2. 주변 상자 검사 (이제 BaseCamp의 실제 범위를 사용합니다!)
        BaseCamp currentCamp = GetBaseCampAt(craftingPosition);

        // 제작대가 거점 안에 있을 때만 상자 공유 기능을 켭니다.
        if (currentCamp != null && StorageChest.allChests != null)
        {
            foreach (var chest in StorageChest.allChests)
            {
                // 상자가 "제작대가 속한 바로 그 거점"의 반지름(buildRadius) 안에 있는지 검사!
                if (Vector3.Distance(currentCamp.transform.position, chest.transform.position) <= currentCamp.buildRadius)
                {
                    total += chest.GetItemCount(item);
                }
            }
        }

        return total;
    }

    // 1. 제작 가능 여부 확인
    public bool CanCraft(CraftingRecipe recipe, Vector3 craftingPosition, int count = 1)
    {
        if (recipe == null) return false;

        foreach (var ing in recipe.ingredients)
        {
            int requiredAmount = ing.amount * count;
            int availableAmount = GetTotalAvailableItem(ing.item, craftingPosition);

            if (availableAmount < requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    // 2. 인벤토리에서 재료 소모
    public void ConsumeIngredients(CraftingRecipe recipe, Vector3 craftingPosition, int count = 1)
    {
        if (recipe == null) return;

        foreach (var ing in recipe.ingredients)
        {
            int amountToTake = ing.amount * count;

            // 1) 플레이어 가방에서 먼저 빼기
            if (InventoryManager.Instance != null)
            {
                int playerHas = InventoryManager.Instance.GetItemCount(ing.item);
                int takeFromPlayer = Mathf.Min(amountToTake, playerHas);

                if (takeFromPlayer > 0)
                {
                    InventoryManager.Instance.RemoveItem(ing.item, takeFromPlayer);
                    amountToTake -= takeFromPlayer;
                }
            }

            // 2) 아직 모자라다면 50m 내 상자 털기
            if (amountToTake > 0 && StorageChest.allChests != null)
            {
                foreach (var chest in StorageChest.allChests)
                {
                    if (Vector3.Distance(craftingPosition, chest.transform.position) <= 50f)
                    {
                        int chestHas = chest.GetItemCount(ing.item);
                        int takeFromChest = Mathf.Min(amountToTake, chestHas);

                        if (takeFromChest > 0)
                        {
                            chest.RemoveItem(ing.item, takeFromChest);
                            amountToTake -= takeFromChest;
                        }

                        if (amountToTake <= 0) break;
                    }
                }
            }
        }
    }

    public int GetMaxCraftableCount(CraftingRecipe recipe, Vector3 craftingPosition)
    {
        if (recipe == null || recipe.ingredients.Count == 0) return 0;

        int maxCount = 9999; // 일단 엄청 큰 수로 시작

        foreach (var ing in recipe.ingredients)
        {
            int available = GetTotalAvailableItem(ing.item, craftingPosition);
            int craftable = available / ing.amount; // 이 재료로 만들 수 있는 개수

            if (craftable < maxCount)
            {
                maxCount = craftable; // 가장 부족한 재료를 기준으로 최대 개수가 정해집니다.
            }
        }
        return maxCount;
    }

    public void RefundIngredients(CraftingRecipe recipe, int uncraftedCount)
    {
        if (recipe == null || InventoryManager.Instance == null) return;

        foreach (var ing in recipe.ingredients)
        {
            int amountToRefund = ing.amount * uncraftedCount;
            // 상자에서 뺀 것들도 일단 안전하게 플레이어 가방으로 환불해 줍니다.
            InventoryManager.Instance.AddItem(ing.item, amountToRefund);
        }
        //Debug.Log($"<color=green>[환불 완료]</color> 제작 취소로 인해 {uncraftedCount}개 분량의 재료가 반환되었습니다.");
    }

    // 3. 완성된 아이템 인벤토리에 추가 시도
    public bool TryCollectItem(ItemData item, int amount)
    {
        if (item == null || InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.AddItem(item, amount);
    }
}