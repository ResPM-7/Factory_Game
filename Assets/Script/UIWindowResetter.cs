using UnityEngine;

// UI 패널에 붙여두면 켜질 때마다 원래 위치로 돌려놓는 스크립트
[RequireComponent(typeof(RectTransform))]
public class UIWindowResetter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _initialPosition;

    void Awake()
    {
        // 1. 내 UI의 RectTransform을 가져옵니다.
        _rectTransform = GetComponent<RectTransform>();

        // 2. 게임이 딱 시작될 때, 에디터에 배치해둔 '원래 위치'를 기억해둡니다.
        _initialPosition = _rectTransform.anchoredPosition;
    }

    // OnEnable은 이 UI가 SetActive(true)로 '켜질 때마다' 자동으로 실행됩니다!
    void OnEnable()
    {
        if (_rectTransform != null)
        {
            // 3. 창이 켜질 때마다, 아까 기억해둔 원래 위치로 돌려놓습니다.
            _rectTransform.anchoredPosition = _initialPosition;
        }
    }
}