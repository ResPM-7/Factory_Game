using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CaptureBall : MonoBehaviour
{
    [HideInInspector]
    public GameObject originalPrefab; // 매니저에게 돌아갈 때 필요한 여권(원본 프리팹)

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 force)
    {
        // ✨ [Unity 6 최신 문법] 기존의 velocity 대신 linearVelocity를 사용합니다!
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.AddForce(force, ForceMode.Impulse);

        // 2초 뒤에 창고로 돌아가기 예약
        Invoke(nameof(ReturnToPool), 2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 몬스터와 부딪혔는지 검사 (CatchableMonster 스크립트가 있어야 작동)
        if (collision.gameObject.TryGetComponent(out PalController monster))
        {
            bool isCaught = PalBoxManager.Instance.CatchPal(monster.myData);

            if (isCaught)
            {
                Debug.Log($"<color=green>[포획 성공]</color> {monster.myData.palName} 획득!");
                monster.OnCaught(); // 몬스터 지우기
            }
            else
            {
                Debug.LogWarning("팰 상자가 가득 찼습니다!");
            }

            // 어딘가 부딪혔으니 2초 예약 취소하고 즉시 창고로 복귀
            CancelInvoke(nameof(ReturnToPool));
            ReturnToPool();
        }
    }

    // 파괴(Destroy) 대신 질문자님의 ItemPoolManager로 반환
    private void ReturnToPool()
    {
        if (originalPrefab != null && ItemPoolManager.Instance != null)
        {
            ItemPoolManager.Instance.ReturnPrefab(originalPrefab, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}