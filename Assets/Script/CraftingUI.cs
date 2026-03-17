using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

    [Header("Ingredients 영역 (미리 배치한 5개 슬롯)")]
    // 하이어라키의 Item Slot들을 순서대로 드래그해서 넣으세요 (Element 0~4)
    public IngredientSlot[] ingredientSlots;

    [Header("제작 연출 요소")]
    public Slider craftSlider;
    public Text buttonText;
    public Button craftButton;

    [Header("레시피 설정")]
    public List<CraftingRecipe> allRecipes;
    public GameObject recipeButtonPrefab;
    public Transform recipeParent;

    private CraftingRecipe _selectedRecipe;
    private bool _isCrafting = false; // 현재 제작 게이지 차는 중인가?
    private bool _isFinished = false; // 제작 완료 후 수령 대기 중인가?

    void Awake() => Instance = this;

    void Start()
    {
        foreach (var slot in ingredientSlots) slot.gameObject.SetActive(false);
        SetIconAlpha(0);
        GenerateAllRecipes();

        // 2. 추가된 로직: 시작 시 제작창 패널을 비활성화하고 내부 상태를 초기화합니다.
        SetCraftingUIState(false);
    }

    void Update()
    {
        // 열려있는 제작대의 실시간 진행 상태를 UI에 반영
        if (uiPanel.activeSelf && _targetStation != null && _targetStation.isCrafting)
        {
            float duration = _targetStation.currentRecipe.craftTime;
            float elapsed = _targetStation.elapsedTime;

            if (craftSlider != null) craftSlider.value = elapsed;
            if (buttonText != null) buttonText.text = $"{(duration - elapsed):F1}s";
        }
    }

    // --- UI 상태 관리 ---
    public void SetCraftingUIState(bool isOpen)
    {
        _isCraftingUIOpen = isOpen;
        if (uiPanel != null) uiPanel.SetActive(isOpen);

        if (!isOpen) ResetStateUI(); // 닫을 때 상태 초기화

        // ✨ 추가된 핵심 코드: UI가 켜지거나 꺼질 때 UIManager에게 마우스 상태를 새로고침하라고 지시합니다.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateInputState();
        }
    }

    public void OpenCraftingUI(CraftingStation station = null)
    {
        _targetStation = station;

        // 1. 제작창 패널 활성화
        SetCraftingUIState(true);

        // 2. ✨ 추가된 로직: 인벤토리 매니저를 통해 인벤토리 UI를 함께 엽니다.
        if (InventoryManager.Instance != null && !InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.SetInventoryState(true);
        }

        // 3. 기존 레시피 및 상태 갱신 로직
        if (_targetStation != null && _targetStation.currentRecipe != null)
        {
            SelectRecipe(_targetStation.currentRecipe);

            if (_targetStation.isFinished)
            {
                if (craftSlider != null) craftSlider.value = _targetStation.currentRecipe.craftTime;
                if (buttonText != null) buttonText.text = "COLLECT";
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

    // --- 레시피 목록 생성 ---
    public void GenerateAllRecipes()
    {
        foreach (Transform child in recipeParent) Destroy(child.gameObject);
        foreach (var recipe in allRecipes)
        {
            GameObject btnGo = Instantiate(recipeButtonPrefab, recipeParent);
            if (btnGo.TryGetComponent(out RecipeButton recipeBtn)) recipeBtn.Setup(recipe);
        }
    }

    // --- 레시피 선택 ---
    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (_isCrafting || _isFinished) return; // 제작 중엔 변경 금지

        _selectedRecipe = recipe;

        // 결과물 정보 세팅
        if (resultPreviewIcon != null) resultPreviewIcon.texture = recipe.resultItem.iconTexture;
        if (resultNameText != null) resultNameText.text = recipe.resultItem.itemName;

        SetIconAlpha(1);       // 아이콘 표시
        UpdateIngredientsUI(); // 5개 슬롯 갱신
        ResetStateUI();        // 버튼 및 슬라이더 초기화
    }

    // --- 재료 슬롯 업데이트 (최적화: 미리 만들어둔 5개 재사용) ---
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

    // --- 제작 로직 ---
    public void OnCraftButtonClicked()
    {
        if (_targetStation == null || _selectedRecipe == null) return;

        if (_targetStation.isFinished)
        {
            CollectItem();
        }
        else if (!_targetStation.isCrafting && CanCraft(_selectedRecipe, 1))
        {
            // 재료 소모 후 제작대에게 제작 시작 명령
            ConsumeIngredients(_selectedRecipe, 1);
            UpdateIngredientsUI();

            _targetStation.StartCrafting(_selectedRecipe);

            if (craftSlider != null) craftSlider.maxValue = _selectedRecipe.craftTime;
            craftButton.interactable = false;
        }
    }

    IEnumerator CraftRoutine()
    {
        _isCrafting = true;
        craftButton.interactable = false;

        ConsumeIngredients(_selectedRecipe, 1);
        UpdateIngredientsUI(); // 소모 후 재료 수량 실시간 반영

        float elapsed = 0f;
        float duration = _selectedRecipe.craftTime;

        if (craftSlider != null)
        {
            craftSlider.maxValue = duration;
            craftSlider.value = 0;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (craftSlider != null) craftSlider.value = elapsed;
            if (buttonText != null) buttonText.text = $"{(duration - elapsed):F1}s";
            yield return null;
        }

        _isCrafting = false;
        _isFinished = true;
        craftButton.interactable = true;
        if (buttonText != null) buttonText.text = "COLLECT";
    }

    void CollectItem()
    {
        if (InventoryManager.Instance.AddItem(_targetStation.currentRecipe.resultItem, _targetStation.currentRecipe.resultAmount))
        {
            _targetStation.Collect();
            ResetStateUI();
        }
    }

    private void ResetStateUI()
    {
        _isFinished = false;
        _isCrafting = false;

        if (craftSlider != null) craftSlider.value = 0;

        // 재료 체크 후 버튼 활성화 여부 결정
        CheckCanCraft();
    }

    private void CheckCanCraft()
    {
        if (_selectedRecipe == null) return;

        bool hasIngredients = CanCraft(_selectedRecipe, 1);

        if (craftButton != null)
        {
            craftButton.interactable = hasIngredients;
            if (buttonText != null)
                buttonText.text = hasIngredients ? "CRAFT" : "LACK MATERIALS";
        }
    }

    // --- 데이터 검증 및 소모 ---
    bool CanCraft(CraftingRecipe recipe, int count)
    {
        foreach (var ing in recipe.ingredients)
        {
            if (InventoryManager.Instance.GetItemCount(ing.item) < ing.amount * count) return false;
        }
        return true;
    }

    void ConsumeIngredients(CraftingRecipe recipe, int count)
    {
        foreach (var ing in recipe.ingredients)
            InventoryManager.Instance.RemoveItem(ing.item, ing.amount * count);
    }

    public void CloseUI() => SetCraftingUIState(false);
}