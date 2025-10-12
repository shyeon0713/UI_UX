using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;


public class PickAndShowPhoto : MonoBehaviour
{
    [Header("UI refs")]
    public RawImage preview;     // 사진 표시
    public TextMeshProUGUI info;     // TextMeshPro(TextMeshProUGUI 포함)
   

    Texture2D currentTex;

    // 버튼 OnClick에 연결
    public void OnClickPickOne()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // EXIF의 GPS를 확실히 읽으려면 권장 (Manifest에도 선언되어 있어야 함)
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_MEDIA_LOCATION"))
            Permission.RequestUserPermission("android.permission.ACCESS_MEDIA_LOCATION");
#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        NativeGallery.GetImageFromGallery(path =>
        {
            if (string.IsNullOrEmpty(path)) return;
            StartCoroutine(LoadShowAndPrintExif(path));
        }, "사진을 선택하세요", "image/*");
#else
        Debug.LogWarning("Android/iOS 기기에서 실행해야 갤러리를 열 수 있습니다.");
#endif
    }

    System.Collections.IEnumerator LoadShowAndPrintExif(string path)
    {
        // 1) EXIF 읽기 (시간/좌표/방향)
        var (takenAt, lat, lon, orientation) = ExifReaderService.Read(path);

        // 2) 원본 이미지를 로드해서 화면에 표시 (+EXIF 방향 보정)
        byte[] bytes = File.ReadAllBytes(path);

        if (currentTex != null) Destroy(currentTex);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes, markNonReadable: false))
        {
            SetInfo("이미지 로드 실패");
            yield break;
        }
        tex = ApplyOrientation(tex, orientation);
        currentTex = tex;
        if (preview) preview.texture = tex;

        // 3) 화면용 EXIF 텍스트 구성
        var msg = BuildExifText(takenAt, lat, lon, orientation, path);
        SetInfo(msg);

        // 4) 콘솔에도 찍어두면 디버깅 편함
        Debug.Log(msg);

        yield return null;
    }

    string BuildExifText(DateTime? takenAt, double? lat, double? lon, int orientation, string path)
    {
        string timeLine = takenAt.HasValue
            ? $"촬영시각: {takenAt.Value.ToLocalTime():yyyy-MM-dd (ddd) HH:mm:ss}"
            : "촬영시각: (없음)";

        string gpsLine = (lat.HasValue && lon.HasValue)
            ? $"GPS: {lat.Value:F6}, {lon.Value:F6}"
            : "GPS: (없음)";

        string ori = orientation switch { 1 => "정상", 3 => "180°", 6 => "90°", 8 => "270°", _ => orientation.ToString() };
        string oriLine = $"Orientation: {ori}";

        string fileLine = $"파일: {Path.GetFileName(path)}";

        return $"{timeLine}\n{gpsLine}\n{oriLine}\n{fileLine}";
    }

    void SetInfo(string text)
    {
        if (info) info.text = text;

    }

    // ── EXIF 방향 보정(원본 텍스처 회전) ─────────────────────────────────────

    Texture2D ApplyOrientation(Texture2D src, int orientation)
    {
        return orientation switch
        {
            3 => Rotate(src, 180),
            6 => Rotate(src, 90),
            8 => Rotate(src, 270),
            _ => src
        };
    }

    Texture2D Rotate(Texture2D tex, int deg)
    {
        int w = tex.width, h = tex.height;
        Texture2D rot = (deg == 180) ? new Texture2D(w, h, tex.format, false)
                                     : new Texture2D(h, w, tex.format, false);

        for (int y = 0; y < rot.height; y++)
            for (int x = 0; x < rot.width; x++)
            {
                float u, v;
                if (deg == 180) { u = w - 1 - x; v = h - 1 - y; }
                else if (deg == 90) { u = y; v = w - 1 - x; }
                else { /*270*/ u = h - 1 - y; v = x; }

                rot.SetPixel(x, y, tex.GetPixel((int)u, (int)v));
            }
        rot.Apply();
        Destroy(tex);
        return rot;
    }
}
