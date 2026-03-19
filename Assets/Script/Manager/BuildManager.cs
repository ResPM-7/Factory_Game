using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("건축 상태")]
    public bool isBuildingMode = false;
    private ItemData currentBlueprintData;

    // ✨ [추가] 하이어라키에 미리 만들어둔 고스트들의 '부모 오브젝트'를 연결합니다.
    public GameObject ghostRoot;

    private Camera mainCamera;

    [Header("건축 설정")]
    public LayerMask groundLayer;
    public float maxBuildDistance = 15f;
    public bool useGridSnap = true;
    public float gridSize = 1f;

    private Vector3 _lastGhostPosition;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;

        // 시작할 때 고스트 루트는 무조건 꺼둡니다.
        if (ghostRoot != null) ghostRoot.SetActive(false);
    }

    void Update()
    {
        if (!isBuildingMode || currentBlueprintData == null || ghostRoot == null) return;

        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        if (mouse.rightButton.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelBuilding();
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * maxBuildDistance, Color.red);

        // 1. 레이저가 땅(Ground) 레이어에 맞았을 때
        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, groundLayer))
        {
            // ✨ [디버그 1] 맞은 위치 확인
            //Debug.Log($"🟢 [1. 레이저 적중] 레이저가 땅('{hit.collider.name}')에 맞았습니다! (좌표: {hit.point})");

            if (!ghostRoot.activeSelf)
            {
                ghostRoot.SetActive(true);
                //Debug.Log("🟢 [2. 부모 활성화] 부모 오브젝트(Blue_print)를 켰습니다.");
            }

            Vector3 targetPosition = hit.point;
            if (useGridSnap)
            {
                targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
            }

            // ✨ [디버그 3] 최종 이동 목표 좌표 확인
            //Debug.Log($"🟢 [3. 위치 계산] 청사진이 이동할 최종 좌표: {targetPosition}");

            if (_lastGhostPosition != targetPosition)
            {
                ghostRoot.transform.position = targetPosition;
                _lastGhostPosition = targetPosition;
                //Debug.Log($"🟢 [4. 고스트 이동] 청사진을 {targetPosition} (으)로 이동시켰습니다.");
            }

            // ✨ [디버그 5] 핵심! 켜져 있는 자식(도면)이 진짜로 있는지 검사합니다.
            int activeChildCount = 0;
            foreach (Transform child in ghostRoot.transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    activeChildCount++;
                    //Debug.Log($"🟢 [5. 자식 상태] 현재 눈에 보여야 할 청사진: {child.name}");
                }
            }

            // 켜진 자식이 하나도 없다면 치명적 오류 띄우기
            if (activeChildCount == 0)
            {
                //Debug.LogError("🔴 [오류] 부모(Blue_print)는 켜졌는데, 그 안에 켜져 있는 청사진(자식)이 하나도 없습니다! StartBuilding에서 자식을 못 켰습니다.");
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                PlaceBuilding(targetPosition);
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit anyHit, maxBuildDistance))
            {
                //Debug.LogWarning($"🔴 [레이저 막힘] 레이저가 땅이 아니라 '{anyHit.collider.name}' (레이어: {LayerMask.LayerToName(anyHit.collider.gameObject.layer)})에 먼저 맞았습니다!");
            }

            if (ghostRoot.activeSelf) ghostRoot.SetActive(false);
        }
    }

    public void StartBuilding(ItemData blueprintItem)
    {
        // 도면이 아니거나, 켤 고스트 이름이 없으면 무시
        if (!blueprintItem.isBuildable || string.IsNullOrEmpty(blueprintItem.ghostObjectName)) return;

        if (isBuildingMode && currentBlueprintData == blueprintItem) return;

        currentBlueprintData = blueprintItem;
        isBuildingMode = true;

        // ✨ [핵심] 고스트 루트 안에서 이름이 일치하는 자식만 켜고, 나머지는 다 끕니다.
        bool foundGhost = false;
        foreach (Transform child in ghostRoot.transform)
        {
            if (child.name == blueprintItem.ghostObjectName)
            {
                child.gameObject.SetActive(true);
                foundGhost = true;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        if (!foundGhost)
        {
            Debug.LogError($"[건축 오류] '{blueprintItem.ghostObjectName}' 이름을 가진 고스트를 찾을 수 없습니다!");
            CancelBuilding();
            return;
        }

        if (UIManager.Instance != null) UIManager.Instance.CloseAllMenus();
    }

    private void PlaceBuilding(Vector3 position)
    {
        // 실제 건물 프리팹 생성 (회전값은 ghostRoot의 회전값을 따라갑니다)
        Instantiate(currentBlueprintData.placeablePrefab, position, ghostRoot.transform.rotation);

        InventoryManager.Instance.RemoveItem(currentBlueprintData, 1);

        if (InventoryManager.Instance.GetItemCount(currentBlueprintData) <= 0)
        {
            CancelBuilding();
        }
    }

    public void CancelBuilding()
    {
        isBuildingMode = false;
        currentBlueprintData = null;
        if (ghostRoot != null) ghostRoot.SetActive(false);
    }
}