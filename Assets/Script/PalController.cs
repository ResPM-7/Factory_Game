using UnityEngine;

// [유니티 팁] 몬스터라면 무조건 팰 데이터가 있어야 하므로 강제합니다.
public class PalController : MonoBehaviour
{
    [Header("팰 핵심 정보")]
    public PalData myData; // 이제 여기서 데이터 하나만 관리하면 끝!

    // ==========================================
    // 1. 포획 관련 로직 (기존 CatchableMonster)
    // ==========================================
    public void OnCaught()
    {
        Debug.Log($"<color=cyan>[포획 연출]</color> {myData.palName}이(가) 사라집니다.");

        // 몬스터가 잡혔을 때 필드에서 제거
        Destroy(gameObject);
    }

    // ==========================================
    // 2. 작업 관련 로직 (기존 PalWorker)
    // ==========================================
    public void TryAssignWork(WorkType requiredWork)
    {
        if (myData == null) return;

        int myLevel = myData.GetWorkLevel(requiredWork);

        if (myLevel > 0)
        {
            Debug.Log($"<color=green>[작업 성공]</color> {myData.palName} (Lv.{myLevel}) -> {requiredWork} 시작!");
        }
        else
        {
            Debug.Log($"<color=red>[작업 불가]</color> {myData.palName}은 {requiredWork} 능력이 없습니다.");
        }
    }
}