using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SlideManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public ScrollRect scrollRect;          
    public RectTransform content;

    [Header("Tuning")]
    public float snapDuration = 0.25f;
   [Range(0.1f, 0.9f)] public float snapThreshold = 0.5f;

    private float pageWidth;
    private int pageIndex = 0;

    private Vector2 dragStartContentPos;
    private bool isSnapping;

    void Start()
    {
        scrollRect.enabled = false;  // ScrollRect 비활성화
        pageWidth = scrollRect.viewport.rect.width;

        SetPageImmediate(0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSnapping) return;
        dragStartContentPos = content.anchoredPosition;
    }

    public void OnDrag(PointerEventData e)
    {
        if (isSnapping) return;

        // ScrollRect가 자체적으로 content를 움직이므로, 여기서는 ScrollRect를 쓰지 않고 직접 움직일 거면
        // scrollRect.enabled = false 방식도 가능하지만, 충돌을 피하려면 보통 OnDrag에서 content를 직접 제어합니다.
        Vector2 pos = content.anchoredPosition;
        pos.x += e.delta.x; // 드래그 방향 그대로

        // 2페이지 기준 클램프: [ -pageWidth , 0 ]
        pos.x = Mathf.Clamp(pos.x, -pageWidth, 0f);
        content.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (isSnapping) return;

        float deltaX = content.anchoredPosition.x - dragStartContentPos.x;
        float absDelta = Mathf.Abs(deltaX);

        // 이동 비율 (0~1)
        float progress = absDelta / pageWidth;

        int targetPage = pageIndex; // 기본-> 현재 페이지 유지

        if (progress >= snapThreshold)
        {
            // 왼쪽으로 드래그 → 다음 페이지
            if (deltaX < 0 && pageIndex < 1)
                targetPage = pageIndex + 1;

            // 오른쪽으로 드래그 → 이전 페이지
            else if (deltaX > 0 && pageIndex > 0)
                targetPage = pageIndex - 1;
        }

        StartCoroutine(SlideTo(targetPage));
    }

    private IEnumerator SlideTo(int targetPage)
    {
        isSnapping = true;

        float targetX = (targetPage == 0) ? 0f : -pageWidth;
        Vector2 start = content.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / snapDuration;
            content.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        content.anchoredPosition = end;
        pageIndex = targetPage; // 현재 화면 고정
        isSnapping = false;
    }

    private void SetPageImmediate(int targetPage)
    {
        float targetX = (targetPage == 0) ? 0f : -pageWidth;
        Vector2 pos = content.anchoredPosition;
        pos.x = targetX;
        content.anchoredPosition = pos;
        pageIndex = targetPage;
    }
}


