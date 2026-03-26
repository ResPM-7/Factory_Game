using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("툴팁 패널 및 텍스트")]
    public GameObject tooltipPanel;
    public Text nameText;
    public Text descriptionText;

    [Header("마우스 커서와의 여백")]
    public Vector2 offset = new Vector2(15f, 15f); // 인스펙터에서 양수로 입력하셔도 코드가 알아서 방향을 맞춰줍니다.

    private RectTransform backgroundRect;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tooltipPanel != null)
        {
            backgroundRect = tooltipPanel.GetComponent<RectTransform>();
        }

        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf && backgroundRect != null)
        {
            Vector2 mousePos = Input.mousePosition;

            float tooltipWidth = backgroundRect.rect.width;
            float tooltipHeight = backgroundRect.rect.height;

            // X: 0 (왼쪽 기준 -> 오른쪽으로 늘어남)
            // Y: 1 (위쪽 기준 -> 아래로 늘어남)
            float pivotX = 0f;
            float pivotY = 1f;
            // 화면 오른쪽 밖으로 짤릴 위험이 있다면? -> 피벗을 오른쪽(1)으로 바꿔서 왼쪽으로 뻗게 함
            if (mousePos.x + tooltipWidth + offset.x > Screen.width)
            {
                pivotX = 1f;
            }

            // 화면 아래 밖으로 짤릴 위험이 있다면? -> 피벗을 아래(0)로 바꿔서 위로 뻗게 함
            if (mousePos.y - tooltipHeight - offset.y < 0)
            {
                pivotY = 0f;
            }

            // 3. 계산된 피벗 적용
            backgroundRect.pivot = new Vector2(pivotX, pivotY);

            // 4. 피벗 방향에 맞춰서 마우스에 가려지지 않게 오프셋 방향 자동 조절
            // pivotX가 0이면(오른쪽 뻗음) 오른쪽(+)으로, 1이면(왼쪽 뻗음) 왼쪽(-)으로 밀어줌
            float finalOffsetX = (pivotX == 0f) ? Mathf.Abs(offset.x) : -Mathf.Abs(offset.x);

            // pivotY가 1이면(아래로 뻗음) 아래(-)로, 0이면(위로 뻗음) 위(+)로 밀어줌
            float finalOffsetY = (pivotY == 1f) ? -Mathf.Abs(offset.y) : Mathf.Abs(offset.y);

            // 5. 최종 위치 적용
            transform.position = mousePos + new Vector2(finalOffsetX, finalOffsetY);
        }
    }

    public void ShowTooltip(string itemName, string itemDescription)
    {
        if (nameText != null) nameText.text = itemName;
        if (descriptionText != null) descriptionText.text = itemDescription;

        tooltipPanel.SetActive(true);

        if (backgroundRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
        }
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}