using UnityEngine;

public class ResourceCampObject : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public string campName;
    public ItemData resourceItem;
    public WorkType campWorkType;
    public float baseProductionTime = 5f; // 기본 생산 시간

    // 1. 여기에 최대 보관 개수 변수를 추가합니다! (유니티 인스펙터에서 수정 가능)
    public int maxCapacity = 1000;

    [Header("현재 상태")]
    public int currentAmount = 0;
    public float currentProgress = 0f;

    // 팰 배치 슬롯 (4칸)
    public PalData[] assignedPals = new PalData[4];

    void Update()
    {
        float totalWorkSpeed = GetTotalWorkSpeed();

        // 팰이 있고 적성이 맞을 때만 작동
        if (totalWorkSpeed > 0)
        {
            ProduceResource(totalWorkSpeed);
        }
    }

    private void ProduceResource(float speed)
    {
        // 2. 핵심 브레이크: 이미 꽉 찼다면 생산 로직을 여기서 바로 멈춥니다!
        if (currentAmount >= maxCapacity)
        {
            currentProgress = 0f; // 진행도도 초기화해서 깔끔하게 대기 상태로 만듦
            return;
        }

        // 슬라이더 진행
        currentProgress += (Time.deltaTime * speed) / baseProductionTime;

        if (currentProgress >= 1f)
        {
            currentProgress = 0f;
            currentAmount++; // 자원 1개 추가

            // 만약 UI가 켜져 있다면 화면 갱신
            if (ResourceCollectionUI.Instance != null && ResourceCollectionUI.Instance.TargetCamp == this)
            {
                ResourceCollectionUI.Instance.RefreshDisplay();
            }
        }
    }

    private float GetTotalWorkSpeed()
    {
        float totalLevelSum = 0f;
        foreach (var pal in assignedPals)
        {
            if (pal != null)
            {
                int workLevel = pal.GetWorkLevel(campWorkType);
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