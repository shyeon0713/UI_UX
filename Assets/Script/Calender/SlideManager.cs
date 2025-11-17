using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class SlideManager : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform calendarP;
    public RectTransform diagramP;

    private float screenWidth;
    private Vector2 calendarStartPos;
    private Vector2 diagramStartPos;

    void Start()
    {
        screenWidth = Screen.width;

        //초기위치 설정
        calendarP.anchoredPosition = Vector2.zero;
        diagramP.anchoredPosition = new Vector2(screenWidth, 0);
    }

    public void OnDrag(PointerEventData e)
    {
        float deltaX = e.delta.x;

        // 슬라이드 거리만큼 두 패널 이동
        calendarP.anchoredPosition += new Vector2(deltaX, 0);
        diagramP.anchoredPosition += new Vector2(deltaX, 0);
    }

    public void OnEndDrag(PointerEventData e)
    {
        // 50% 이상 넘기면 화면 전환
        if (calendarP.anchoredPosition.x < -screenWidth / 2)
        {
            // 다이어그램 화면으로 전환
            StartCoroutine(SlideTo(diagram: true));
        }
        else
        {
            // 달력 화면으로 전환
            StartCoroutine(SlideTo(diagram: false));
        }
    }

    private IEnumerator SlideTo(bool diagram)
    {
        float t = 0;
        float duration = 0.25f;

        Vector2 calendarTarget = diagram ? new Vector2(-screenWidth, 0) : Vector2.zero;
        Vector2 diagramTarget = diagram ? Vector2.zero : new Vector2(screenWidth, 0);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            calendarP.anchoredPosition = Vector2.Lerp(calendarP.anchoredPosition, calendarTarget, t);
            diagramP.anchoredPosition = Vector2.Lerp(diagramP.anchoredPosition, diagramTarget, t);

            yield return null;
        }
    }
}


