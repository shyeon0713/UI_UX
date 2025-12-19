using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ReminderToggleItem : MonoBehaviour
{
    [Header("Header (접힌 상태)")]
    public TMP_Text headerText;
    public Button toggleButton;

    [Header("Content (펼쳐진 상태)")]
    public GameObject contentPanel;
    public Transform imageGrid;       // GridLayoutGroup
    public GameObject imagePrefab;    // 이미지 버튼 프리팹

    private bool isExpanded = false;
    private List<string> imagePaths;
    private Action<Texture2D, string> onImageClicked;

    void Start()
    {
        toggleButton.onClick.AddListener(ToggleContent);
        contentPanel.SetActive(false);
    }

    public void Setup(Expenditure expenditure, List<string> allImagePaths,
                      Action<Texture2D, string> imageClickCallback)
    {
        // 헤더 텍스트 설정
        headerText.text = $"{expenditure.date:MM/dd HH:mm} | {expenditure.storename} | " +
                         $"-{expenditure.expendituredetails:N0}원";

        imagePaths = allImagePaths;
        onImageClicked = imageClickCallback;
    }

    private void ToggleContent()
    {
        isExpanded = !isExpanded;
        contentPanel.SetActive(isExpanded);

        if (isExpanded && imageGrid.childCount == 0)
        {
            LoadImages();
        }
    }

    private void LoadImages()
    {
        foreach (string path in imagePaths)
        {
            StartCoroutine(LoadImageCoroutine(path));
        }
    }

    private IEnumerator LoadImageCoroutine(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ReminderToggleItem] 파일 없음: {path}");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        // 이미지 버튼 생성
        GameObject imgObj = Instantiate(imagePrefab, imageGrid);
        Image img = imgObj.GetComponent<Image>();
        Button btn = imgObj.GetComponent<Button>();

        img.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        btn.onClick.AddListener(() => onImageClicked?.Invoke(texture, path));

        yield return null;
    }
}