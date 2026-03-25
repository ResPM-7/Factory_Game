using UnityEngine;

public class CraftingStation : MonoBehaviour, IInteractable
{
    [Header("제작 상태")]
    public CraftingRecipe currentRecipe;

    public int targetCraftCount = 1; // 총 만들어야 할 목표 개수
    public int completedCount = 0;   // 현재까지 완성된 개수

    public float elapsedTime = 0f;
    public bool isCrafting = false;
    public bool isFinished = false;

    void Update()
    {
        if (isCrafting && currentRecipe != null)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= currentRecipe.craftTime)
            {
                elapsedTime -= currentRecipe.craftTime; // 초과된 시간은 다음 아이템 제작으로 이월
                completedCount++; // 완성 개수 1 증가

                // 목표 개수를 다 채웠으면 완전히 종료
                if (completedCount >= targetCraftCount)
                {
                    elapsedTime = 0f;
                    isCrafting = false;
                    isFinished = true;
                }
            }
        }
    }

    public void StartCrafting(CraftingRecipe recipe, int count)
    {
        currentRecipe = recipe;
        targetCraftCount = count;
        completedCount = 0;
        elapsedTime = 0f;
        isCrafting = true;
        isFinished = false;
    }

    //제작 중간에 취소하는 기능
    public void CancelCrafting()
    {
        if (!isCrafting) return;

        isCrafting = false;
        int uncraftedCount = targetCraftCount - completedCount; // 못 만든 개수 파악

        // 1. 아직 못 만든 개수만큼 재료를 환불해 줍니다!
        if (uncraftedCount > 0 && CraftingManager.Instance != null)
        {
            CraftingManager.Instance.RefundIngredients(currentRecipe, uncraftedCount);
        }

        // 2. 만약 1개라도 완성된 게 있다면 수령(Collect) 대기 상태로 넘깁니다.
        if (completedCount > 0)
        {
            isFinished = true;
        }
        else
        {
            // 한 개도 못 만들고 취소했으면 그냥 초기화
            Collect();
        }
    }

    public void Collect()
    {
        currentRecipe = null;
        targetCraftCount = 1;
        completedCount = 0;
        elapsedTime = 0f;
        isCrafting = false;
        isFinished = false;
    }

    public void Interact()
    {
        if (CraftingUI.Instance != null)
        {
            CraftingUI.Instance.OpenCraftingUI(this);
        }
    }
}