using UnityEngine;

public class ResourceCampObject : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public string campName;
    public ItemData resourceItem;
    public WorkType campWorkType;
    public float baseProductionTime = 5f; // 기본 생산 시간

    [Header("현재 상태")]
    public int currentAmount = 0;
    public float currentProgress = 0f;

    // 팰 배치 슬롯 (4칸)
    public PalData[] assignedPals = new PalData[4];

    void Update()
    {
        float totalWorkSpeed = GetTotalWorkSpeed();

        // (팰이 없거나, 있어도 적성이 없으면 0이 되어 멈춥니다.)
        if (totalWorkSpeed > 0)
        {
            ProduceResource(totalWorkSpeed);
        }
    }

    private void ProduceResource(float speed)
    {
        // 슬라이더 진행 (적성 레벨의 총합에 따라 차오름)
        currentProgress += (Time.deltaTime * speed) / baseProductionTime;

        if (currentProgress >= 1f)
        {
            currentProgress = 0f;
            currentAmount++;

            if (ResourceCollectionUI.Instance != null && ResourceCollectionUI.Instance.TargetCamp == this)
            {
                ResourceCollectionUI.Instance.RefreshDisplay();
            }
        }
    }

    // 핵심 수정: 팰의 적성 레벨에 따라 작업 속도를 계산합니다.
    private float GetTotalWorkSpeed()
    {
        float totalLevelSum = 0f;
        foreach (var pal in assignedPals)
        {
            if (pal != null)
            {
                // 팰 데이터에서 이 건물의 작업 타입에 맞는 레벨을 가져옵니다.
                // 벌목 레벨이 2라면 2를 반환, 없으면 0을 반환합니다.
                int workLevel = pal.GetWorkLevel(campWorkType);

                // 레벨이 0이면 더해도 0이므로 속도에 기여하지 않습니다.
                totalLevelSum += workLevel;
            }
        }
        return totalLevelSum;
    }

    public void Interact()
    {
        if (ResourceCollectionUI.Instance != null)
        {
            ResourceCollectionUI.Instance.OpenUI(this);
        }
    }
}