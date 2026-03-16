using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance;

    [Header("UI 패널 제어")]
    public GameObject uiPanel; // ✨ 추가: UIManager가 끄고 켤 전체 창 (Blacksmithy)
    private bool isCraftingUIOpen = false; // ✨ 추가: 상태 변수

    [Header("오른쪽 상단: 결과물")]
    public RawImage resultPreviewIcon;
    public TextMeshProUGUI resultNameText;

    [Header("오른쪽 하단: 재료 리스트 부모")]
    public Transform ingredientParent;
    public GameObject ingredientSlotPrefab;

    private CraftingRecipe _selectedRecipe;
    private int _craftAmount = 1;

    void Awake() => Instance = this;

    // ✨ 추가: UIManager가 호출할 상태 변경 함수
    public void SetCraftingUIState(bool isOpen)
    {
        isCraftingUIOpen = isOpen;
        if (uiPanel != null) uiPanel.SetActive(isOpen);
    }

    // ✨ 추가: 제작창을 열 때 사용 (E키 등에서 호출용)
    public void OpenCraftingUI()
    {
        SetCraftingUIState(true);
        // 제작할 때는 가방도 같이 보여야 편하니까 강제 오픈
        InventoryManager.Instance.OpenInventoryForce();
        // 조작 모드를 UI로 변경
        InventoryManager.Instance.UpdateInputState();
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        _selectedRecipe = recipe;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (_selectedRecipe == null) return;

        resultPreviewIcon.texture = _selectedRecipe.resultItem.iconTexture;
        resultNameText.text = _selectedRecipe.resultItem.itemName;

        foreach (Transform child in ingredientParent) Destroy(child.gameObject);

        foreach (var ing in _selectedRecipe.ingredients)
        {
            GameObject go = Instantiate(ingredientSlotPrefab, ingredientParent);
            // 여기서 재료 아이콘과 수량을 표시하는 자식 컴포넌트가 있다면 Setup 호출
            // go.GetComponent<IngredientSlot>().Setup(ing);
        }
    }

    public void OnCraftButtonClicked()
    {
        if (_selectedRecipe == null) return;

        if (CanCraft(_selectedRecipe, _craftAmount))
        {
            ConsumeIngredients(_selectedRecipe, _craftAmount);
            InventoryManager.Instance.AddItem(_selectedRecipe.resultItem, _selectedRecipe.resultAmount * _craftAmount);

            // 제작 후 재료 수량이 변했으니 UI 갱신
            UpdateUI();
            Debug.Log($"{_selectedRecipe.resultItem.itemName} 제작 완료!");
        }
        else
        {
            Debug.Log("재료가 부족합니다!");
        }
    }

    bool CanCraft(CraftingRecipe recipe, int count)
    {
        foreach (var ing in recipe.ingredients)
        {
            int myAmount = InventoryManager.Instance.GetItemCount(ing.item);
            if (myAmount < ing.amount * count) return false;
        }
        return true;
    }

    void ConsumeIngredients(CraftingRecipe recipe, int count)
    {
        foreach (var ing in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(ing.item, ing.amount * count);
        }
    }
}