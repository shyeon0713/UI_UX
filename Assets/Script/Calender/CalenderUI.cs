using System;  //DateTime 사용
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Voice Emotion UI")]
     public VoiceEmotionInputUI voiceEmotionUI;

    // 감정 입력 중인 날짜
    private DateTime selectedEmotionDate;


    public EmotionClassifier emotionClassifier;

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

    // 해당 월의 감정 데이터 로그 출력
    private void LogEmotionDataForMonth(DateTime date)
    {
        if (CSVReader.Instance?.expenditure == null)
        {
            Debug.LogError("[CalenderUI] CSV 데이터가 없습니다");
            return;
        }

        var monthData = CSVReader.Instance.expenditure
            .Where(e => e.date.Year == date.Year && e.date.Month == date.Month)
            .ToList();

        Debug.Log($"[CalenderUI] {date:yyyy년 MM월} 데이터: 총 {monthData.Count}건");

        var emotionGroups = monthData.GroupBy(e => e.emotion);
        foreach (var group in emotionGroups)
        {
            Debug.Log($"[CalenderUI] {date:yyyy년 MM월} 감정 통계 - {group.Key}: {group.Count()}건");

            // 각 감정별로 처음 3개 항목 출력
            foreach (var item in group.Take(3))
            {
                Debug.Log($"  - {item.date:MM/dd} | {item.storename} | {item.emotion}");
            }
        }
    }


    #region - 캘린더 버튼 배치
    void GenerateCalender(DateTime date)
    {
        monthText.text = $"{date.Month + "월"}";  // 월 표시
       // yearText.text = date.Year.ToString(); //년 표시 -> 수정
        // 추후에 문자형식으로 변경

        int monthlyTotal = CSVReader.Instance.MonthlyTotalAmount(date.Year, date.Month);
       
        // 월 총 지출 계산 및 표시
        monthlyTotalText.text = $" -{monthlyTotal:N0}원";

        // 버블 다이어그램 상단 정보 갱신
        bubbleDiagram.UpdateHeader(
            date.Year,
            date.Month,
            monthlyTotal
        );

        var monthlyCategoryTotals = GetMonthlyCategoryTotals(date.Year, date.Month);
        bubbleDiagram.UpdateDiagram(monthlyCategoryTotals);
        


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
                cell.Setup(0, 0, ConsumeEmotion.Normal, false);  // 날짜 0, 금액 0, 버튼 비활성화 -> 스프라이트도 기본 스프라이트
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

            CalenderDayCell cell = btn.GetComponent<CalenderDayCell>();
            DateTime current = new DateTime(date.Year, date.Month, day);

            int dailySum = CSVReader.Instance.DailyTotalAmount(current);

            // CSVReader에서 감정 조회 (수정)
            ConsumeEmotion emotion = CSVReader.Instance.GetDailyEmotion(current);

            // 디버그 로그 추가
            if (day <= 5 || emotion != ConsumeEmotion.Normal)
            {
                Debug.Log($"[GenerateCalender] {current:yyyy-MM-dd} | 금액: {dailySum}원 | 감정: {emotion}");
            }

            // Setup에 감정 정보 전달
            cell.Setup(day, dailySum, emotion, true);

            // 날짜 클릭 처리
            int capturedDailySum = dailySum;  // 람다 캡처용

            cell.AddListener((clickedDay) =>
            {
                DateTime clickedDate = new DateTime(date.Year, date.Month, clickedDay);

                // 지출 없는 날은 감정 입력 X
                if (capturedDailySum <= 0)
                    return;

                // CSVReader로 확인 (수정)
                ConsumeEmotion currentEmotion = CSVReader.Instance.GetDailyEmotion(clickedDate);
                if (currentEmotion != ConsumeEmotion.Normal)
                    return;

                selectedEmotionDate = clickedDate;
                Debug.Log("[FLOW] Open Voice Emotion UI : " + clickedDate);
                voiceEmotionUI.Open(clickedDate, OnVoiceEmotionResult);
            });

            index++;
        }
    }
    #endregion
    private void OnVoiceEmotionResult(DateTime date, string text)
    {
        ConsumeEmotion emotion = emotionClassifier.Classify(text);
        Debug.Log($"[OnVoiceEmotionResult] 감정 분류 결과: {emotion}");

        // CSV에 감정 업데이트
        CSVReader.Instance.UpdateDailyEmotion(date, emotion);

        Debug.Log($"[OnVoiceEmotionResult] 캘린더 갱신 시작");

        // 캘린더 UI 갱신
        GenerateCalender(currentDate);

        Debug.Log($"[OnVoiceEmotionResult] 완료");
    }

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

