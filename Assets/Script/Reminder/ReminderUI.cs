using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ReminderUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject reminderPanel;
    public Button closeButton;

    [Header("Calendar")]
    public TMP_Text monthYearText;   // 상단에 월일 표시

    [Header("Toggle Item")]
    public GameObject toggleItemPrefab;  // ToggleItem 프리팹
    public Transform toggleContainer;    // Toggle Content

    [Header("Map View")]
    public GameObject mapViewPanel;
    public RawImage mapImage;
    public Button mapCloseButton;

    [Header("Other UI")]
    public GameObject calenderUI;  //  CalenderUI 참조

    private DateTime currentDate;
    private List<string> currentImagePaths = new List<string>();

    void Start()
    {
        closeButton.onClick.AddListener(Close);
        mapCloseButton.onClick.AddListener(CloseMapView);

        reminderPanel.SetActive(false);
        mapViewPanel.SetActive(false);
    }

    public void Open(DateTime date)
    {
        currentDate = date;

        // CalenderUI 비활성화
        if (calenderUI != null)
        {
            calenderUI.SetActive(false);
            Debug.Log($"[ReminderUI] CalenderUI 비활성화");
        }

        reminderPanel.SetActive(true);

        // 상단 년/월 표시
        monthYearText.text = $"{date.Year} {date.Month}월";

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
            .FindAll(e => e.date.Date == date.Date)
            .OrderBy(e => e.date)  // 시간순 정렬
            .ToList();


        if (dayExpenditures.Count == 0)
        {
            Debug.Log($"[ReminderUI] {date:yyyy-MM-dd} 지출 내역 없음");
            return;
        }

        // 총 소비 금액 계산
        int totalAmount = dayExpenditures.Sum(e => e.expendituredetails);

        // 첫 번째 가맹점명
        string firstStoreName = dayExpenditures[0].storename;


        // MediaStore에서 이미지 가져오기
        currentImagePaths = MediaStoreManager.Instance.GetImagesByDate(date);
        Debug.Log($"[ReminderUI] 이미지 {currentImagePaths.Count}개 발견");

        Debug.Log($"[ReminderUI] 토글 아이템 생성 시작");
        Debug.Log($"[ReminderUI] toggleItemPrefab: {toggleItemPrefab != null}");
        Debug.Log($"[ReminderUI] toggleContainer: {toggleContainer != null}");


        GameObject item = Instantiate(toggleItemPrefab, toggleContainer);

        if (item == null)
        {
            Debug.LogError($"[ReminderUI] 토글 아이템 생성 실패!");
            return;
        }

        ReminderToggleItem toggleItem = item.GetComponent<ReminderToggleItem>();

        if (toggleItem == null)
        {
            Debug.LogError($"[ReminderUI] ReminderToggleItem 컴포넌트 없음!");
            return;
        }

        toggleItem.Setup(
            date,
            totalAmount,
            firstStoreName,
            currentImagePaths,
            OnImageClicked
         );
    }

    // 이미지 클릭 시 확대 표시
    private void OnImageClicked(string imagePath)
    {
        Debug.Log($"[ReminderUI] 이미지 클릭: {imagePath}");

        // EXIF에서 GPS 추출
        var gps = ExtractGPSFromImage(imagePath);

        if (gps.HasValue)
        {
            ShowMapFromGPS(gps.Value.latitude, gps.Value.longitude);
        }
        else
        {
            Debug.LogWarning("[ReminderUI] GPS 정보 없음");
            // 더미 좌표 (테스트용)
            ShowMapFromGPS(37.5665f, 126.9780f);
        }
    }
    // EXIF GPS 추출 (간단 구현)
    private (float latitude, float longitude)? ExtractGPSFromImage(string imagePath)
    {
        // TODO: ExifLib 또는 MetadataExtractor 라이브러리 사용
        // 현재는 더미 데이터 반환
        Debug.Log($"[ReminderUI] GPS 추출 시도: {imagePath}");

        // 임시: 서울 좌표
        return (37.5665f, 126.9780f);
    }

    private void ShowMapFromGPS(float lat, float lon)
    {
        StartCoroutine(LoadGoogleMapsImage(lat, lon));
    }

    private IEnumerator LoadGoogleMapsImage(float lat, float lon)
    {
        string apiKey = "";  //AIzaSyDq0WtWg-wFqkANbAGso2Ku1mbRSqJeL4U
        //커밋할 때는 키 지우기
        int zoom = 15;
        int width = 600;
        int height = 400;

        string url = $"https://maps.googleapis.com/maps/api/staticmap?" +
                     $"center={lat},{lon}&zoom={zoom}&size={width}x{height}" +
                     $"&markers=color:red%7C{lat},{lon}" +
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

        //CalenderUI 다시 활성화
        if (calenderUI != null)
        {
            calenderUI.SetActive(true);
            Debug.Log($"[ReminderUI] CalenderUI 활성화");
        }
    }

    private void CloseMapView()
    {
        mapViewPanel.SetActive(false);
    }
}

