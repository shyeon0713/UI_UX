using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalenderDayCell : MonoBehaviour
{
    public Button button;
    public TMP_Text textDay;
    public TMP_Text totaltext;  //지출내역 text

    private int _daynumber;

    #region- 일/금액 전체 초기화 함수
    public void Setup(int daynumber, int totalAmount, bool interactable)
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
        }else
        {
            totaltext.text = ""; // 내역이 없을 경우, 빈칸으로
        }

        button.interactable = interactable;
        Image btnImage = button.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = Color.white; // 원래 색상으로 복원
        }else{  
            textDay.text ="";
            totaltext.text = "";
            button.interactable = false;

            if (btnImage != null)
            {
                btnImage.color = new Color(0, 0, 0, 0); // 투명
            }
        }
    }



    #endregion

    #region - 이벤트 리스너 연결 -> 소비감정 기입하기 + 해당 요일 소비리마이던 확인하기
    public void AddListener(System.Action<int> onClick)
    {
          // 버튼 클릭할 경우, 소비감정 기입쪽으로 넘어감
    }
    #endregion
}
