using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PalBoxManager : MonoBehaviour
{
    public static PalBoxManager Instance;

    [Header("저장된 팰 목록 ")]
    [HideInInspector]
    public PalData[] storedPals = new PalData[50];
    public int maxCapacity = 50;

    [Header("UI 연결")]
    public GameObject palBoxUI;
    public PalSlot[] palSlots;

    [Header("드래그 UI (아이템과 같은 방식)")]
    public GameObject dragObject;
    public RawImage dragIcon;

    // ✨ 슬롯 전체가 아니라 현재 내가 들고 있는 '팰 데이터' 자체를 기억하게 분리했습니다.
    private PalSlot _selectedPalSlot = null; // 상자에서 뽑았을 때 원래 슬롯을 기억
    private PalData _draggingPalData = null; // 현재 마우스에 들고 있는 팰의 실체

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (palBoxUI != null) palBoxUI.SetActive(false);

        if (storedPals == null || storedPals.Length != maxCapacity)
            storedPals = new PalData[maxCapacity];

        for (int i = 0; i < palSlots.Length; i++)
        {
            if (palSlots[i] != null)
            {
                palSlots[i].slotIndex = i;
                if (i < storedPals.Length && palSlots[i].storedPal != null)
                {
                    storedPals[i] = palSlots[i].storedPal;
                }
            }
        }
        UpdateUI();
    }

    void Update()
    {
        // _draggingPalData가 있을 때 마우스를 따라갑니다.
        if (_draggingPalData != null && dragObject != null && dragObject.activeSelf)
        {
            dragObject.transform.position = Mouse.current.position.ReadValue();
        }
    }

    public void OnPalSlotClicked(PalSlot clickedSlot)
    {
        // 1. 아무것도 안 들고 있을 때 -> 팰 상자에서 집어들기
        if (_draggingPalData == null)
        {
            if (clickedSlot.storedPal == null) return;

            _selectedPalSlot = clickedSlot;
            _draggingPalData = clickedSlot.storedPal; // 데이터 장전!
            _selectedPalSlot.HighlightSlot(true);

            if (dragObject != null)
            {
                dragObject.SetActive(true);
                dragIcon.texture = _draggingPalData.palIcon;
                _selectedPalSlot.iconRawImage.enabled = false;
            }
        }
        // 2. 이미 뭔가를 들고 있을 때 (상자 출신이든 캠프 출신이든)
        else
        {
            // 상자에서 뽑았던 놈을 같은 자리에 다시 놓을 때
            if (_selectedPalSlot == clickedSlot)
            {
                ClearSelection();
            }
            else
            {
                // 클릭한 칸에 무언가 있다면 Swap, 없다면 그냥 넣기
                if (clickedSlot.storedPal != null)
                {
                    SwapPalDataWithDragging(clickedSlot);
                }
                else
                {
                    PlacePalInEmptySlot(clickedSlot);
                }
            }
        }
    }

    // 마우스에 들고 있는 팰과 슬롯에 있는 팰을 교환
    private void SwapPalDataWithDragging(PalSlot targetSlot)
    {
        PalData temp = targetSlot.storedPal;

        // 타겟 슬롯에 내가 들고있던 놈 꽂아줌
        storedPals[targetSlot.slotIndex] = _draggingPalData;
        targetSlot.Setup(_draggingPalData);

        // 만약 원래 상자에서 뽑았던 놈이라면
        if (_selectedPalSlot != null)
        {
            storedPals[_selectedPalSlot.slotIndex] = temp;
            _selectedPalSlot.Setup(temp);
            ClearSelection();
        }
        // 원래 캠프에서 뽑아온 외부 인력이었다면
        else
        {
            // 상자에 있던 놈을 새로 내 손에 쥐어줌 (스왑 완료)
            ForceSelectPal(temp);
        }
    }

    // 빈 슬롯에 팰을 내려놓음
    private void PlacePalInEmptySlot(PalSlot targetSlot)
    {
        // 타겟 슬롯에 배치
        storedPals[targetSlot.slotIndex] = _draggingPalData;
        targetSlot.Setup(_draggingPalData);

        // 만약 상자에서 뽑았던 놈이 이동하는 거라면 원래 자리 비움
        if (_selectedPalSlot != null)
        {
            storedPals[_selectedPalSlot.slotIndex] = null;
            _selectedPalSlot.ClearSlot();
        }

        ClearSelection();
    }

    // 캠프에서 팰을 꺼내 바로 손에 쥐어주는 기능
    public void ForceSelectPal(PalData palToSelect)
    {
        if (palToSelect == null) return;

        _selectedPalSlot = null; // 상자 출신이 아님
        _draggingPalData = palToSelect; // 데이터 장전

        if (dragObject != null)
        {
            dragObject.SetActive(true);
            dragIcon.texture = _draggingPalData.palIcon;
        }
    }

    public void ClearSelection()
    {
        if (_selectedPalSlot != null)
        {
            _selectedPalSlot.iconRawImage.enabled = true;
            _selectedPalSlot.HighlightSlot(false);
            _selectedPalSlot = null;
        }
        _draggingPalData = null; // 들고 있던 데이터 날림
        if (dragObject != null) dragObject.SetActive(false);
    }

    // 외부에서 현재 들고 있는 팰을 물어볼 때 리턴
    public PalData GetSelectedPal()
    {
        return _draggingPalData;
    }

    // 캠프에 배치완료 등 손에서 팰을 완전히 없앨 때
    public void RemoveSelectedPal()
    {
        // 상자에서 뽑았던 놈이면 상자 데이터에서도 삭제
        if (_selectedPalSlot != null)
        {
            storedPals[_selectedPalSlot.slotIndex] = null;
            _selectedPalSlot.ClearSlot();
        }
        ClearSelection();
    }

    public bool CatchPal(PalData newPal)
    {
        for (int i = 0; i < storedPals.Length; i++)
        {
            if (storedPals[i] == null)
            {
                storedPals[i] = newPal;
                Debug.Log($"<color=cyan>[팰 획득]</color> {newPal.palName}을(를) {i}번 칸에 저장했습니다!");
                UpdateUI();
                return true;
            }
        }
        Debug.LogWarning("Pal Chest가 가득 찼습니다! 더 이상 잡을 수 없습니다.");
        return false;
    }

    public void UpdateUI()
    {
        if (palSlots == null || palSlots.Length == 0) return;
        for (int i = 0; i < palSlots.Length; i++)
        {
            if (i < storedPals.Length) palSlots[i].Setup(storedPals[i]);
        }
    }

    public void TogglePalBoxUI()
    {
        if (palBoxUI == null) return;
        bool isCurrentlyActive = palBoxUI.activeSelf;
        palBoxUI.SetActive(!isCurrentlyActive);

        if (palBoxUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // UI 끌 때 들고 있던 팰 처리 (증발 방지)
            if (_draggingPalData != null)
            {
                ReturnPalToBox(_draggingPalData);
            }
            ClearSelection();
        }
    }

    public void ReturnPalToBox(PalData pal)
    {
        if (pal == null) return;
        CatchPal(pal);
        Debug.Log($"<color=yellow>[팰 상자 복귀]</color> {pal.palName}이(가) 다시 상자로 돌아왔습니다.");
    }
}