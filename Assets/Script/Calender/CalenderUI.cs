using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

public class CalenderUI : MonoBehaviour
{
    [Header("달력 구성요소들")]
    public TextMeshProUGUI monthText;
    public Transform datesGrid; 
    public GameObject daybuttonPrefab;

    private DateTime currentDate;  // 달력에 필요한 모든 기능에 포함
    // using System;

    private List<Button> pool = new List<Button> ();  //pool 리스트를 버튼으로 생성
                                                      // 버튼 재배치이기 때문에


    #region - 풀 생성
    void InitializePool()
    {
        int maxSlots = 42; // 6주 * 7일

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject obj = Instantiate(daybuttonPrefab,datesGrid);
            Button btn = obj.GetComponent<Button>();

            pool.Add(btn);

            obj.SetActive(false);  //처음에는 다 비활성화 시켜놓기

        }
    }

    #endregion
    private void Start()
    {
        currentDate = DateTime.Now; //현재 date값 저장
        GenerateCalender(currentDate);
    }

    public void NextMonth()  //버튼이랑 연결
    {
        currentDate = currentDate.AddMonths(1);
        GenerateCalender(currentDate);
    }

    public void PreMonth()  //버튼이랑 연결
    {
        currentDate = currentDate.AddMonths(-1);
        GenerateCalender (currentDate);
    }

    #region - 캘린터 배치 / 날짜 채우기 / 오늘날짜 표시 및 빈칸 처리
    void GenerateCalender (DateTime date)
    {
        monthText.text = $"{date.Year}";  // 해당부분 수정필요
        //- 월을 영어로 표기

        DateTime firstday = new DateTime(date.Year, date.Month,1);  //시작날짜
        int startday = (int)firstday.DayOfWeek;
        int dayInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        foreach (var btn in pool)  // 1. 전체 비활성화
        
           btn.gameObject.SetActive(false);  // Destroy보다는 SetActive활용하여 비활성/활성시키기

            int index = 0; //poolindex 초기화
        

        for (int i = 0; i < startday; i++)   // 빈칸 처리하기 (끝날짜 이후)
        {
            pool[index].gameObject.SetActive(true);
            pool[index].GetComponentInChildren<TMP_Text>().text = "";
            index++;


        }

        // 날짜 채우기
        for (int day =1; day <= dayInMonth; day++)
        {
            var btn = pool[index];  
            btn.gameObject.SetActive(true);

            btn.GetComponentInChildren<TMP_Text>().text = day.ToString();

            // 오늘 날짜 강조 -> 글씨에 강조
            if (date == DateTime.Now.Date && day == DateTime.Now.Day)
            {
                btn.GetComponent<Image>().color = new Color(1f, 0.8f, 0.8f);
            }
            else
            {
                btn.GetComponent<Image>().color = Color.white;
            }

            index++;
        }
    }
    #endregion
}


