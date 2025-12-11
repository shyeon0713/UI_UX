using System;  //DateTime 사용
using System.Collections;
using System.Collections.Generic;
using System.IO;  // 파일 저장 /읽기용
using UnityEngine;
using UnityEngine.Networking;  //  csv파일 다운로드
using System.Globalization;
using System.Linq; // Sum 계산을 위해 사용
using System.Text.RegularExpressions;


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
}

public class CSVReader : MonoBehaviour
{
    public static CSVReader Instance;  // 싱글톤

    [Header("구글 시트 csv 다운로드 주소")]
    // 구글 시트 csv 다운로드 주소
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



    #region - 데이터 로드 메서드
    public void LoadDate()
    {
        // 주소 생성
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

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

        // 공백으로 붙어있는 데이터를 날짜 패턴으로 분리
        csvData = System.Text.RegularExpressions.Regex.Replace(
            csvData,
            @"(\s+)(20\d{2}\.\d{1,2}\.\d{1,2})",
            "\n$2"
        );

        using (StringReader reader = new StringReader(csvData))
        {
            string header = reader.ReadLine();

            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] cols = line.Split(new[] { ',' }, 5);

                if (cols.Length < 5)
                    continue;

                string dateStr = cols[0].Trim();
                string timeStr = cols[1].Trim();
                string classStr = cols[2].Trim();
                string moneyStr = cols[3].Trim();
                string storeStr = cols[4].Trim();

                // 날짜 파싱 (여러 형식 지원)
                if (!DateTime.TryParseExact(dateStr,
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

                expenditure.Add(new Expenditure
                {
                    rawDate = dateStr,
                    date = parsedDate,
                    time = timeStr,
                    classification = classStr,
                    expendituredetails = amount,
                    storename = storeStr
                });
            }
        }

        Debug.Log($"[ParseCSV] 총 {expenditure.Count}건 완료");
    }
    #endregion


    #region - 총 출금/입금 내역 계산
    public int DailyTotalAmount(DateTime targetDate)
    {
        Debug.Log($"=== DailyTotalAmount 시작 ===");
        Debug.Log($"조회 날짜: {targetDate:yyyy-MM-dd}");
        Debug.Log($"전체 데이터 건수: {expenditure?.Count ?? 0}");

        if (expenditure == null || expenditure.Count == 0)
        {
            Debug.LogError("expenditure 리스트가 비어있음!");
            return 0;
        }

        // 전체 데이터 중 처음 5개 출력
        Debug.Log("--- 전체 데이터 샘플 ---");
        foreach (var e in expenditure.Take(5))
        {
            Debug.Log($"  {e.date:yyyy-MM-dd} | {e.expendituredetails}원");
        }

        DateTime day = targetDate.Date;
        var list = expenditure.Where(e => e.date.Date == day).ToList();

        Debug.Log($"매칭된 건수: {list.Count}");

        foreach (var e in list)
        {
            Debug.Log($"  [매칭] {e.date:yyyy-MM-dd} {e.time} | {e.expendituredetails}원 | {e.storename}");
        }

        int totalSum = list.Sum(e => e.expendituredetails);
        Debug.Log($"합계: {totalSum}원");

        return totalSum;
    }

    #endregion
}

