using UnityEngine;

public class CraftingStation : MonoBehaviour, IInteractable
{
    [Header("제작 상태")]
    public CraftingRecipe currentRecipe;
    public float elapsedTime = 0f;
    public bool isCrafting = false;
    public bool isFinished = false;

    void Update()
    {
        // 배경에서 제작 진행
        if (isCrafting && currentRecipe != null)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= currentRecipe.craftTime)
            {
                elapsedTime = currentRecipe.craftTime;
                isCrafting = false;
                isFinished = true;
            }
        }
    }

    public void StartCrafting(CraftingRecipe recipe)
    {
        currentRecipe = recipe;
        elapsedTime = 0f;
        isCrafting = true;
        isFinished = false;
    }

    public void Collect()
    {
        currentRecipe = null;
        elapsedTime = 0f;
        isFinished = false;
    }

    public void Interact()
    {
        // UI를 열 때 이 제작대(this)의 정보를 넘겨줍니다.
        if (CraftingUI.Instance != null)
        {
            CraftingUI.Instance.OpenCraftingUI(this);
        }
    }
}