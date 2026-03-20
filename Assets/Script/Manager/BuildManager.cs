using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("건축 상태")]
    public bool isBuildingMode = false;
    private ItemData currentBlueprintData;
    public GameObject ghostRoot;

    private Camera mainCamera;

    [Header("건축 설정")]
    public LayerMask groundLayer;
    public float maxBuildDistance = 15f; 
    public bool useGridSnap = true;
    public float gridSize = 1f;

    // ✨ [새로 추가된 핵심 기능] 겹침 방지 및 색상 제어
    [Header("건축 방해물 & 색상 설정")]
    public LayerMask obstacleLayer;      // 겹치면 안 되는 물체들의 레이어 (보통 Default, Player 등)
    public Vector3 boxSize = new Vector3(0.45f, 0.45f, 0.45f); // 겹침을 검사할 가상 박스의 절반 크기
    public Color validColor = new Color(0, 0, 1, 0.5f);   // 설치 가능: 반투명 파란색
    public Color invalidColor = new Color(1, 0, 0, 0.5f); // 설치 불가: 반투명 빨간색

    private Vector3 _lastGhostPosition;
    private bool _canBuild = false;      // 현재 설치 가능한 상태인가?
    private List<Renderer> _ghostRenderers = new List<Renderer>(); // 고스트의 색상을 바꿀 렌더러들

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
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

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, groundLayer))
        {
            if (!ghostRoot.activeSelf) ghostRoot.SetActive(true);

            Vector3 targetPosition = hit.point;
            if (useGridSnap)
            {
                targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
            }

            // 고스트 이동
            if (_lastGhostPosition != targetPosition)
            {
                ghostRoot.transform.position = targetPosition;
                _lastGhostPosition = targetPosition;
            }

            // ✨ 1. 겹침 검사 (가상의 박스를 만들어서 장애물이 있는지 확인)
            // 중심점을 땅바닥에서 박스 높이만큼 위로 살짝 올려서 검사합니다.
            Vector3 checkCenter = targetPosition + Vector3.up * boxSize.y;
            Collider[] colliders = Physics.OverlapBox(checkCenter, boxSize, ghostRoot.transform.rotation, obstacleLayer);

            // 겹친 물체가 하나도 없으면(0) 설치 가능!
            _canBuild = (colliders.Length == 0);

            // ✨ 2. 색상 변경 (가능하면 파랑, 불가능하면 빨강)
            Color targetColor = _canBuild ? validColor : invalidColor;
            foreach (var rend in _ghostRenderers)
            {
                // 머티리얼의 색상을 실시간으로 바꿉니다.
                rend.material.color = targetColor;
            }

            // ✨ 3. 클릭 시 설치 (설치 가능할 때만!)
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (_canBuild)
                {
                    PlaceBuilding(targetPosition);
                }
                else
                {
                    Debug.Log("여기는 다른 물체와 겹쳐서 건물을 설치할 수 없습니다!");
                }
            }
        }
        else
        {
            if (ghostRoot.activeSelf) ghostRoot.SetActive(false);
        }
    }

    public void StartBuilding(ItemData blueprintItem)
    {
        if (!blueprintItem.isBuildable || string.IsNullOrEmpty(blueprintItem.ghostObjectName)) return;
        if (isBuildingMode && currentBlueprintData == blueprintItem) return;

        currentBlueprintData = blueprintItem;
        isBuildingMode = true;

        bool foundGhost = false;
        foreach (Transform child in ghostRoot.transform)
        {
            if (child.name == blueprintItem.ghostObjectName)
            {
                child.gameObject.SetActive(true);
                foundGhost = true;

                // ✨ 방금 켜진 고스트 안에 있는 모든 Renderer(외형)를 리스트에 담아둡니다. (색상을 바꾸기 위해)
                _ghostRenderers.Clear();
                _ghostRenderers.AddRange(child.GetComponentsInChildren<Renderer>());
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        if (!foundGhost)
        {
            CancelBuilding();
            return;
        }

        if (UIManager.Instance != null) UIManager.Instance.CloseAllMenus();
    }

    private void PlaceBuilding(Vector3 position)
    {
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