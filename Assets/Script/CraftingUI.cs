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

    [Header("수량 조절 UI")]
    public Slider quantitySlider;
    public Button plusButton;
    public Button minusButton;
    public Text quantityText;

    private int _currentCraftCount = 1;
    private int _maxCraftCount = 1;

    [Header("레시피 설정")]
    public GameObject recipeButtonPrefab;
    public Transform recipeParent;

    private CraftingRecipe _selectedRecipe;

    void Awake() => Instance = this;

    void Start()
    {
        foreach (var slot in ingredientSlots) slot.gameObject.SetActive(false);
        SetIconAlpha(0);

        if (CraftingManager.Instance != null)
        {
            GenerateRecipeButtons(CraftingManager.Instance.allRecipes);
        }

        if (!_isCraftingUIOpen) SetCraftingUIState(false);

        if (quantitySlider != null)
        {
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void Update()
    {
        if (uiPanel.activeSelf && _targetStation != null && _selectedRecipe != null)
        {
            if (_targetStation.isCrafting)
            {
                if (craftSlider != null)
                {
                    craftSlider.maxValue = _targetStation.currentRecipe.craftTime;
                    craftSlider.value = _targetStation.elapsedTime;
                }

                if (buttonText != null) buttonText.text = "취소";
                craftButton.interactable = true;

                if (resultNameText != null)
                    resultNameText.text = $"{_selectedRecipe.resultItem.itemName} ({_targetStation.completedCount} / {_targetStation.targetCraftCount})";
            }
            else if (_targetStation.isFinished)
            {
                if (craftSlider != null)
                {
                    craftSlider.maxValue = 1;
                    craftSlider.value = 1;
                }
                if (buttonText != null) buttonText.text = "수령";
                craftButton.interactable = true;

                if (resultNameText != null)
                    resultNameText.text = $"{_selectedRecipe.resultItem.itemName} ({_targetStation.completedCount}개 완성)";
            }
        }
    }

    public void OnPlusButtonClicked()
    {
        if (_currentCraftCount < _maxCraftCount) UpdateQuantity(_currentCraftCount + 1);
    }

    public void OnMinusButtonClicked()
    {
        if (_currentCraftCount > 1) UpdateQuantity(_currentCraftCount - 1);
    }

    private void OnSliderValueChanged(float value)
    {
        UpdateQuantity(Mathf.RoundToInt(value));
    }

    private void UpdateQuantity(int count)
    {
        _currentCraftCount = Mathf.Clamp(count, 1, Mathf.Max(1, _maxCraftCount));

        if (quantityText != null) quantityText.text = _currentCraftCount.ToString();
        if (quantitySlider != null) quantitySlider.SetValueWithoutNotify(_currentCraftCount);

        UpdateIngredientsUI();
        CheckCanCraft();
    }

    public void SetCraftingUIState(bool isOpen)
    {
        if (isOpen && !gameObject.activeSelf) gameObject.SetActive(true);

        _isCraftingUIOpen = isOpen;
        if (uiPanel != null) uiPanel.SetActive(isOpen);
        if (!isOpen) ResetStateUI();
        if (UIManager.Instance != null) UIManager.Instance.UpdateInputState();
    }

    public void OpenCraftingUI(CraftingStation station = null)
    {
        if (InventoryManager.Instance != null && !InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.SetInventoryState(true);
        }

        _targetStation = station;
        SetCraftingUIState(true);

        if (_targetStation != null)
        {
            if (_targetStation.currentRecipe != null)
            {
                // ✨ 1. 제작대가 이미 일하고 있을 때 (강제로 레시피 띄움)
                SelectRecipe(_targetStation.currentRecipe, forceUpdate: true);

                if (_targetStation.isFinished)
                {
                    if (craftSlider != null) { craftSlider.maxValue = 1; craftSlider.value = 1; }
                    if (buttonText != null) buttonText.text = "수령";
                    if (craftButton != null) craftButton.interactable = true;
                    ToggleQuantityUI(false);
                }
                else if (_targetStation.isCrafting)
                {
                    if (craftButton != null) craftButton.interactable = false;
                    ToggleQuantityUI(false);
                }
            }
            else
            {
                // ✨ 2. 아무것도 안 하는 '새 제작대'를 열었을 때!
                if (_selectedRecipe != null)
                {
                    // 이전에 보던 레시피가 있으면 잠금을 풀고 바로 띄워줍니다!
                    SelectRecipe(_selectedRecipe, forceUpdate: true);
                }
                else
                {
                    ResetStateUI();
                    ToggleQuantityUI(false);
                }
            }
        }
        else
        {
            ResetStateUI();
        }
    }

    private void ToggleQuantityUI(bool isOn)
    {
        if (plusButton != null) plusButton.interactable = isOn;
        if (minusButton != null) minusButton.interactable = isOn;
        if (quantitySlider != null) quantitySlider.interactable = isOn;
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

    // ✨ [수정됨] 매개변수에 forceUpdate가 추가되었습니다. 
    // 시스템이 자동으로 띄울 때는 잠금을 무시하고 화면을 세팅합니다!
    public void SelectRecipe(CraftingRecipe recipe, bool forceUpdate = false)
    {
        // 강제 업데이트가 아닌데(유저가 클릭했는데), 제작대가 일하고 있으면 변경 불가
        if (!forceUpdate && _targetStation != null && (_targetStation.isCrafting || _targetStation.isFinished)) return;

        _selectedRecipe = recipe;

        if (resultPreviewIcon != null) resultPreviewIcon.texture = recipe.resultItem.iconTexture;
        if (resultNameText != null) resultNameText.text = recipe.resultItem.itemName;
        SetIconAlpha(1);

        Vector3 craftPos = _targetStation != null ? _targetStation.transform.position : Vector3.zero;
        _maxCraftCount = CraftingManager.Instance.GetMaxCraftableCount(_selectedRecipe, craftPos);

        if (quantitySlider != null)
        {
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = Mathf.Max(1, _maxCraftCount);
        }

        ToggleQuantityUI(true); // ✨ 잠금 해제!
        UpdateQuantity(1);      // ✨ 1개로 초기화 및 버튼 재활성화!
        ResetStateUI();
    }

    void UpdateIngredientsUI()
    {
        if (_selectedRecipe == null) return;

        int requiredCount = _selectedRecipe.ingredients.Count;
        Vector3 craftPos = _targetStation != null ? _targetStation.transform.position : Vector3.zero;

        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (i < requiredCount)
            {
                ingredientSlots[i].gameObject.SetActive(true);
                int totalAvailable = CraftingManager.Instance.GetTotalAvailableItem(_selectedRecipe.ingredients[i].item, craftPos);
                int requiredTotal = _selectedRecipe.ingredients[i].amount * _currentCraftCount;
                ingredientSlots[i].Setup(_selectedRecipe.ingredients[i], totalAvailable, requiredTotal);
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
        else if (_targetStation.isCrafting)
        {
            _targetStation.CancelCrafting();
            ToggleQuantityUI(true); // 취소했으니 다시 UI 잠금 해제
            UpdateQuantity(1);      // 수량 1개로 초기화
        }
        else if (!_targetStation.isCrafting && CraftingManager.Instance.CanCraft(_selectedRecipe, _targetStation.transform.position, _currentCraftCount))
        {
            CraftingManager.Instance.ConsumeIngredients(_selectedRecipe, _targetStation.transform.position, _currentCraftCount);
            UpdateIngredientsUI();

            _targetStation.StartCrafting(_selectedRecipe, _currentCraftCount);
            ToggleQuantityUI(false);
        }
    }

    void CollectItem()
    {
        int totalAmountToGive = _targetStation.currentRecipe.resultAmount * _targetStation.completedCount;

        bool added = CraftingManager.Instance.TryCollectItem(_targetStation.currentRecipe.resultItem, totalAmountToGive);

        if (added)
        {
            _targetStation.Collect();
            SelectRecipe(_selectedRecipe, forceUpdate: true); // 수령 후 UI 즉시 초기화
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
        if (resultNameText != null && _selectedRecipe != null) resultNameText.text = _selectedRecipe.resultItem.itemName;
        CheckCanCraft();
    }

    private void CheckCanCraft()
    {
        if (_selectedRecipe == null) return;

        Vector3 craftPos = _targetStation != null ? _targetStation.transform.position : Vector3.zero;
        bool hasIngredients = CraftingManager.Instance.CanCraft(_selectedRecipe, craftPos, _currentCraftCount);

        if (craftButton != null && _targetStation != null && !_targetStation.isCrafting && !_targetStation.isFinished)
        {
            craftButton.interactable = hasIngredients;
            if (buttonText != null)
                buttonText.text = hasIngredients ? "제작" : "재료 부족";
        }
    }

    public void CloseUI() => SetCraftingUIState(false);
}