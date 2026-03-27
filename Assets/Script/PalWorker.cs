using UnityEngine;

public class PalWorker : MonoBehaviour
{
    [Header("나의 정보")]
    public PalData myData; // 에디터에서 슬라임 데이터나 거북이 데이터를 넣어줍니다.

    // 거점(Base)이나 제작대에서 몬스터에게 일을 시킬 때 호출하는 함수
    public void TryAssignWork(WorkType requiredWork)
    {
        if (myData == null) return;

        int myLevel = myData.GetWorkLevel(requiredWork);

        if (myLevel > 0)
        {
            Debug.Log($"<color=green>[작업 배정 성공]</color> {myData.palName}이(가) {requiredWork} 작업을 시작합니다! (능력치: Lv.{myLevel})");
            // 나중에 여기에 애니메이션 실행, NavMesh로 제작대로 이동하는 코드 등을 넣게 됩니다.
        }
        else
        {
            Debug.Log($"<color=red>[작업 배정 실패]</color> {myData.palName}은(는) {requiredWork} 작업을 할 줄 모릅니다.");
        }
    }
}