using System.Collections;
using UnityEngine;

// 이 스크립트를 가진 빈 오브젝트(부모)가 자기 밑에 있는 자원들의 부활을 책임집니다.
public class ResourceSpawner : MonoBehaviour
{
    // 자원이 다 캐지면 부모인 나에게 "저 좀 이따 깨워주세요!"라고 알립니다.
    public void StartRespawnTimer(ResourceNode node, float delayTime)
    {
        StartCoroutine(RespawnRoutine(node, delayTime));
    }

    private IEnumerator RespawnRoutine(ResourceNode node, float delayTime)
    {
        // 지정된 시간만큼 부모가 대신 기다려줍니다.
        yield return new WaitForSeconds(delayTime);

        // 시간이 다 되면 자식의 체력을 채우고, 다시 짠! 하고 켭니다.
        node.ResetResource();
        node.gameObject.SetActive(true);

        Debug.Log($"🌱 부모 스포너가 [{node.gameObject.name}] 자원을 다시 살려냈습니다!");
    }
}