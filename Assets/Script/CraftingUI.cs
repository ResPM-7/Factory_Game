using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour, IUIPanel
{
    public static CraftingUI Instance;

    [Header("연결된 제작대")]
    private CraftingStation _targetStation;

    [Header("UI 패널 제어")]
    public GameObject uiPanel;
    private bool _isCraftingUIOpen = false;

    [Header("오른쪽 상단: 결과물")]
    public RawImage resultPreviewIcon;
    public Text resultNameText;

    [Header("Ingredients 영역")]
    public IngredientSlot[] ingredientSlots;

    [Header("제작 연출 요소")]
    public Slider craftSlider;
    public Text buttonText;
    public Button craftButton;

    [Header("레시피 설정")]
    public GameObject recipeButtonPrefab;
    public Transform recipeParent;

    private CraftingRecipe _selectedRecipe;

    void Awake() => Instance = this;

    void Start()
    {
        foreach (var slot in ingredientSlots) slot.gameObject.SetActive(false);
        SetIconAlpha(0);

        // 매니저에 등록된 레시피 목록을 가져와서 UI 버튼을 만듭니다.
        if (CraftingManager.Instance != null)
        {
            GenerateRecipeButtons(CraftingManager.Instance.allRecipes);
        }

        SetCraftingUIState(false);
    }

    void Update()
    {
        if (uiPanel.activeSelf && _targetStation != null)
        {
            // 제작대에서 진행 중인 시간을 UI에 갱신
            if (_targetStation.isCrafting)
            {
                float duration = _targetStation.currentRecipe.craftTime;
                float elapsed = _targetStation.elapsedTime;

                if (craftSlider != null) craftSlider.value = elapsed;
                if (buttonText != null) buttonText.text = $"{(duration - elapsed):F1}s";
            }
            // 제작이 막 끝났을 때
            else if (_targetStation.isFinished && craftButton.interactable == false)
            {
                if (craftSlider != null) craftSlider.value = _targetStation.currentRecipe.craftTime;
                if (buttonText != null) buttonText.text = "COLLECT";
                craftButton.interactable = true;
            }
        }
    }

    public void SetCraftingUIState(bool isOpen)
    {
        _isCraftingUIOpen = isOpen;
        if (uiPanel != null) uiPanel.SetActive(isOpen);

        if (!isOpen) ResetStateUI();

        if (UIManager.Instance != null) UIManager.Instance.UpdateInputState();
    }

    public void OpenCraftingUI(CraftingStation station = null)
    {
        _targetStation = station;
        SetCraftingUIState(true);

        // 제작창 열 때 인벤토리도 자동으로 같이 열기
        if (InventoryManager.Instance != null && !InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.SetInventoryState(true);
        }

        if (_targetStation != null && _targetStation.currentRecipe != null)
        {
            SelectRecipe(_targetStation.currentRecipe);

            if (_targetStation.isFinished)
            {
                if (craftSlider != null) craftSlider.value = _targetStation.currentRecipe.craftTime;
                if (buttonText != null) buttonText.text = "COLLECT";
                if (craftButton != null) craftButton.interactable = true;
            }
            else if (_targetStation.isCrafting)
            {
                if (craftButton != null) craftButton.interactable = false;
            }
        }
        else
        {
            ResetStateUI();
        }
    }

    private void SetIconAlpha(float alpha)
    {
        if (resultPreviewIcon != null)
        {
            Color c = resultPreviewIcon.color;
            c.a = alpha;
            resultPreviewIcon.color = c;
        }
    }

    public void GenerateRecipeButtons(List<CraftingRecipe> recipes)
    {
        foreach (Transform child in recipeParent) Destroy(child.gameObject);
        foreach (var recipe in recipes)
        {
            GameObject btnGo = Instantiate(recipeButtonPrefab, recipeParent);
            if (btnGo.TryGetComponent(out RecipeButton recipeBtn)) recipeBtn.Setup(recipe);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (_targetStation != null && (_targetStation.isCrafting || _targetStation.isFinished)) return;

        _selectedRecipe = recipe;

        if (resultPreviewIcon != null) resultPreviewIcon.texture = recipe.resultItem.iconTexture;
        if (resultNameText != null) resultNameText.text = recipe.resultItem.itemName;

        SetIconAlpha(1);
        UpdateIngredientsUI();
        ResetStateUI();
    }

    void UpdateIngredientsUI()
    {
        if (_selectedRecipe == null) return;

        int requiredCount = _selectedRecipe.ingredients.Count;

        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (i < requiredCount)
            {
                ingredientSlots[i].gameObject.SetActive(true);
                ingredientSlots[i].Setup(_selectedRecipe.ingredients[i]);
            }
            else
            {
                ingredientSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnCraftButtonClicked()
    {
        if (_targetStation == null || _selectedRecipe == null) return;

        if (_targetStation.isFinished)
        {
            CollectItem();
        }
        // ✨ 여기서 계산 로직을 CraftingManager에게 맡깁니다!
        else if (!_targetStation.isCrafting && CraftingManager.Instance.CanCraft(_selectedRecipe, 1))
        {
            // 재료 소모도 Manager가 합니다.
            CraftingManager.Instance.ConsumeIngredients(_selectedRecipe, 1);
            UpdateIngredientsUI();

            _targetStation.StartCrafting(_selectedRecipe);

            if (craftSlider != null) craftSlider.maxValue = _selectedRecipe.craftTime;
            craftButton.interactable = false;
        }
    }

    void CollectItem()
    {
        // ✨ 수령 로직도 Manager를 통해 시도합니다.
        bool added = CraftingManager.Instance.TryCollectItem(_targetStation.currentRecipe.resultItem, _targetStation.currentRecipe.resultAmount);

        if (added)
        {
            _targetStation.Collect();
            ResetStateUI();
        }
        else
        {
            Debug.Log("인벤토리가 가득 차서 아이템을 받을 수 없습니다!");
            if (buttonText != null) buttonText.text = "인벤 가득 참";
        }
    }

    private void ResetStateUI()
    {
        if (craftSlider != null) craftSlider.value = 0;
        CheckCanCraft();
    }

    private void CheckCanCraft()
    {
        if (_selectedRecipe == null) return;

        // 버튼을 켤지 말지도 Manager에게 물어보고 결정합니다.
        bool hasIngredients = CraftingManager.Instance.CanCraft(_selectedRecipe, 1);

        if (craftButton != null)
        {
            craftButton.interactable = hasIngredients;
            if (buttonText != null)
                buttonText.text = hasIngredients ? "CRAFT" : "재료부족";
        }
    }

    public void CloseUI() => SetCraftingUIState(false);
}