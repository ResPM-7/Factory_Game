using UnityEngine;

// 이 스크립트는 인벤토리의 도면이 아니라, "실제로 땅에 지어진 건물" 프리팹에 붙입니다.
public class PlacedBuilding : MonoBehaviour
{
    [Header("건물 정보")]
    public ItemData originalBlueprint; // 이 건물을 지을 때 사용한 도면 데이터

    // (선택) 철거 시 마우스를 올렸을 때 빨간색으로 빛나게 할 외형 렌더러들
    public Renderer[] buildingRenderers;
    private Color originalColor;

    void Start()
    {
        if (buildingRenderers.Length > 0 && buildingRenderers[0] != null)
        {
            // 나중에 원래 색으로 돌려놓기 위해 첫 번째 렌더러의 색상을 기억해둡니다.
            // (머티리얼이 복잡하다면 렌더러 배열 대신 별도의 하이라이트 시스템을 써도 좋습니다)
            originalColor = buildingRenderers[0].material.color;
        }
    }

    public void HighlightForDemolition(bool highlight)
    {
        foreach (var rend in buildingRenderers)
        {
            if (rend != null)
            {
                // 철거 모드에서 마우스를 올리면 반투명한 빨간색으로, 아니면 원래 색으로!
                rend.material.color = highlight ? new Color(1, 0, 0, 0.5f) : originalColor;
            }
        }
    }
}