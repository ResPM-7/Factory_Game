using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // KeyControl 사용을 위해 추가

public class ActionbarManager : MonoBehaviour
{
    [Header("슬롯 설정")]
    public RectTransform[] slots;      // 액션바의 각 슬롯 10개
    public Image highlightImage;      // 선택된 칸을 보여줄 강조 이미지

    private KeyControl[] _hotbarKeys; // 숫자 키들을 담아둘 배열
    private int _currentSlotIndex = 0;

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

    void SelectSlot(int index)
    {
        if (slots == null || slots.Length <= index) return;

        _currentSlotIndex = index;

        if (highlightImage != null)
        {
            // Outline 오브젝트를 찾아 정확한 위치로 이동
            Transform outlineTransform = slots[index].Find("Outline");

            if (outlineTransform != null)
            {
                highlightImage.transform.position = outlineTransform.position;
            }
            else
            {
                highlightImage.transform.position = slots[index].position;
            }
        }

        Debug.Log($"현재 {index + 1}번 슬롯 선택됨");
    }
}