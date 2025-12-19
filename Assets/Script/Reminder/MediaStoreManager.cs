using System;
using System.Collections.Generic;
using UnityEngine;

public class MediaStoreManager : MonoBehaviour
{
    private static MediaStoreManager _instance;
    public static MediaStoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MediaStoreManager");
                _instance = go.AddComponent<MediaStoreManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject mediaStoreHelper;
#endif

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

#if UNITY_ANDROID && !UNITY_EDITOR
        mediaStoreHelper = new AndroidJavaObject("com.yourcompany.mediastore.MediaStoreHelper");
#endif
    }

    // 권한 요청
    public void RequestPermission(Action<bool> callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(
            UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(
                UnityEngine.Android.Permission.ExternalStorageRead);
            
            // 권한 결과 대기 코루틴
            StartCoroutine(WaitForPermission(callback));
        }
        else
        {
            callback?.Invoke(true);
        }
#else
        callback?.Invoke(true); // 에디터에서는 항상 true
#endif
    }

    private System.Collections.IEnumerator WaitForPermission(Action<bool> callback)
    {
        float timeout = 10f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.ExternalStorageRead))
            {
                callback?.Invoke(true);
                yield break;
            }
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        callback?.Invoke(false);
    }

    // 날짜별 이미지 가져오기
    public List<string> GetImagesByDate(DateTime date)
    {
        List<string> paths = new List<string>();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string dateString = date.ToString("yyyy-MM-dd");
            string result = mediaStoreHelper.CallStatic<string>("getImagesByDate", dateString);
            
            if (!string.IsNullOrEmpty(result))
            {
                paths.AddRange(result.Split('|'));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[MediaStore] Error: {e.Message}");
        }
#else
        // 에디터 테스트용 더미 데이터
        Debug.Log($"[MediaStore] Editor Mode - 날짜: {date:yyyy-MM-dd}");
        paths.Add("dummy_image_1.jpg");
        paths.Add("dummy_image_2.jpg");
#endif

        return paths;
    }
}