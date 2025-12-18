using System;  //DateTime 사용
using System.Collections;
using System.Collections.Generic;
using System.IO;  // 파일 저장 /읽기용
using UnityEngine;
using UnityEngine.Networking;  //  csv파일 다운로드
using System.Globalization;
using System.Linq; // Sum 계산을 위해 사용



// 모바일 환경에서 추후에 csv파일을 전달받을 수 있는 방식으로 변경
// 구글 스프레드시트의 데이터 값을 웹에 게시로 얻어
//해당 데이터값을 받아오기 -> 구글 스프레드시트가 일종의 서버역할


[System.Serializable]
public class Expenditure   
{
    public string rawDate;      // 원본 문자열 (디버그용)
    public DateTime date;       // 파싱된 날짜
    public string time;         // 결제시간
    public string classification;  // 매출구분
    public int expendituredetails; // 지출내역(금액)
    public string storename;       // 가맹점명

    public ConsumeEmotion emotion; // 소비감정
}



public class CSVReader : MonoBehaviour
{
    public static CSVReader Instance;  // 싱글톤

    [Header("구글 시트 csv 다운로드 주소")]
    private string serverURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQPj6bS8R3JHyH2lg8rSOQloMgVDnYX14E5RxHOa6dlPH7k_ceSIdct4IOMIC50mgUk06MlVNpLwFd7/pub?output=csv";

    // 저장될 파일 이름
    private string fileName = "ExpenditureData.csv";

