using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    // 무기의 공격력
    public int weaponDamage = 1;

    private void OnTriggerEnter(Collider other)
    {
        // ✨ 핵심 수정: IInteractable이 아니라 IDamageable을 찾습니다!
        if (other.TryGetComponent(out IDamageable damageableTarget))
        {
            Debug.Log($"💥 타격 적중! 대상: {other.gameObject.name} 에게 {weaponDamage}의 데미지!");

            // 찾은 대상에게 데미지를 입힙니다! (나무의 TakeDamage가 실행됨)
            damageableTarget.TakeDamage(weaponDamage);

            // 다단 히트 방지를 위해 공격 박스를 즉시 끕니다.
            gameObject.SetActive(false);
        }
    }
}