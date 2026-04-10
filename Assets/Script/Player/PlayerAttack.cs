using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public GameObject attackBox;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.5f;

    [Header("입력 설정")]
    public InputActionReference attackAction;

    private bool isAttacking = false;

    void Start()
    {
        if (attackBox != null) attackBox.SetActive(false);
    }

    void Update()
    {
        // 1. 교통정리 (이 조건들 중 하나라도 해당되면 공격 로직을 막습니다)

        // UI가 열려있으면 막음
        bool isUIOpen = UIManager.Instance != null && UIManager.Instance.IsAnyUIOpen();
        if (isUIOpen) return;

        // 건축 모드 중이면 막음
        if (BuildManager.Instance != null && BuildManager.Instance.isBuildingMode) return;

        // ✨ [핵심] 우클릭으로 조준(Aiming) 중일 때는 '공 던지기'에 좌클릭을 양보합니다!
        if (CameraManager.Instance != null && CameraManager.Instance.isAiming) return;


        // 2. 공격 실행
        if (attackAction != null && attackAction.action.WasPressedThisFrame())
        {
            if (!isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (attackBox != null) attackBox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackBox != null) attackBox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown - attackDuration);

        isAttacking = false;
    }
}