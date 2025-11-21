using System;  //DateTime 사용
using System.Collections;
using System.Collections.Generic;
using System.IO;  // 파일 저장 /읽기용
using UnityEngine;
using UnityEngine.Networking;  //  csv파일 다운로드
using System.Linq; // Sum 계산을 위해 사용


// 모바일 환경에서 추후에 csv파일을 전달받을 수 있는 방식으로 변경
// 구글 스프레드시트의 데이터 값을 웹에 게시로 얻어
//해당 데이터값을 받아오기 -> 구글 스프레드시트가 일종의 서버역할


[System.Serializable]
public class Expenditure   
{
    public string date;  //결제일자
    public string time;  //결제시간
    public string classification;  //매출구분
    public int expendituredetails;  //지출내역
    public string storename; //가맹점명

}

public class CSVReader : MonoBehaviour
{
    public static CSVReader Instance;  // 싱글톤

    [Header("구글 시트 csv 다운로드 주소")]
    // 구글 시트 csv 다운로드 주소
    private string serverURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQPj6bS8R3JHyH2lg8rSOQloMgVDnYX14E5RxHOa6dlPH7k_ceSIdct4IOMIC50mgUk06MlVNpLwFd7/pub?gid=0&single=true&output=csv";

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
        expenditure.Clear(); // 리스트 초기화

        StringReader reader = new StringReader(csvData);

        string header = reader.ReadLine();

        //Peek() = 다음에 읽은 데이터가 있는지 확인만 하고, 커서(위치)는 움직이지 않는 함수
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;

            string[] rows = line.Split(',');
            //,기준으로 문자열 자르기

            if (rows.Length < 5) continue;
            //데이터 칸 수가 맞는지 확인하기 -> 최소 5개여야함

            Expenditure data = new Expenditure();

            data.date = rows[0]; //A열 결제일자 표시
            data.time = rows[1]; //B열 결제시간 표시
            data.classification = rows[2];//C열  매출구분 표시


            if (int.TryParse(rows[3], out int cost))
            {
                data.expendituredetails = cost;  //E열 , 지출내역은 int으로 TryParse를 사용하여 변환
            }
            else
            {
                data.expendituredetails = 0; // 반환하지 못할 경우, 내역을 0원으로 넘기기
            }

            data.storename = rows[4]; //E열 가맹점 표시

            expenditure.Add(data);
        }

        reader.Close();
        Debug.Log("지출내역 업로드");
        Debug.Log($"{expenditure.Count}개 업로드");
    }

    #endregion


    #region - 총 출금/입금 내역 계산
    public int DailyTotalAmount(DateTime targetDate)
    {
        //지출내역의 리스트가 비어있으면 0원 리턴
        if (expenditure == null || expenditure.Count == 0)
        {
            Debug.Log("리스트에 데이터 없음");
                return 0;
        }


        //해당 날짜와 일치하는 항목만 골라서 금액 합산
        // Where -> 데이터리스트에서 내가 원하는 조건에 맞는 것만 걸러내는 필터역할
        //item => {...} : 리스트에 있는 데이터 하나를 가져와서 중괄호 안의 내용을 실행해라
        //반복문을 쓰지 않아도 알아서 리스트를 끝까지 반복

        int totalSum = expenditure.Where(item =>
        {
            if (DateTime.TryParse(item.date, out DateTime itemDate))
            {
                return itemDate.Year == targetDate.Year &&  //년도가 동일한지
                itemDate.Month == targetDate.Month && //월이 동일한지
                itemDate.Day == targetDate.Day; //일이 동일한지 확인
            }
            return false; // 날짜 형식이 이상하면 제외시키기  
        })
          .Sum(item => item.expendituredetails);  //지출내역끼리만 다 더함
        return totalSum;
    }

    #endregion
}

