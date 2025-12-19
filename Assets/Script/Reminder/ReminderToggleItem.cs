using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ReminderToggleItem : MonoBehaviour
{
    [Header("Header (축약 상태)")]
    public TMP_Text storeNameText;
    public TMP_Text totalAmountText;
    public Button toggleButton;

    [Header("Content (확장 상태)")]
    public GameObject contentPanel;
    public Transform imageGrid;         // GridLayoutGroup (3열)
    public GameObject imagePrefab;      // 이미지 버튼 프리팹

    private bool isExpanded = false;
    private List<string> imagePaths;
    private Action<string> onImageClicked;

    void Awake()
    {
        if (toggleButton == null)
        {
            // Header 전체를 버튼으로 사용
            toggleButton = GetComponent<Button>();
            if (toggleButton == null)
                toggleButton = gameObject.AddComponent<Button>();
        }

        toggleButton.onClick.AddListener(ToggleContent);

        if (contentPanel != null)
            contentPanel.SetActive(false);
    }

    public void Setup(DateTime date, int totalAmount, string firstStoreName,
                      List<string> allImagePaths, Action<string> imageClickCallback)
    {
        storeNameText.text = $"{firstStoreName} ...";
        totalAmountText.text = $"-{totalAmount:N0}";

        imagePaths = allImagePaths;
        onImageClicked = imageClickCallback;
    }

    private void ToggleContent()
    {
        isExpanded = !isExpanded;

        if (contentPanel != null)
            contentPanel.SetActive(isExpanded);

        if (isExpanded && imageGrid != null && imageGrid.childCount == 0)
        {
            LoadImages();
        }
    }

    private void LoadImages()
    {
        if (imagePaths == null) return;

        foreach (string path in imagePaths)
        {
            StartCoroutine(LoadImageCoroutine(path));
        }
    }

    private IEnumerator LoadImageCoroutine(string path)
    {
        Texture2D texture = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ReminderToggleItem] 파일 없음: {path}");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(path);
        texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(fileData))
        {
            Debug.LogWarning($"[ReminderToggleItem] LoadImage 실패: {path}");
            yield break;
        }
#else
        // 에디터 테스트용(Resources) 사용 안 하면 이 블록은 지워도 됨
        string fileName = Path.GetFileNameWithoutExtension(path);
        texture = Resources.Load<Texture2D>($"TestImages/{fileName}");
        if (texture == null)
        {
            Debug.LogWarning($"[ReminderToggleItem] 테스트 이미지 없음: {path}");
            yield break;
        }
#endif

        // 이미지 버튼 생성
        if (imagePrefab == null || imageGrid == null)
        {
            Debug.LogError("[ReminderToggleItem] imagePrefab 또는 imageGrid가 null");
            yield break;
        }

        GameObject imgObj = Instantiate(imagePrefab, imageGrid);

        Image img = imgObj.GetComponent<Image>();
        Button btn = imgObj.GetComponent<Button>();

        if (img == null)
        {
            Debug.LogError("[ReminderToggleItem] imagePrefab에 Image 컴포넌트가 없음");
            yield break;
        }
        if (btn == null)
        {
            Debug.LogError("[ReminderToggleItem] imagePrefab에 Button 컴포넌트가 없음");
            yield break;
        }

        // Sprite 생성 및 적용
        img.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
        img.preserveAspect = true;

        // 클릭 이벤트
        string capturedPath = path;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onImageClicked?.Invoke(capturedPath));

        yield return null;
    }
}