    //파싱된 데이터를 담을 리스트
    public List<Expenditure> expenditure = new List<Expenditure>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        LoadDate();
    }

    [ContextMenu("저장 파일 삭제 후 서버에서 다운로드")]
    public void ForceRedownload()
    {
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"[CSVReader] 기존 파일 삭제: {savePath}");
        }

        Debug.Log("[CSVReader] 서버에서 새로 다운로드 시작");
        UpdateFromServer();
    }

    // CSVReader.cs에 public 함수 추가
    [ContextMenu("CSV 재파싱")]
    public void ReloadAndParseCSV()
    {
        Debug.Log("[CSVReader] 수동 재파싱 시작");
        LoadDate();
    }

    #region - 데이터 로드 메서드
    public void LoadDate()
    {
        // 주소 생성
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

        // 에디터에서는 항상 서버에서 최신 데이터 다운로드
#if UNITY_EDITOR
        Debug.Log("[CSVReader] 에디터 모드: 서버에서 최신 CSV 다운로드");
        UpdateFromServer();
        return;
#endif

        if (File.Exists(savePath))
        {
            Debug.Log("저장된 최신 데이터");
            string csvData = File.ReadAllText(savePath);
            ParseCSV(csvData); //파싱 실행
            
        }
        else  //저장된 파일이 없을 경우
        {
            Debug.Log("저장된 파일이 없음, 기본 파일 적용");
            TextAsset defaultData = Resources.Load<TextAsset>("DefaultData");

            if(defaultData != null) //기본 파일이 있을 경우, 유니티내에 기본파일을 넣어둠
            {
                ParseCSV(defaultData.text);
            }
            else
            {
                //혹시 기본파일이 없을 경우. 서버에서 가져오기
                UpdateFromServer();
            }
        }
    }
    #endregion

    #region - 서버에서 가져오는 메서드
    public void UpdateFromServer()
    {
        StartCoroutine(DownloadCoroutine());
    }

    IEnumerator DownloadCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serverURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //다운로드 실패
            }
            else
            {
                string csvData = www.downloadHandler.text;

                //추후 인터넷 없이 사용하기 위해 다운받아서 리스트로 갱신
                string savePath = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllText(savePath, csvData);

                //다운받은 뒤 리스트 갱신
                ParseCSV(csvData);
            }

        }
    }

    #endregion

    #region - csv파일 리스트로 변환(파싱작업) 메서드
    void ParseCSV(string csvData)
    {
        expenditure.Clear();

        if (csvData.StartsWith("\uFEFF"))
        {
            csvData = csvData.Substring(1);
        }

        csvData = System.Text.RegularExpressions.Regex.Replace(
            csvData,
            @"(\s+)(20\d{2}\.\d{1,2}\.\d{1,2})",
            "\n$2"
        );

        using (StringReader reader = new StringReader(csvData))
        {
            string header = reader.ReadLine();
            int lineNumber = 0;

            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] cols = line.Split(',');

                // 날짜,시간,구분,금액,가맹점명 최소 필요
                if (cols.Length < 5)
                    continue;

                string dateStr = cols[0].Trim();
                string timeStr = cols[1].Trim();
                string classStr = cols[2].Trim();
                string moneyStr = cols[3].Trim();
                string storeStr = cols[4].Trim();

                if (!DateTime.TryParseExact(
                    dateStr,
                    new[] { "yyyy.MM.dd", "yyyy.M.d", "yyyy.MM.d", "yyyy.M.dd" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    continue;
                }

                moneyStr = moneyStr.Replace(",", "").Replace("원", "").Trim();
                if (!int.TryParse(moneyStr, out int amount))
                    continue;

                ConsumeEmotion emotion = ConsumeEmotion.Normal;

                // 감정 컬럼이 있는 경우만 처리
                if (cols.Length >= 6)
                {
                    string emotionStr = cols[5].Trim();

                    if (!string.IsNullOrEmpty(emotionStr))
                    {
                        if (!Enum.TryParse(emotionStr, true, out emotion))
                        {
                            Debug.LogWarning($"[ParseCSV] 감정 파싱 실패: {emotionStr} (line {lineNumber})");
                            emotion = ConsumeEmotion.Normal;
                        }
                    }
                }

                expenditure.Add(new Expenditure
                {
                    rawDate = dateStr,
                    date = parsedDate,
                    time = timeStr,
                    classification = classStr,
                    expendituredetails = amount,
                    storename = storeStr,
                    emotion = emotion
                });

                Debug.Log($"[CSV] {parsedDate:yyyy-MM-dd} | {storeStr} | {emotion}");
            }

            var emotionGroups = expenditure.GroupBy(e => e.emotion);
            foreach (var group in emotionGroups)
            {
                Debug.Log($"[ParseCSV] 감정 통계 - {group.Key}: {group.Count()}건");

                if (group.Key != ConsumeEmotion.Normal)
                {
                    foreach (var item in group)
                    {
                        Debug.Log($"  ▶ {item.date:yyyy-MM-dd} | {item.storename} | {item.emotion}");
                    }
                }
            }
        }
    }
    #endregion

    #region - 소비감정 관리
    public ConsumeEmotion GetDailyEmotion(DateTime date)
    {
        DateTime targetDate = date.Date;

        // 해당 날짜의 모든 항목 조회
        var dayItems = expenditure.Where(e => e.date.Date == targetDate).ToList();

        if (dayItems.Count == 0)
        {
            return ConsumeEmotion.Normal;
        }

        // 감정별 개수 집계
        Dictionary<ConsumeEmotion, int> emotionCount = new Dictionary<ConsumeEmotion, int>();

        foreach (var item in dayItems)
        {
            if (!emotionCount.ContainsKey(item.emotion))
            {
                emotionCount[item.emotion] = 0;
            }
            emotionCount[item.emotion]++;
        }

        // 최빈값(가장 많이 등장한 감정) 찾기
        ConsumeEmotion mostFrequent = ConsumeEmotion.Normal;
        int maxCount = 0;

        foreach (var kvp in emotionCount)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostFrequent = kvp.Key;
            }
        }

        Debug.Log($"[{date:yyyy-MM-dd}] 감정 집계 결과: {string.Join(", ", emotionCount.Select(x => $"{x.Key}={x.Value}"))} → 최종: {mostFrequent}");

        return mostFrequent;
    }

    // 특정 날짜의 소비감정 업데이트
    public void UpdateDailyEmotion(DateTime date, ConsumeEmotion emotion)
    {
        DateTime targetDate = date.Date;
        bool updated = false;

        // 해당 날짜의 모든 항목에 감정 적용
        foreach (var item in expenditure)
        {
            if (item.date.Date == targetDate)
            {
                item.emotion = emotion;
                updated = true;
            }
        }

        if (updated)
        {
            Debug.Log($"[{date:yyyy-MM-dd}] 감정 업데이트 완료: {emotion}");
        }
    }

    #endregion

    #region - 총 출금/입금 내역 계산
    public int DailyTotalAmount(DateTime targetDate)
    {
       // Debug.Log($"=== DailyTotalAmount 시작 ===");
       // Debug.Log($"조회 날짜: {targetDate:yyyy-MM-dd}");
       // Debug.Log($"전체 데이터 건수: {expenditure?.Count ?? 0}");

        if (expenditure == null || expenditure.Count == 0)
        {
            Debug.LogError("expenditure 리스트가 비어있음!");
            return 0;
        }

        // 전체 데이터 중 처음 5개 출력
      //  Debug.Log("--- 전체 데이터 샘플 ---");
        foreach (var e in expenditure.Take(5))
        {
          //  Debug.Log($"  {e.date:yyyy-MM-dd} | {e.expendituredetails}원");
        }

        DateTime day = targetDate.Date;
        var list = expenditure.Where(e => e.date.Date == day).ToList();

     //   Debug.Log($"매칭된 건수: {list.Count}");

        foreach (var e in list)
        {
        //    Debug.Log($"  [매칭] {e.date:yyyy-MM-dd} {e.time} | {e.expendituredetails}원 | {e.storename}");
        }

        int totalSum = list.Sum(e => e.expendituredetails);
     //   Debug.Log($"합계: {totalSum}원");

        return totalSum;
    }

    #endregion

    #region - 월별 총 지출 계산
    public int MonthlyTotalAmount(int year, int month)
    {
        if(expenditure == null || expenditure.Count == 0)
        {
            Debug.LogError("expenditure 리스트가 비어있음!");
            return 0;

        }

        var monthlyList = expenditure
        .Where(e => e.date.Year == year && e.date.Month == month)
        .ToList();

        int totalSum = monthlyList.Sum(e => e.expendituredetails);

        Debug.Log($"{year}년 {month}월 총 지출: {totalSum}원 ({monthlyList.Count}건)");

        return totalSum;
    }

    #endregion

}

