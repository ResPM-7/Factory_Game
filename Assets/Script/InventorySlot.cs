using UnityEngine;
using UnityEngine.UI;
using TMPro; // 개수 표시용

public class InventorySlot : MonoBehaviour
{
    public ItemData item;      // 현재 슬롯의 아이템 데이터
    public int amount;         // 아이템 개수

    [Header("UI 연결")]
    public RawImage iconRawImage;    // 자식 오브젝트 중 'Icon' 연결
    public Text stackText; // 자식 오브젝트 중 'Stack' 연결

    void Start() => UpdateSlotUI();

    // 슬롯 UI를 최신화하는 함수
    public void UpdateSlotUI()
    {
        // 아이콘 이미지의 컬러/투명도 설정 (기존 코드 유지)
        Color iconColor = iconRawImage.color;

        if (item != null && amount > 0)
        {
            // 1. 아이콘 활성화 및 데이터 세팅
            iconColor.a = 1.0f;
            iconRawImage.color = iconColor;
            iconRawImage.texture = item.iconTexture;
            iconRawImage.enabled = true;

            // 2. 텍스트 활성화 (이 부분이 핵심입니다!)
            stackText.enabled = true;
            stackText.text = amount > 1 ? amount.ToString() : "";
        }
        else
        {
            // 아이템이 없을 때 초기화
            iconColor.a = 0f;
            iconRawImage.color = iconColor;
            iconRawImage.texture = null;
            iconRawImage.enabled = false;

            // 텍스트도 비활성화
            stackText.text = "";
            stackText.enabled = false;

            item = null;
            amount = 0;
        }
    }
    public void ClearSlot()
    {
        item = null;
        amount = 0;
        UpdateSlotUI();
    }
}