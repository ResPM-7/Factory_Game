using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class BaseCamp : MonoBehaviour, IInteractable
{
    public static List<BaseCamp> activeBaseCamps = new List<BaseCamp>();

    [Header("거점 설정")]
    public float buildRadius = 50f; // 건축 가능 반경 (50m)

    [Header("경계선 시각화")]
    public int segments = 50;       // 원을 얼마나 부드럽게 그릴 것인가 (점의 개수)
    public float lineWidth = 0.3f;  // 선의 굵기

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void Start()
    {
        activeBaseCamps.Add(this);
        DrawBoundary();
    }

    private void OnDestroy()
    {
        activeBaseCamps.Remove(this);
    }

    // 선의 기본 옵션을 세팅합니다.
    private void SetupLineRenderer()
    {
        line.positionCount = segments;
        line.useWorldSpace = false; // 거점이 움직이면 선도 같이 따라가게 설정
        line.loop = true;           // 선의 시작과 끝을 자동으로 연결해 완벽한 원을 만듭니다.
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }

    // 수학(삼각함수)을 이용해 둥근 원형으로 점들을 찍어줍니다.
    private void DrawBoundary()
    {
        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * buildRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * buildRadius;

            // Y축을 0.2f로 설정해, 선이 바닥 그래픽에 파묻히지 않고 살짝 뜨게 만듭니다.
            line.SetPosition(i, new Vector3(x, 0.2f, z));

            angle += (360f / segments);
        }
    }

    public void Interact()
    {
        // 내 몸에 붙어있는 ChooseEntity 스크립트를 가져와서
        Debug.Log("★ Interact 함수가 호출되었습니다! ★"); // 이 로그가 찍히는지 확인
        ChooseEntity myEntity = GetComponent<ChooseEntity>();

        // 그게 무슨 타입인지 확인하고 작동시킵니다.
        if (myEntity != null && myEntity.entityType == EntityNames.MonsterBox)
        {
            Debug.Log("몬스터 박스 상호작용!");
            if (PalBoxManager.Instance != null)
            {
                PalBoxManager.Instance.TogglePalBoxUI();
            }
        }
    }
}