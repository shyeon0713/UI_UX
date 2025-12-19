using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ReminderUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject reminderPanel;
    public Button closeButton;

    [Header("Toggle Item")]
    public GameObject toggleItemPrefab;  // ToggleItem 프리팹
    public Transform toggleContainer;    // ScrollView Content

    [Header("Image View")]
    public GameObject imageViewPanel;    // 이미지 확대 패널
    public Image fullscreenImage;        // 확대된 이미지
    public Button imageCloseButton;

    [Header("Map View")]
    public GameObject mapViewPanel;      // 지도 패널
    public RawImage mapImage;            // Google Maps 이미지
    public Button mapCloseButton;

    private DateTime currentDate;
    private List<string> currentImagePaths = new List<string>();

    void Start()
    {
        closeButton.onClick.AddListener(Close);
        imageCloseButton.onClick.AddListener(CloseImageView);
        mapCloseButton.onClick.AddListener(CloseMapView);

        reminderPanel.SetActive(false);
        imageViewPanel.SetActive(false);
        mapViewPanel.SetActive(false);
    }

    public void Open(DateTime date)
    {
        currentDate = date;
        reminderPanel.SetActive(true);

        // 권한 요청 후 데이터 로드
        MediaStoreManager.Instance.RequestPermission((granted) =>
        {
            if (granted)
            {
                LoadData(date);
            }
            else
            {
                Debug.LogError("[ReminderUI] 권한 거부됨");
            }
        });
    }

    private void LoadData(DateTime date)
    {
        // 기존 토글 아이템 제거
        foreach (Transform child in toggleContainer)
        {
            Destroy(child.gameObject);
        }

        // CSV에서 해당 날짜 지출 내역 가져오기
        var dayExpenditures = CSVReader.Instance.expenditure
            .FindAll(e => e.date.Date == date.Date);

        if (dayExpenditures.Count == 0)
        {
            Debug.Log($"[ReminderUI] {date:yyyy-MM-dd} 지출 내역 없음");
            return;
        }

        // MediaStore에서 이미지 가져오기
        currentImagePaths = MediaStoreManager.Instance.GetImagesByDate(date);
        Debug.Log($"[ReminderUI] 이미지 {currentImagePaths.Count}개 발견");

        // 각 지출 내역에 대해 토글 아이템 생성
        foreach (var expenditure in dayExpenditures)
        {
            GameObject item = Instantiate(toggleItemPrefab, toggleContainer);
            ReminderToggleItem toggleItem = item.GetComponent<ReminderToggleItem>();

            toggleItem.Setup(
                expenditure,
                currentImagePaths,
                OnImageClicked
            );
        }
    }

    // 이미지 클릭 시 확대 표시
    private void OnImageClicked(Texture2D texture, string imagePath)
    {
        imageViewPanel.SetActive(true);
        fullscreenImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        // 이미지에 클릭 이벤트 추가 (GPS → Map)
        Button imageButton = fullscreenImage.gameObject.GetComponent<Button>();
        if (imageButton == null)
            imageButton = fullscreenImage.gameObject.AddComponent<Button>();

        imageButton.onClick.RemoveAllListeners();
        imageButton.onClick.AddListener(() => ShowMapFromGPS(imagePath));
    }

    private void ShowMapFromGPS(string imagePath)
    {
        // TODO: EXIF에서 GPS 추출 후 Google Maps 표시
        Debug.Log($"[ReminderUI] GPS 지도 표시: {imagePath}");

        // 임시: 더미 좌표
        float latitude = 37.5665f;  // 서울
        float longitude = 126.9780f;

        StartCoroutine(LoadGoogleMapsImage(latitude, longitude));
    }

    private IEnumerator LoadGoogleMapsImage(float lat, float lon)
    {
        string apiKey = "YOUR_GOOGLE_MAPS_API_KEY";
        string url = $"https://maps.googleapis.com/maps/api/staticmap?" +
                     $"center={lat},{lon}&zoom=15&size=600x400&markers=color:red%7C{lat},{lon}" +
                     $"&key={apiKey}";

        UnityEngine.Networking.UnityWebRequest www =
            UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Texture2D mapTexture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
            mapImage.texture = mapTexture;
            mapViewPanel.SetActive(true);
        }
        else
        {
            Debug.LogError($"[ReminderUI] Map 로드 실패: {www.error}");
        }
    }

    public void Close()
    {
        reminderPanel.SetActive(false);
    }

    private void CloseImageView()
    {
        imageViewPanel.SetActive(false);
    }

    private void CloseMapView()
    {
        mapViewPanel.SetActive(false);
    }
}
