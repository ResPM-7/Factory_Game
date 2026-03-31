using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourceCollectionUI : MonoBehaviour, IUIPanel
{
    public static ResourceCollectionUI Instance;

    [Header("UI 컴포넌트")]
    public Text titleText;
    public Text amountText;
    public Text receiveButtonText; // 메시지 출력용 공유
    public RawImage mainIcon;
    public Slider productionSlider;
    public GameObject uiPanel;

    [Header("팰 슬롯 UI (통합 PalSlot 사용)")]
    // ✨ ResourceMonsterSlot 대신 PalSlot 배열을 사용합니다.
    public PalSlot[] monsterSlots;

    public ResourceCampObject TargetCamp { get; private set; }

    private string _originalButtonText = "수령";
    private bool _isDisplayingError = false;

    private void Awake() => Instance = this;

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        _originalButtonText = receiveButtonText.text; // 시작 시 버튼 텍스트 저장
    }

    void Update()
    {
        // UI가 켜져 있을 때만 슬라이더 실시간 업데이트
        if (uiPanel.activeSelf && TargetCamp != null)
        {
            productionSlider.value = TargetCamp.currentProgress;
        }
    }

    public void OpenUI(ResourceCampObject camp)
    {
        TargetCamp = camp;
        titleText.text = camp.campName;
        mainIcon.texture = camp.resourceItem.iconTexture;

        uiPanel.SetActive(true);
        RefreshDisplay();

        // 인벤토리와 팰 상자 동시 오픈
        InventoryManager.Instance.OpenInventoryForce();
        if (PalBoxManager.Instance != null) PalBoxManager.Instance.TogglePalBoxUI();

        UpdateMonsterSlots();
    }

    // ✨ 슬롯들의 UI 상태를 건물 데이터에 맞춰 갱신합니다.
    public void UpdateMonsterSlots()
    {
        if (TargetCamp == null) return;

        for (int i = 0; i < monsterSlots.Length; i++)
        {
            if (i < TargetCamp.assignedPals.Length)
            {
                // PalSlot의 Setup 함수를 호출하여 아이콘과 데이터를 연결합니다.
                monsterSlots[i].Setup(TargetCamp.assignedPals[i]);
            }
        }
    }

    // ✨ 팰 배치/해제 핵심 로직
    public void AssignPalToCamp(int index, PalData pal)
    {
        if (TargetCamp == null) return;

        // 1. 팰 해제 (pal이 null인 경우)
        if (pal == null)
        {
            TargetCamp.assignedPals[index] = null;
            UpdateMonsterSlots();
            return;
        }

        // 2. 팰 배치 (적성 검사)
        int workLevel = pal.GetWorkLevel(TargetCamp.campWorkType);

        if (workLevel > 0)
        {
            TargetCamp.assignedPals[index] = pal;
            UpdateMonsterSlots();
            Debug.Log($"<color=cyan>{pal.palName}</color> 배치 성공!");
        }
        else
        {
            // 적성 불일치 시 에러 처리
            PalBoxManager.Instance.ClearSelection();
            StopAllCoroutines();
            StartCoroutine(ShowErrorMessage("적성이 맞지 않는 팰입니다"));
        }
    }

    public void RefreshDisplay()
    {
        if (TargetCamp != null)
            amountText.text = $"x {TargetCamp.currentAmount}";
    }

    public void OnReceiveButtonClick()
    {
        if (TargetCamp == null || TargetCamp.currentAmount <= 0 || _isDisplayingError) return;

        bool isSuccess = InventoryManager.Instance.AddItem(TargetCamp.resourceItem, TargetCamp.currentAmount);

        if (isSuccess)
        {
            TargetCamp.currentAmount = 0;
            RefreshDisplay();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ShowErrorMessage("인벤토리 공간이 부족합니다"));
        }
    }

    IEnumerator ShowErrorMessage(string message)
    {
        _isDisplayingError = true;
        receiveButtonText.text = $"<color=red>{message}</color>";

        yield return new WaitForSeconds(2f);

        receiveButtonText.text = _originalButtonText;
        _isDisplayingError = false;
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);
        // 창 닫을 때 팰 상자 선택 상태가 남아있다면 해제
        if (PalBoxManager.Instance != null) PalBoxManager.Instance.ClearSelection();
    }
}