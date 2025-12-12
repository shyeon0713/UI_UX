using System;  //DateTime 사용
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CalenderUI : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject daybuttonPrefab;
    public Transform datesGrid;

    [Header("Text UI")]
    public TMP_Text monthText;
    public TMP_Text yearText;
    public TMP_Text monthlyTotalText;  // 월 총 지출 표시

    [Header("년/월 선택 UI")]
    public Button premonth;
    public Button nextmonth;
    //추후에 팝업형식의 달력을 띄워 원하는 날짜를 직접 선택할 수 있도록 수정

    // public Button nextyear;  년을 변경하는 버튼 -> 팝업으로 수정? 아님 버튼으로 수정?
    // public Button preyear;


    [Header("버블다이어그램")]
    public BubbleDiagramController bubbleDiagram;
    public CategoryClassifier categoryClassifier;

    public CSVReader csvreader;  //지출내역 가져오기


    private DateTime currentDate;  // 현재 표시 중인 년/월
    // using System;

    private List<Button> pool = new List<Button>();  //pool ����Ʈ�� ��ư���� ����

    const int maxSlots = 42; // 6주 * 7일 -> const로 선언

    #region - 풀링 
    void InitializePool()
    {

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject obj = Instantiate(daybuttonPrefab, datesGrid);
            Button btn = obj.GetComponent<Button>();

            pool.Add(btn);

            obj.SetActive(false);  //처음에 전부 비활성화
        }
    }

    void Awake()
    {
        currentDate = DateTime.Today; // 시작은 오늘 기준
        InitializePool();

    }

    #region - CSV 데이터 로드
    #endregion
    private void Start()
    {
        currentDate = DateTime.Now; //DateTime 활용

        premonth.onClick.AddListener(PreMonth); //이전 달 버튼 리스너 추가
        nextmonth.onClick.AddListener(NextMonth); // 다음 달 버튼 리스너 추가

        StartCoroutine(WaitForDataAndGenerate());
    }


    private IEnumerator WaitForDataAndGenerate()
    {
        // CSVReader 인스턴스와 데이터 로드 대기
        while (CSVReader.Instance == null ||
               CSVReader.Instance.expenditure == null ||
               CSVReader.Instance.expenditure.Count == 0)
        {
            yield return null;
        }

        Debug.Log("데이터 로드 완료, 캘린더 생성 시작");
        GenerateCalender(currentDate);
    }

    #endregion


    #region - 버튼 리스너 (Month)
    public void NextMonth()  //��ư�̶� ����
    {
        currentDate = currentDate.AddMonths(1);
        GenerateCalender(currentDate);
       
    }

    public void PreMonth()  //��ư�̶� ����
    {
        currentDate = currentDate.AddMonths(-1);
        GenerateCalender(currentDate);
        
    }
    #endregion

    #region - 캘린더 버튼 배치
    void GenerateCalender(DateTime date)
    {
        monthText.text = $"{date.Month + "월"}";  // 월 표시
       // yearText.text = date.Year.ToString(); //년 표시 -> 수정
        // 추후에 문자형식으로 변경

        int monthlyTotal = CSVReader.Instance.MonthlyTotalAmount(date.Year, date.Month);
        // 월 총 지출 계산 및 표시
        monthlyTotalText.text = $" -{monthlyTotal:N0}원";


        DateTime firstday = new DateTime(date.Year, date.Month, 1);  //���۳�¥
        int startday = (int)firstday.DayOfWeek;
        int dayInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        foreach (var btn in pool)  //��ü ��Ȱ��ȭ
            btn.gameObject.SetActive(false);  // Destroy ->  SetActive활용

        int index = 0; //poolindex �ʱ�ȭ


        for (int i = 0; i < startday; i++)   // 빈칸처리
        {
            var btn = pool[index];
            btn.gameObject.SetActive(true);

            CalenderDayCell cell = btn.GetComponent<CalenderDayCell>();
            if (cell != null)
            {
                cell.Setup(0, 0, false);  // 날짜 0, 금액 0, 버튼 비활성화
            }
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = new Color(0, 0, 0, 0); //  비활성화 대신 버튼 이미지 투명화
            }

            index++;
        }

        // 날짜 배치 + 지출내역 배치 
        for (int day = 1; day <= dayInMonth; day++)
        {
            var btn = pool[index];
            btn.gameObject.SetActive(true);


            CalenderDayCell cell = btn.GetComponent<CalenderDayCell>();  // DayCell 함수가져오기

            if (cell != null) //cell이 null이 아닐 경우
            {
                DateTime Currentdate = new DateTime(date.Year, date.Month, day);  //년월일이 확인을 위해 현재 날짜 확인

                int dailySum = CSVReader.Instance.DailyTotalAmount(Currentdate);
                //static은 개인변수를 통해서 접근이 불가능 -> CSVReader를 사용
                yearText.text = Currentdate.Year.ToString();
                //Setup 함수 호출
                cell.Setup(day, dailySum, true);

                //클릭 이벤트 연결
                cell.AddListener((clickedDay) =>
                {

                });
            }
            index++;
        }

           
    }
    #endregion


    private Dictionary<CategoryType, int> GetMonthlyCategoryTotals(int year, int month)
    {
        Dictionary<CategoryType, int> result = new();

        foreach (var item in CSVReader.Instance.expenditure)
        {
            // 문자열 파싱 전부 제거
            if (item.date.Year != year || item.date.Month != month)
                continue;

            CategoryType category =
                categoryClassifier.Classify(item.storename);

            if (!result.ContainsKey(category))
                result[category] = 0;

            result[category] += item.expendituredetails;
        }

        return result;
    }


}

