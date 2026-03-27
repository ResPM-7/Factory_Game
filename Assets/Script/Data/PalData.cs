using UnityEngine;
using System.Collections.Generic;

//구조체: 특정 작업과 그 레벨(속도)을 묶어줍니다.
[System.Serializable]
public struct WorkSuitability
{
    public WorkType workType;
    public int level; // 레벨이 높을수록 작업 속도가 빠름 (예: 벌목 1레벨, 2레벨...)
}

// ScriptableObject를 상속받아 유니티 에디터에서 파일로 생성할 수 있게 합니다.
[CreateAssetMenu(fileName = "New Pal Data", menuName = "Pal System/Pal Data")]
public class PalData : ScriptableObject
{
    [Header("몬스터 아이디")]
    public string palID; // "Wood", "Rock", "Iron"

    [Header("기본 정보")]
    public string palName;         // 이름 (슬라임, 거북이 등)
    public GameObject palPrefab;   // 소환할 3D 모델 프리팹
    public Texture palIcon;         // UI에 표시될 아이콘

    [Header("작업 적성 (Work Suitability)")]
    // 이 몬스터가 할 수 있는 작업들의 목록
    public List<WorkSuitability> workSuitabilities = new List<WorkSuitability>();

    // 특정 작업을 할 수 있는지, 한다면 레벨이 몇인지 확인하는 함수
    public int GetWorkLevel(WorkType type)
    {
        foreach (var w in workSuitabilities)
        {
            if (w.workType == type)
            {
                return w.level; // 할 줄 알면 레벨 반환
            }
        }
        return 0; // 목록에 없으면(0) 할 줄 모르는 작업임
    }
}