using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public static class PermissionService
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static int Sdk() => new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
#endif

    /// <summary>
    /// 갤러리 이미지 접근 + EXIF 위치 메타데이터 접근 권한을 순차적으로 요청.
    /// 모든 권한 허용 시 true, 하나라도 거부되면 false.
    /// </summary>
    public static void RequestForGalleryAndExif(Action<bool> onDone)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // OS 버전에 따라 필요한 권한 목록 구성
        var need = new List<string>();
        if (Sdk() >= 33) need.Add("android.permission.READ_MEDIA_IMAGES");
        else             need.Add("android.permission.READ_EXTERNAL_STORAGE");
        need.Add("android.permission.ACCESS_MEDIA_LOCATION");

        RequestSequential(need, 0, onDone);
#else
        onDone?.Invoke(true);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // 재귀적으로 한 개씩 요청하면서 진행
    static void RequestSequential(List<string> list, int index, Action<bool> onDone)
    {
        if (index >= list.Count) { onDone?.Invoke(true); return; }

        string perm = list[index];

        // 이미 허용됨 → 다음으로
        if (Permission.HasUserAuthorizedPermission(perm))
        {
            RequestSequential(list, index + 1, onDone);
            return;
        }

        var cb = new PermissionCallbacks();

        cb.PermissionGranted += grantedPerm =>
        {
            if (grantedPerm == perm)
                RequestSequential(list, index + 1, onDone); // 다음 권한 요청
        };

        cb.PermissionDenied += deniedPerm =>
        {
            if (deniedPerm == perm)
                onDone?.Invoke(false); // 거부
        };

        cb.PermissionDeniedAndDontAskAgain += deniedPerm =>
        {
            if (deniedPerm == perm)
                onDone?.Invoke(false); // 다시 묻지 않기까지 체크
        };

        Permission.RequestUserPermission(perm, cb);
    }
#endif
}
