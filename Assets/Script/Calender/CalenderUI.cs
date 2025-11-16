using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

public class CalenderUI : MonoBehaviour
{
    [Header("UI reference")]
    public TextMeshProUGUI monthText;
    public Transform datesGrid;
    public GameObject daybuttonPrefab;

    private DateTime currentDate;  // �޷¿� �ʿ��� ��� ��ɿ� ����
    // using System;

    private List<Button> pool = new List<Button>();  //pool ����Ʈ�� ��ư���� ����
                                                     // ��ư ���ġ�̱� ������


    #region -
    void InitializePool()
    {
        int maxSlots = 42; // 6�� * 7��

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject obj = Instantiate(daybuttonPrefab, datesGrid);
            Button btn = obj.GetComponent<Button>();

            pool.Add(btn);

            obj.SetActive(false);  //ó������ �� ��Ȱ��ȭ ���ѳ���

        }
    }

    #endregion
    private void Start()
    {
        currentDate = DateTime.Now; //���� date�� ����
        GenerateCalender(currentDate);
    }

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

    #region - 
    void GenerateCalender(DateTime date)
    {
        monthText.text = $"{date.Year}";  // �ش�κ� �����ʿ�
        //- ���� ����� ǥ��

        DateTime firstday = new DateTime(date.Year, date.Month, 1);  //���۳�¥
        int startday = (int)firstday.DayOfWeek;
        int dayInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        foreach (var btn in pool)  // 1. ��ü ��Ȱ��ȭ

            btn.gameObject.SetActive(false);  // Destroy���ٴ� SetActiveȰ���Ͽ� ��Ȱ��/Ȱ����Ű��

        int index = 0; //poolindex �ʱ�ȭ


        for (int i = 0; i < startday; i++)   // ��ĭ ó���ϱ� (����¥ ����)
        {
            pool[index].gameObject.SetActive(true);
            pool[index].GetComponentInChildren<TMP_Text>().text = "";
            index++;


        }

        // ��¥ ä���
        for (int day = 1; day <= dayInMonth; day++)
        {
            var btn = pool[index];
            btn.gameObject.SetActive(true);

            btn.GetComponentInChildren<TMP_Text>().text = day.ToString();

            // ���� ��¥ ���� -> �۾��� ����
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

