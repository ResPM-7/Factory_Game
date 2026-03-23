using UnityEngine;

public class ResourceNode : MonoBehaviour, IDamageable
{
    [Header("채집 (드롭) 데이터")]
    // ✨ GameObject(프리팹) 대신 ItemData를 넣도록 변경했습니다!
    public ItemData dropItemData;
    public int dropCount = 3;         // 부서질 때 몇 개를 떨어뜨릴 것인가?
    public float dropForce = 2f;      // 튀어 오르는 힘 (수치가 클수록 멀리 날아감)

    [Header("자원 상태")]
    public int maxHP = 5;
    private int currentHP;

    private ResourceSpawner mySpawner;

    private void Start()
    {
        currentHP = maxHP;
        mySpawner = GetComponentInParent<ResourceSpawner>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log($"나무 타격! 남은 HP: {currentHP}");

        if (currentHP <= 0)
        {
            // 체력이 0이 되면 아이템을 흩뿌리는 함수를 실행합니다!
            DropItems();

            if (mySpawner != null) mySpawner.StartRespawnTimer(this, 10f);
            gameObject.SetActive(false);
        }
    }

    // --- 아이템을 사방으로 흩뿌리는 핵심 로직 ---
    private void DropItems()
    {
        // 데이터가 안 들어가 있으면 에러를 막기 위해 실행하지 않습니다.
        if (dropItemData == null) return;

        for (int i = 0; i < dropCount; i++)
        {
            // 1. 생성 위치
            Vector3 spawnPos = transform.position + (Vector3.up * 1.5f);

            // ✨ 2. 핵심 수정: Instantiate 대신 풀 매니저의 GetItem을 호출하여 재활용 창고에서 꺼내옵니다!
            GameObject droppedObj = ItemPoolManager.Instance.GetItem(dropItemData, spawnPos, Quaternion.identity);

            // 3. 물리(Rigidbody)를 가져와서 힘을 가합니다.
            if (droppedObj.TryGetComponent(out Rigidbody rb))
            {
                // 💡 [매우 중요] 재활용된 오브젝트는 이전에 날아가던 힘을 기억할 수 있으므로 0으로 초기화합니다.
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // X, Z축(좌우, 앞뒤)으로는 랜덤하게, Y축(위)으로는 솟구치게 방향 설정
                Vector3 randomDir = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-0.5f, 0.5f)
                ).normalized;

                // 뻥! 하고 차는 느낌(Impulse)으로 밀어냅니다.
                rb.AddForce(randomDir * dropForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * dropForce, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("떨어진 아이템 프리팹에 Rigidbody가 없어서 날아가지 않습니다!");
            }
        }
    }

    public void ResetResource()
    {
        currentHP = maxHP;
    }
}