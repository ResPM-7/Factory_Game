using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // KeyControl 사용을 위해 추가

public class ActionbarManager : MonoBehaviour
{
    public static ActionbarManager Instance;

    [Header("슬롯 설정")]
    public RectTransform[] slots;      // 액션바의 각 슬롯 10개
    public Image highlightImage;      // 선택된 칸을 보여줄 강조 이미지

    private KeyControl[] _hotbarKeys; // 숫자 키들을 담아둘 배열
    private int _currentSlotIndex = 0;

    public ItemData SelectedItem { get; private set; }

    void Awake() => Instance = this; // 🚀 싱글톤 초기화

    void Start()
    {
        // 1. 키보드 키 할당 (1~9, 0 순서)
        var kb = Keyboard.current;
        if (kb != null)
        {
            _hotbarKeys = new[] {
                kb.digit1Key, kb.digit2Key, kb.digit3Key, kb.digit4Key, kb.digit5Key,
                kb.digit6Key, kb.digit7Key, kb.digit8Key, kb.digit9Key, kb.digit0Key
            };
        }

        // 2. UI 레이아웃 강제 업데이트 및 초기화
        Canvas.ForceUpdateCanvases();
        SelectSlot(0);
    }

    void Update()
    {
        if (_hotbarKeys == null) return;

        // 3. 반복문을 통해 어떤 숫자 키가 눌렸는지 체크
        for (int i = 0; i < _hotbarKeys.Length; i++)
        {
            if (_hotbarKeys[i].wasPressedThisFrame)
            {
                SelectSlot(i);
                break; // 키 하나가 눌렸으면 루프 종료
            }
        }
    }

    public void UpdateUI()
    {
        foreach (RectTransform slot in slots)
        {
            InventorySlot slotScript = slot.GetComponent<InventorySlot>();
            if (slotScript != null)
            {
                slotScript.UpdateSlotUI();
            }
        }

        SelectSlot(_currentSlotIndex);
    }

    void SelectSlot(int index)
    {
        if (slots == null || slots.Length <= index) return;

        _currentSlotIndex = index;

        //하이라이트 이동
        if (highlightImage != null)
        {
            Transform outlineTransform = slots[index].Find("Outline");
            highlightImage.transform.position = outlineTransform != null ? outlineTransform.position : slots[index].position;
        }

        // 2. ✨ 선택한 슬롯의 아이템 데이터를 가져옵니다.
        // (액션바의 UI 슬롯 오브젝트에 InventorySlot 스크립트가 붙어있다고 가정합니다)
        InventorySlot slotScript = slots[index].GetComponent<InventorySlot>();
        SelectedItem = (slotScript != null) ? slotScript.item : null;

        // 아이템이 슬롯에 존재한다면?
        if (SelectedItem != null && SelectedItem.isBuildable)
        {
            if (BuildManager.Instance != null) BuildManager.Instance.StartBuilding(SelectedItem);
        }
        else
        {
            if (BuildManager.Instance != null) BuildManager.Instance.CancelBuilding();
        }
    }
}