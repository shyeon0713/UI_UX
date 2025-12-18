using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalenderDayCell : MonoBehaviour
{
    public Button button;
    public TMP_Text textDay;
    public TMP_Text totaltext;  //지출내역 text


    [Header("감정별 스프라이트")]
    public Sprite[] emotionSprites;  // 기본, 긍정, 부정, 충동

    private Image _btnImage;  // Cell 베경
    private int _daynumber;

    void Awake()   // Cell 배경 이미지속성 가져오기
    {
        _btnImage = button.GetComponent<Image>();
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
public void AddListener(System.Action<int> onClick)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            Debug.Log("[FLOW] DayCell Clicked : " + _daynumber);
            onClick?.Invoke(_daynumber);
        });
    }
    #endregion
}
