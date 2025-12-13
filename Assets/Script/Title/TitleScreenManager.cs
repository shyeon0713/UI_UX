using UnityEngine;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    public GameObject titlePanel;  // 타이틀 UI 패널
    public float displayTime = 3f; // 타이틀 표시 시간

    void Start()
    {
        titlePanel.SetActive(true);
        StartCoroutine(HideTitlePanel());
    }

    IEnumerator HideTitlePanel()
    {
        yield return new WaitForSeconds(displayTime);
        titlePanel.SetActive(false);
    }
}

