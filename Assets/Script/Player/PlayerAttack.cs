using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public GameObject attackBox;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.5f;

    // ✨ Input System의 액션을 인스펙터에서 직접 넣을 수 있게 해주는 마법의 변수입니다!
    [Header("입력 설정")]
    public InputActionReference attackAction;

    private bool isAttacking = false;

    void Start()
    {
        if (attackBox != null) attackBox.SetActive(false);
    }

    void Update()
    {
        // ✨ 매번 찾아올 필요 없이, 인스펙터에 넣은 액션이 눌렸는지만 바로 검사합니다.
        // (.action을 붙여서 실제 액션 데이터에 접근합니다)
        if (attackAction != null && attackAction.action.WasPressedThisFrame())
        {
            bool isUIOpen = UIManager.Instance != null && UIManager.Instance.IsAnyUIOpen();

            if (!isAttacking && !isUIOpen)
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