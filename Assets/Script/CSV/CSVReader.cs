using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using static CSVReader;

public class CSVReader : MonoBehaviour
{
    public string csvFileName = "UIUX_09"; //불러올 파일 이름

    public Dictionary<string, Expenditure> expenditure = new Dictionary<string, Expenditure>();


    [System.Serializable]
    public class Expenditure
    {
        public string date;  //결제일자
        public string time;  //결제시간
        public string classification;  //매출구분
        public int expendituredetails;  //지출내역
        public string storename; //가맹점명

    }

    private void Start()
    {
        ReadCSV();
    }

    #region - 파일을 읽어오는 메서드
    private void ReadCSV()
    {
        string path = "UIUX_09.csv"; //파일이름.확장자

        List<Expenditure> exceList = new List<Expenditure>();

        StreamReader reader = new StreamReader(Application.dataPath + "/" + path);  //경로설정
        bool isFinish = false;

        while (isFinish == false) {
            string data = reader.ReadLine(); // 한 줄 읽기

            // data 변수가 비었는지 확인
            if (data == null)
            {
                // 만약 비었다면? 마지막 줄 == 데이터 없음이니
                // isFinish를 true로 만들고 반복문 탈출
                isFinish = true;
                break;
            }
            var splitData = data.Split(','); // 콤마로 데이터 분할

            Expenditure expend = new Expenditure();

            expend.date = splitData[0];
            expend.time = splitData[1];
            expend.classification = splitData[2];
            expend.expendituredetails = int.Parse(splitData[3]);  // int.Parse()를 활용하여 자료변환
            expend.storename = splitData[4];


            expenditure.Add(expend.date, expend);
            Debug.Log(expend.date);
            Debug.Log(expenditure.Count); // 잘 들어갔는지 체크


        }
        Debug.Log(expenditure);
    }
}

#endregion