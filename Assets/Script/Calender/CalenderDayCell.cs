using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalenderDayCell : MonoBehaviour
{
    public Button button;
    public TMP_Text label;

    private int _daynumber;

    public void Setup(int daynumber, bool interactable)
    {
        _daynumber = daynumber;
        label.text = daynumber > 0 ? daynumber.ToString() : "";  // 0이면 빈칸처리

        button.interactable = interactable;  //버튼 클릭

    }

    public void AddListener(System.Action<int> onClick)
    {
          // 버튼 클릭할 경우, 소비감정 기입쪽으로 넘어감
    }
}
