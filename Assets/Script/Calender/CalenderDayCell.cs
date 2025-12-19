using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CalenderDayCell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button button;
    public TMP_Text textDay;
    public TMP_Text totaltext;  //지출내역 text

    [Header("버튼 클릭 4초정도 유지할 경우 voice UI로 넘어감")]
    private bool isPointerDown = false;
    private float pressStartTime;
    private const float LONG_PRESS_DURATION = 0.8f; // 0.8-1초 권장

    [Header("감정별 스프라이트")]
    public Sprite[] emotionSprites;  // 기본, 긍정, 부정, 충동

    private Image _btnImage;  // Cell 베경
    private int _daynumber;


    [Header("이벤트 선언")]
    private System.Action<int> onLongPress;  // Voice UI용
    private System.Action<int> onShortClick; // 리마인더 UI용

    void Awake()   // Cell 배경 이미지속성 가져오기
    {
        _btnImage = button.GetComponent<Image>();
    }

    //인터페이스 구현
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isPointerDown = true;
        pressStartTime = Time.time;
        StartCoroutine(CheckLongPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        float pressDuration = Time.time - pressStartTime;
        isPointerDown = false;

        // 짧은 클릭 = 리마인더 UI
        if (pressDuration < LONG_PRESS_DURATION)
        {
            Debug.Log($"[FLOW] Short Click (리마인더): {_daynumber}");
            onShortClick?.Invoke(_daynumber);
        }
    }

    // 롱프레스 체크 코루틴
    private IEnumerator CheckLongPress()
    {
        yield return new WaitForSeconds(LONG_PRESS_DURATION);

        // 여전히 누르고 있으면 롱프레스
        if (isPointerDown)
        {
            Debug.Log($"[FLOW] Long Press (Voice UI): {_daynumber}");
            onLongPress?.Invoke(_daynumber);
            isPointerDown = false; // 중복 실행 방지
        }
    }

    #region- 일/금액 전체 초기화 함수
    public void Setup(int daynumber, int totalAmount, ConsumeEmotion emotion, bool interactable)
    {
        _daynumber = daynumber;

        //날짜 표시
        textDay.text = daynumber > 0 ? daynumber.ToString() : "";  // 0이면 빈칸처리

        //지출내역 표시-> 날짜가 존재하고 총액이 0 이상일경우
        if (daynumber > 0 && totalAmount > 0)
        {
   
            totaltext.text = totalAmount.ToString();  // 원 제외
            // 가격 + 원 표시 $"{totalAmount + "원"}";
            totaltext.color = Color.gray; // 지출은 회색으로 표시

            // 감정 스프라이트 적용
            // enum을 int로 캐스팅하여 배열 인덱스로 사용
            int emotionIndex = (int)emotion;
            if (emotionIndex < emotionSprites.Length && _btnImage != null)
            {
                _btnImage.sprite = emotionSprites[emotionIndex];
            }
        }

        else
        {
            totaltext.text = ""; // 내역이 없을 경우, 빈칸으로
        }

        button.interactable = interactable;


        if (_btnImage != null)
        {
            _btnImage.color = interactable ? Color.white : new Color(0, 0, 0, 0);
        }
        else
        {
            textDay.text = "";
            totaltext.text = "";
            button.interactable = false;
        }
    }

#endregion

#region - 이벤트 리스너 연결 -> 소비감정 기입하기 + 해당 요일 소비리마이던 확인하기
public void AddListener(System.Action<int> onLongPressCallback, System.Action<int> onShortClickCallback)
    {
        {
        onLongPress = onLongPressCallback;
        onShortClick = onShortClickCallback;
    }
}
    #endregion
}
