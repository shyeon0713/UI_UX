using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BubbleDiagramController : MonoBehaviour
{
    [Header("버블차트 아이템")]
    public List<BubbleItem> bubbles;

    [Header("최대/최소 사이즈 지정")]
    public float maxRadius = 140f;   // 가장 큰 원 반지름
    public float minRadius = 40f;    // 최소 원 크기

    [Header("Text UI")]
    public TMP_Text monthlyTotalText;  // 월 총 지출 표시
    public TMP_Text yearText;  // 년 표시
    public TMP_Text monthText;  // 월 표시

    [Header("범위")]
    public float selectedScale = 1.1f;

    private BubbleItem selectedBubble;

    public void UpdateDiagram(Dictionary<CategoryType, int> monthlyTotals)
    {
        if (monthlyTotals == null || monthlyTotals.Count == 0)
            return;

        float total = monthlyTotals.Values.Sum();

        if (total <= 0f) return;

        foreach (var bubble in bubbles)
        {
            if (!monthlyTotals.TryGetValue(bubble.category, out int value))
            {
                bubble.bubbleImage.gameObject.SetActive(false);
                continue;
            }

            float percent = value / total; // 0~1

            float radius = maxRadius * Mathf.Sqrt(percent);
            radius = Mathf.Max(radius, minRadius);

            bubble.bubbleImage.rectTransform.sizeDelta =
                new Vector2(radius * 2f, radius * 2f);

            bubble.bubbleImage.rectTransform.anchoredPosition =
                bubble.basePosition;

            bubble.label.text =
                $"{GetCategoryName(bubble.category)}\n{percent * 100f:0.#}%";
            // 라벨 텍스트 

            bubble.bubbleImage.gameObject.SetActive(true);
        }
    }

    public void UpdateHeader(int year, int month, int monthlyTotal)
    {
        if (monthText != null)
        {
            monthText.text = $"{month}월";
        }
        if(yearText != null)
        {
            yearText.text = $"{year}";
        }

        if (monthlyTotalText != null)
            monthlyTotalText.text = $"-{monthlyTotal:N0}원";
    }

    // 카테고리 라벨링
    private string GetCategoryName(CategoryType type)
    {
        switch (type)
        {
            case CategoryType.Food: return "식비";
            case CategoryType.Culture: return "문화";
            case CategoryType.Transport: return "교통";
            case CategoryType.Shopping: return "쇼핑";
            case CategoryType.Saving: return "저축";
            case CategoryType.Other: return "그 외";
            default: return "";
        }
    }
}


