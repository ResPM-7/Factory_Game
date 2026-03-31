using UnityEngine;
using System.Collections.Generic;
public class PalBoxManager : MonoBehaviour
{
    public static PalBoxManager Instance;

    [Header("저장된 팰 목록")]
    public List<PalData> storedPals = new List<PalData>();
    public int maxCapacity = 40; // 스크린샷을 보니 8칸 x 5줄 = 40칸이네요!

    [Header("UI 연결")]
    public GameObject palBoxUI;
    public PalSlot[] palSlots; // 스크린샷에 있는 40개의 슬롯들을 여기에 연결합니다.

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 시작할 때 빈 슬롯들을 초기화해줍니다.
        UpdateUI();
    }

    // [핵심] 필드에서 몬스터를 스피어로 잡았을 때 부르는 함수입니다!
    public bool CatchPal(PalData newPal)
    {
        if (storedPals.Count < maxCapacity)
        {
            storedPals.Add(newPal); // 리스트에 추가
            Debug.Log($"<color=cyan>[팰 획득]</color> {newPal.palName}을(를) Pal Chest에 전송했습니다!");

            UpdateUI(); // 화면 갱신
            return true;
        }
        else
        {
            Debug.LogWarning("Pal Chest가 가득 찼습니다! 더 이상 잡을 수 없습니다.");
            return false;
        }
    }

    // UI 슬롯에 저장된 팰들을 순서대로 그려주는 함수
    public void UpdateUI()
    {
        if (palSlots == null || palSlots.Length == 0) return;

        for (int i = 0; i < palSlots.Length; i++)
        {
            // 잡은 팰이 있는 칸은 팰 데이터를 넣어주고
            if (i < storedPals.Count)
            {
                palSlots[i].Setup(storedPals[i]);
            }
            // 남는 빈칸은 싹 비워줍니다.
            else
            {
                palSlots[i].ClearSlot();
            }
        }
    }

    public void OnPalClicked(PalData clickedPal)
    {
        // 일단 클릭이 잘 되는지 콘솔창에 띄워봅니다!
        Debug.Log($"<color=yellow>[팰 선택]</color> {clickedPal.palName}을(를) 클릭했습니다! (여기에 거점 배치 로직 추가 예정)");

        // TODO: 클릭한 팰을 거점에 소환하는 코드 작성
    }

    public void TogglePalBoxUI()
    {
        if (palBoxUI == null)
        {
            Debug.LogError("UI 패널이 연결되지 않았습니다! 인스펙터를 확인해주세요.");
            return;
        }

        // 현재 켜져있으면 끄고, 꺼져있으면 켭니다.
        bool isCurrentlyActive = palBoxUI.activeSelf;
        palBoxUI.SetActive(!isCurrentlyActive);

        // UI 상태에 따라 마우스 커서 조절 및 데이터 갱신
        if (palBoxUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}